using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SSF.Interop.SIIFNacion.Application.Common.Auditoria;
using SSF.Interop.SIIFNacion.Application.Common.Interfaces.ExternalServices.SIIF;
using SSF.Interop.SIIFNacion.Application.Contracts.Persistence;
using SSF.Interop.SIIFNacion.Application.DTOs.External.SIIF.Common;
using SSF.Interop.SIIFNacion.Application.DTOs.External.SIIF.Contracts;
using SSF.Interop.SIIFNacion.Application.DTOs.External.SIIF.Response;
using SSF.Interop.SIIFNacion.Domain.Cdp;

namespace SSF.Interop.SIIFNacion.Application.Features.SIIF.Requests.Commands.CoordinadorSincronizacionRp
{
    /// <summary>
    /// 📋 Handler: Orquesta la consulta masiva paginada de compromisos RP para un año dado.
    ///
    /// Lógica:
    /// 1. Hardcodea la configuración (PCI, TipoGasto, Rango, Vigencia, paginación).
    /// 2. Formatea las fechas usando el año recibido: [AÑO]-01-15 a [AÑO]-01-31.
    /// 3. Itera automáticamente las páginas de SIIF hasta agotar resultados.
    /// 4. Filtra registros cuya NombreRazonSocial sea exactamente:
    ///    "MINISTERIO DEL TRABAJO - SUPERINTENDENCIA DE SUBSIDIO FAMILIAR".
    /// 5. Persiste con Upsert (llave de control) evitando duplicados.
    /// 6. Retorna un resumen de registros procesados vs guardados.
    /// </summary>
    public sealed class CoordinarSincronizacionRpCommandHandler
        : IRequestHandler<CoordinarSincronizacionRpCommand, CoordinarSincronizacionRpResponseDto>
    {
        // ── Constantes hardcodeadas por requerimiento ──────────────────────────────────────
        private const string PciConsulta    = "36-01-07";
        private const string TipoGasto      = "Todos";
        private const string Rango          = "Todos";
        private const string Vigencia       = "1";
        private const int    PageSize       = 50;

        private const string EntidadFiltro  =
            "MINISTERIO DEL TRABAJO - SUPERINTENDENCIA DE SUBSIDIO FAMILIAR";

        private readonly ISiifBudgetService       _siifBudgetService;
        private readonly IDynCompromPaginRepository _repository;
        private readonly IAuditoriaLogger         _audit;

        public CoordinarSincronizacionRpCommandHandler(
            ISiifBudgetService siifBudgetService,
            IDynCompromPaginRepository repository,
            IAuditoriaLogger audit)
        {
            _siifBudgetService = siifBudgetService;
            _repository        = repository;
            _audit             = audit;
        }

        public async Task<CoordinarSincronizacionRpResponseDto> Handle(
            CoordinarSincronizacionRpCommand request,
            CancellationToken cancellationToken)
        {
            var idTramite = Guid.NewGuid().ToString();
            var usuario   = request.LoginUsuarioSiifHeader;
            var totalSw   = Stopwatch.StartNew();

            // ── 1. Validar año ──────────────────────────────────────────────────────────
            if (request.Anio < 2000 || request.Anio > 2100)
            {
                return new CoordinarSincronizacionRpResponseDto
                {
                    Exito   = false,
                    Mensaje = $"El año '{request.Anio}' no es válido. Debe estar entre 2000 y 2100."
                };
            }

            // ── 2. Construir fechas y parámetros fijos ─────────────────────────────────
            var fechaInicio = $"{request.Anio}-01-15";
            var fechaFin    = $"{request.Anio}-01-31";

            await _audit.InfoAsync(
                idTramite,
                SiifPuntoDeControl.InicioFlujo,
                $"🚀 Inicio CoordinarSincronizacionRp. Anio={request.Anio}, " +
                $"FechaInicio={fechaInicio}, FechaFin={fechaFin}, PCI={PciConsulta}, " +
                $"Vigencia={Vigencia}.",
                peticion: null, respuesta: null, tiempoRq: null, tiempoRs: null,
                estadoFinal: null, nmUserUpdate: usuario, cancellationToken);

            var headers = new SiifRequestHeaderDto
            {
                CodPci           = request.CodPciHeader,
                LoginUsuarioSiif = request.LoginUsuarioSiifHeader,
                Consecutivo      = request.ConsecutivoHeader,
                Hash             = request.HashHeader
            };

            // ── 3. Bucle de paginación automática ──────────────────────────────────────
            var todosLosRegistros = new List<ListaCompromisoItemDto>();
            int currentPage       = 1;
            bool hasMorePages     = true;
            var siifSw            = Stopwatch.StartNew();

            try
            {
                while (hasMorePages)
                {
                    var body = new ConsultaListaCompromisoPaginadaRequestDto
                    {
                        PaginationDto = new PaginationDto
                        {
                            Page = currentPage,
                            Size = PageSize
                        },
                        ConsultaCompromiso = new ConsultaCompromisoFiltroDto
                        {
                            PCI         = PciConsulta,
                            FechaInicio = fechaInicio,
                            FechaFin    = fechaFin,
                            TipoGasto   = TipoGasto,
                            Rango       = Rango,
                            Vigencia    = Vigencia
                        }
                    };

                    await _audit.InfoAsync(
                        idTramite,
                        SiifPuntoDeControl.EnvioRequestSiif,
                        $"📤 Consultando página {currentPage} a SIIF (Size={PageSize}).",
                        peticion: JsonSerializer.Serialize(body), respuesta: null,
                        tiempoRq: null, tiempoRs: null, estadoFinal: null,
                        nmUserUpdate: usuario, cancellationToken);

                    var response = await _siifBudgetService
                        .ConsultarListaCompromisoPaginadaAsync(body, headers, cancellationToken);

                    var items = response?.consultaCompromisoSal;

                    if (items != null && items.Count > 0)
                    {
                        todosLosRegistros.AddRange(items);

                        // Si la página devuelve menos de PageSize, no hay más páginas
                        hasMorePages = items.Count == PageSize;
                        currentPage++;
                    }
                    else
                    {
                        hasMorePages = false;
                    }
                }

                siifSw.Stop();
            }
            catch (Exception ex)
            {
                siifSw.Stop();
                totalSw.Stop();

                await _audit.ErrorAsync(
                    idTramite,
                    SiifPuntoDeControl.FinError,
                    $"💥 Error consultando SIIF. {ex.Message}",
                    peticion: null, respuesta: null,
                    tiempoRq: null, tiempoRs: $"{totalSw.ElapsedMilliseconds} ms",
                    estadoFinal: SiifAuditoriaEstadoFinal.Fallido,
                    nmUserUpdate: usuario, exception: ex, cancellationToken);

                return new CoordinarSincronizacionRpResponseDto
                {
                    Exito   = false,
                    Mensaje = $"Error al consultar SIIF: {ex.Message}",
                    RegistrosProcesadosTotales = todosLosRegistros.Count
                };
            }

            await _audit.InfoAsync(
                idTramite,
                SiifPuntoDeControl.RespuestaSiifRecibida,
                $"📥 Páginas agotadas. Total registros traídos de SIIF = {todosLosRegistros.Count}.",
                peticion: null, respuesta: null,
                tiempoRq: null, tiempoRs: $"{siifSw.ElapsedMilliseconds} ms",
                estadoFinal: null, nmUserUpdate: usuario, cancellationToken);

            // ── 4. Generar estructura Compromiso Actual y Reserva por cada item ─────────
            // Punto de extensión: aquí se conectarán los servicios de Compromiso Actual y Reserva.
            // Por ahora se prepara la lista enriquecida lista para persistir.
            var itemsEnriquecidos = EnriquecerConCompromisoYReserva(todosLosRegistros);

            // ── 5. Filtrar por entidad exacta ──────────────────────────────────────────
            var registrosFiltrados = itemsEnriquecidos
                .Where(i => string.Equals(
                    i.NombreRazonSocial?.Trim(),
                    EntidadFiltro,
                    StringComparison.OrdinalIgnoreCase))
                .ToList();

            await _audit.InfoAsync(
                idTramite,
                SiifPuntoDeControl.ValidacionRespuesta,
                $"🔍 Filtrado por entidad '{EntidadFiltro}': " +
                $"{registrosFiltrados.Count} de {todosLosRegistros.Count} registros califican.",
                peticion: null, respuesta: null,
                tiempoRq: null, tiempoRs: null,
                estadoFinal: null, nmUserUpdate: usuario, cancellationToken);

            // ── 6. Persistir solo los registros filtrados (Upsert con llave de control) ─
            int registrosGuardados = 0;

            if (registrosFiltrados.Count > 0)
            {
                var entidades = MapToEntities(registrosFiltrados, Vigencia);

                var swInsert = Stopwatch.StartNew();
                await _repository.UpsertCompromisosAsync(entidades, cancellationToken);
                swInsert.Stop();

                registrosGuardados = registrosFiltrados.Count;

                await _audit.InfoAsync(
                    idTramite,
                    SiifPuntoDeControl.InsercionBd,
                    $"💾 Upsert BD completado. Guardados={registrosGuardados}, " +
                    $"ms={swInsert.ElapsedMilliseconds}.",
                    peticion: null, respuesta: null,
                    tiempoRq: null, tiempoRs: $"{swInsert.ElapsedMilliseconds} ms",
                    estadoFinal: null, nmUserUpdate: usuario, cancellationToken);
            }
            else
            {
                await _audit.InfoAsync(
                    idTramite,
                    SiifPuntoDeControl.ValidacionRespuesta,
                    "📭 Ningún registro coincide con la entidad filtro. No se persiste nada.",
                    peticion: null, respuesta: null,
                    tiempoRq: null, tiempoRs: null,
                    estadoFinal: null, nmUserUpdate: usuario, cancellationToken);
            }

            // ── 7. Fin exitoso ──────────────────────────────────────────────────────────
            totalSw.Stop();

            await _audit.InfoAsync(
                idTramite,
                SiifPuntoDeControl.FinExitoso,
                $"✅ CoordinarSincronizacionRp completado. " +
                $"Procesados={todosLosRegistros.Count}, Guardados={registrosGuardados}, " +
                $"msTotal={totalSw.ElapsedMilliseconds}.",
                peticion: null, respuesta: null,
                tiempoRq: null, tiempoRs: $"{totalSw.ElapsedMilliseconds} ms",
                estadoFinal: SiifAuditoriaEstadoFinal.Exitoso,
                nmUserUpdate: usuario, cancellationToken);

            return new CoordinarSincronizacionRpResponseDto
            {
                Exito                      = true,
                Mensaje                    = "Sincronización completada exitosamente.",
                RegistrosProcesadosTotales = todosLosRegistros.Count,
                RegistrosGuardados         = registrosGuardados
            };
        }

        // ── Punto de extensión: Compromiso Actual y Reserva ────────────────────────────────
        /// <summary>
        /// 🔧 Estructura preparada para integrar los servicios de "Compromiso Actual" y "Reserva".
        /// Por ahora retorna la lista original sin modificaciones adicionales.
        /// Aquí se debe implementar la consulta al servicio de Compromiso cuando esté disponible.
        /// </summary>
        private static List<ListaCompromisoItemDto> EnriquecerConCompromisoYReserva(
            List<ListaCompromisoItemDto> items)
        {
            // TODO: Por cada item, llamar al servicio de Compromiso Actual (ConsultarCompromisoPptalAsync)
            // y al de Reserva para enriquecer la entidad antes de persistir.
            //
            // Ejemplo del contrato futuro:
            // foreach (var item in items)
            // {
            //     var compromisoActual = await _siifBudgetService.ConsultarCompromisoPptalAsync(...);
            //     var reserva = await _siifBudgetService.ConsultarReservaAsync(...);
            //     item.CompromisoActual = compromisoActual;
            //     item.Reserva = reserva;
            // }
            return items;
        }

        // ── Mapper ─────────────────────────────────────────────────────────────────────────
        private static IEnumerable<DynTblCompromPagin> MapToEntities(
            IEnumerable<ListaCompromisoItemDto> items,
            string vigencia)
        {
            var ahora = DateTime.Now;
            var bnNow = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            foreach (var item in items)
            {
                yield return new DynTblCompromPagin
                {
                    Oid       = Guid.NewGuid().ToString("N"),
                    NrVersion = 1,
                    BnCreated = bnNow,
                    FgEnabled = 1,
                    FgSystem  = 0,
                    BnUpdated = 0,

                    IdCompromiso   = $"{item.CodigoPCI}-{item.CodigoCompromiso}",
                    IdPci          = ParseDecimalNullable(item.IdPCI?.ToString()),
                    DescripcionPci = item.DescripcionPCI,
                    CodCompromiso  = ParseDecimalNullable(item.CodigoCompromiso),
                    VigenciaCod    = vigencia,
                    VigenciaNm     = null,

                    FechaRegistro = ParseFechaNullable(item.FechaRegistro),
                    FechaCreacion = ParseFechaNullable(item.FechaCreacion),
                    Estado        = item.Estado,

                    CodDependencia = item.CodigoDependencia,
                    DescripcionDep = item.DescripcionDependencia,
                    CodPGastos     = item.CodigoPosicionGastos,
                    DesPGastos     = item.DescripcionPosicionGastos,
                    CodFuente      = item.CodigoFuente,
                    Fuente         = item.Fuente,
                    CodSituacion   = item.CodigoSituacion,
                    Situacion      = item.Situacion,
                    CodRecurso     = item.CodigoRecurso,
                    Recursos       = item.Recursos,

                    VlInicial     = ParseMonedaNullable(item.ValorInicial),
                    VlOperaciones = ParseMonedaNullable(item.ValorOperaciones),
                    VlActual      = ParseMonedaNullable(item.ValorActual),
                    SaldoUtilizar = ParseMonedaNullable(item.SaldoUtilizar),

                    TipoIdentificac = item.TipoIdentificacion,
                    NnIdentificacio = item.NumeroIdentificacion,
                    NmRazonSocial   = item.NombreRazonSocial,

                    MedioPago    = item.MedioPago,
                    TipoCuenta   = item.TipoCuenta,
                    NnCuenta     = item.NumeroCuenta,
                    EstadoCuenta = item.EstadoCuenta,
                    NitEntFinan  = item.NitEntidadFinanciera,
                    DesEntFinan  = item.DescripcionEntidadFinanciera,

                    CodCdp       = ParseDecimalNullable(item.CDP),
                    CuentasPagar = item.CuentasPagar,
                    Obligaciones = item.Obligaciones,
                    OrdenesPago  = item.OrdenesPago,
                    Reintegros   = item.Reintegros,

                    FechaDocSoporte = ParseFechaNullable(item.FechaDocumentoSoporte),
                    TipoDocSoporte  = item.TipoDocumentoSoporte,
                    NumeroDocSoport = item.NumeroDocumentoSoporte,
                    Observaciones   = item.Observaciones,

                    FechaCarga   = ahora,
                    AnioVigencia = vigencia
                };
            }
        }

        // ── Helpers de parseo ──────────────────────────────────────────────────────────────
        private static decimal? ParseMonedaNullable(string? valor)
        {
            if (string.IsNullOrWhiteSpace(valor)) return null;
            var limpio = valor.Trim().Replace(".", "").Replace(",", ".");
            return decimal.TryParse(limpio, NumberStyles.Any,
                                    CultureInfo.InvariantCulture, out var r) ? r : null;
        }

        private static decimal? ParseDecimalNullable(string? valor)
        {
            if (string.IsNullOrWhiteSpace(valor)) return null;
            return decimal.TryParse(valor.Trim(), NumberStyles.Any,
                                    CultureInfo.InvariantCulture, out var r) ? r : null;
        }

        private static DateTime? ParseFechaNullable(string? valor)
        {
            if (string.IsNullOrWhiteSpace(valor)) return null;
            string[] formatos =
            {
                "yyyy-MM-dd", "dd/MM/yyyy", "dd-MM-yyyy",
                "yyyy-MM-ddTHH:mm:ss", "dd/MM/yyyy HH:mm:ss"
            };
            return DateTime.TryParseExact(valor.Trim(), formatos,
                                          CultureInfo.InvariantCulture,
                                          DateTimeStyles.None, out var f) ? f : null;
        }
    }
}
