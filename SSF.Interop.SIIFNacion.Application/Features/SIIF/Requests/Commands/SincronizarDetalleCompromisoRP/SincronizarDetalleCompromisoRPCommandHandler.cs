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
using SSF.Interop.SIIFNacion.Application.DTOs.External.SIIF.Response.Contracts;
using SSF.Interop.SIIFNacion.Domain.Cdp;

namespace SSF.Interop.SIIFNacion.Application.Features.SIIF.Requests.Commands.SincronizarDetalleCompromisoRP
{
    /// <summary>
    /// Handler principal de la HU 6 — Sincronización completa de Registros Presupuestales.
    ///
    /// Estrategia: Wipe &amp; Load
    ///
    /// FASE 1 — Maestro (Lista Compromiso):
    ///   1.1 Limpia DYNTBLCOMPROMPAGIN y DYNTBLCCOMPPTAL para la vigencia indicada.
    ///   1.2 Consulta el servicio lista_compromiso de SIIF de forma paginada.
    ///   1.3 Persiste todos los RPs resultantes en DYNTBLCOMPROMPAGIN.
    ///
    /// FASE 2 — Detalle (Consulta Compromiso Presupuestal):
    ///   2.1 Itera cada registro en DYNTBLCOMPROMPAGIN.
    ///   2.2 Por cada RP, llama a consulta_compromiso_pptal usando (Vigencia + CodCompromiso).
    ///   2.3 Aplica limpieza de datos (codigo_soporte sin puntos, numero_documento entero).
    ///   2.4 Inserta el detalle en DYNTBLCCOMPPTAL.
    ///   2.5 Si un RP individual falla, registra el error y continúa con el siguiente.
    /// </summary>
    public class SincronizarDetalleCompromisoRPCommandHandler
        : IRequestHandler<SincronizarDetalleCompromisoRPCommand, SincronizarDetalleCompromisoRPResponseDto>
    {
        // ── Constantes de consulta (hardcodeadas por requerimiento del servicio paginado) ──
        private const string PciConsulta = "36-01-07";
        private const string TipoGasto   = "Todos";
        private const string Rango       = "Todos";
        private const int    PageSize    = 50;
        private const string FechaInicio = "2026-01-15";
        private const string FechaFin    = "2026-01-31";

        private readonly ISiifBudgetService         _siifBudgetService;
        private readonly IDynCompromPaginRepository  _paginRepository;
        private readonly IDynCompromisoRepository    _detailRepository;
        private readonly IAuditoriaLogger            _audit;

        public SincronizarDetalleCompromisoRPCommandHandler(
            ISiifBudgetService siifBudgetService,
            IDynCompromPaginRepository paginRepository,
            IDynCompromisoRepository detailRepository,
            IAuditoriaLogger audit)
        {
            _siifBudgetService = siifBudgetService;
            _paginRepository   = paginRepository;
            _detailRepository  = detailRepository;
            _audit             = audit;
        }

        public async Task<SincronizarDetalleCompromisoRPResponseDto> Handle(
            SincronizarDetalleCompromisoRPCommand request,
            CancellationToken cancellationToken)
        {
            var idTramite = Guid.NewGuid().ToString();
            var usuario   = request.LoginUsuarioSiifHeader;
            var totalSw   = Stopwatch.StartNew();
            var result    = new SincronizarDetalleCompromisoRPResponseDto();

            // Regla de Vigencia: "1" → "Actual" | "2" → "Reserva Presupuestal"
            var vigenciaSiif = request.Vigencia == "1" ? "Actual" : "Reserva Presupuestal";

            var headers = new SiifRequestHeaderDto
            {
                CodPci           = request.CodPciHeader,
                LoginUsuarioSiif = request.LoginUsuarioSiifHeader,
                Consecutivo      = request.ConsecutivoHeader,
                Hash             = request.HashHeader
            };

            await _audit.InfoAsync(idTramite, SiifPuntoDeControl.InicioFlujo,
                $"🚀 [HU6] Inicio Wipe & Load. Vigencia={request.Vigencia} ({vigenciaSiif}).",
                null, null, null, null, null, usuario, cancellationToken);

            // ════════════════════════════════════════════════════════════════════
            // FASE 1 — PASO 1.1: Limpieza (Wipe)
            // ════════════════════════════════════════════════════════════════════
            try
            {
                await _audit.InfoAsync(idTramite, SiifPuntoDeControl.InicioFlujo,
                    $"🗑️ [Fase 1] Iniciando limpieza de tablas para Vigencia={request.Vigencia}.",
                    null, null, null, null, null, usuario, cancellationToken);

                await _detailRepository.WipeByVigenciaAsync(request.Vigencia, cancellationToken);
                await _paginRepository.DeleteByAnioVigenciaAsync(request.Vigencia, cancellationToken);

                await _audit.InfoAsync(idTramite, SiifPuntoDeControl.InsercionBd,
                    "✅ [Fase 1] Limpieza completada — DYNTBLCCOMPPTAL y DYNTBLCOMPROMPAGIN vaciadas.",
                    null, null, null, null, null, usuario, cancellationToken);
            }
            catch (Exception ex)
            {
                await _audit.ErrorAsync(idTramite, SiifPuntoDeControl.FinError,
                    $"💥 [Fase 1] Error crítico en limpieza: {ex.Message}",
                    null, null, null, null, SiifAuditoriaEstadoFinal.Fallido, usuario, ex, cancellationToken);

                result.Estado  = "ERROR";
                result.Mensaje = $"Error en Wipe de BD: {ex.Message}";
                return result;
            }

            // ════════════════════════════════════════════════════════════════════
            // FASE 1 — PASO 1.2: Consulta lista paginada a SIIF (Load → DYNTBLCOMPROMPAGIN)
            // ════════════════════════════════════════════════════════════════════
            var todosLosRps = new List<ListaCompromisoItemDto>();
            int currentPage = 1;
            bool hasMorePages = true;
            var sw1 = Stopwatch.StartNew();

            try
            {
                while (hasMorePages)
                {
                    var body = new ConsultaListaCompromisoPaginadaRequestDto
                    {
                        PaginationDto = new PaginationDto { Page = currentPage, Size = PageSize },
                        ConsultaCompromiso = new ConsultaCompromisoFiltroDto
                        {
                            PCI         = PciConsulta,
                            FechaInicio = FechaInicio,
                            FechaFin    = FechaFin,
                            TipoGasto   = TipoGasto,
                            Rango       = Rango,
                            Vigencia    = request.Vigencia
                        }
                    };

                    await _audit.InfoAsync(idTramite, SiifPuntoDeControl.EnvioRequestSiif,
                        $"📤 [Fase 1] Consultando página {currentPage} (Size={PageSize}) a SIIF.",
                        JsonSerializer.Serialize(body), null, null, null, null, usuario, cancellationToken);

                    var response = await _siifBudgetService
                        .ConsultarListaCompromisoPaginadaAsync(body, headers, cancellationToken);

                    var items = response?.consultaCompromisoSal;

                    if (items != null && items.Count > 0)
                    {
                        todosLosRps.AddRange(items);
                        hasMorePages = items.Count == PageSize;
                        currentPage++;
                    }
                    else
                    {
                        hasMorePages = false;
                    }
                }
            }
            catch (Exception ex)
            {
                sw1.Stop();
                await _audit.ErrorAsync(idTramite, SiifPuntoDeControl.FinError,
                    $"💥 [Fase 1] Error consultando lista paginada SIIF: {ex.Message}",
                    null, null, null, null, SiifAuditoriaEstadoFinal.Fallido, usuario, ex, cancellationToken);

                result.Estado  = "ERROR";
                result.Mensaje = $"Error en consulta paginada SIIF: {ex.Message}";
                return result;
            }

            sw1.Stop();

            await _audit.InfoAsync(idTramite, SiifPuntoDeControl.RespuestaSiifRecibida,
                $"📥 [Fase 1] Total RPs obtenidos de SIIF: {todosLosRps.Count}. Tiempo: {sw1.ElapsedMilliseconds}ms.",
                null, null, null, null, null, usuario, cancellationToken);

            if (!todosLosRps.Any())
            {
                result.Mensaje = "SIIF no devolvió registros para la vigencia indicada.";
                await _audit.InfoAsync(idTramite, SiifPuntoDeControl.FinExitoso, result.Mensaje,
                    null, null, null, null, SiifAuditoriaEstadoFinal.Exitoso, usuario, cancellationToken);
                return result;
            }

            // ── Persiste el Maestro en DYNTBLCOMPROMPAGIN ──
            try
            {
                var entidadesPagin = MapToPaginEntities(todosLosRps, request.Vigencia);
                await _paginRepository.SaveRangeAsync(entidadesPagin, cancellationToken);

                await _audit.InfoAsync(idTramite, SiifPuntoDeControl.InsercionBd,
                    $"💾 [Fase 1] {todosLosRps.Count} registros guardados en DYNTBLCOMPROMPAGIN.",
                    null, null, null, null, null, usuario, cancellationToken);
            }
            catch (Exception ex)
            {
                await _audit.ErrorAsync(idTramite, SiifPuntoDeControl.FinError,
                    $"💥 [Fase 1] Error guardando maestro en BD: {ex.Message}",
                    null, null, null, null, SiifAuditoriaEstadoFinal.Fallido, usuario, ex, cancellationToken);

                result.Estado  = "ERROR";
                result.Mensaje = $"Error guardando maestro: {ex.Message}";
                return result;
            }

            // ════════════════════════════════════════════════════════════════════
            // FASE 2 — Iteración detalle por RP (Load → DYNTBLCCOMPPTAL)
            // ════════════════════════════════════════════════════════════════════
            await _audit.InfoAsync(idTramite, SiifPuntoDeControl.InicioFlujo,
                $"🔄 [Fase 2] Iniciando iteración de detalle para {todosLosRps.Count} RPs.",
                null, null, null, null, null, usuario, cancellationToken);

            int exitosos = 0;
            int fallidos = 0;

            foreach (var rp in todosLosRps)
            {
                // Omitir registros sin código válido
                if (string.IsNullOrWhiteSpace(rp.CodigoCompromiso) ||
                    !int.TryParse(rp.CodigoCompromiso.Trim(), out var codRp) ||
                    codRp <= 0)
                {
                    fallidos++;
                    await _audit.WarnAsync(idTramite, SiifPuntoDeControl.ValidacionRespuesta,
                        $"⚠️ [Fase 2] RP omitido — CódigoCompromiso inválido: '{rp.CodigoCompromiso}'.",
                        null, null, null, null, SiifAuditoriaEstadoFinal.Parcial, usuario, cancellationToken);
                    continue;
                }

                try
                {
                    // ── Construir body de detalle usando Vigencia + id_compromiso ──
                    var bodyDetalle = new ConsultarCompromisoRequestDto
                    {
                        Pci                   = PciConsulta,
                        CodCompromisoPptalGastos = codRp,
                        Vigencia              = vigenciaSiif
                    };

                    var detalle = await _siifBudgetService
                        .ConsultarCompromisoPptalAsync(bodyDetalle, headers, cancellationToken);

                    if (detalle == null || detalle.Codigo <= 0)
                    {
                        fallidos++;
                        await _audit.WarnAsync(idTramite, SiifPuntoDeControl.ValidacionRespuesta,
                            $"⚠️ [Fase 2] RP {codRp} no encontrado en SIIF o respuesta vacía.",
                            null, null, null, null, SiifAuditoriaEstadoFinal.Parcial, usuario, cancellationToken);
                        continue;
                    }

                    // ── Mapear con limpieza de datos ──
                    var entidadDetalle = MapToDetailEntity(detalle, request.Vigencia);

                    // ── Persistencia (INSERT directo — tabla ya fue limpiada en Fase 1) ──
                    entidadDetalle.Oid        = Guid.NewGuid().ToString("N").ToUpperInvariant();
                    entidadDetalle.BnCreated  = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                    entidadDetalle.NrVersion  = 1;
                    entidadDetalle.FgEnabled  = 1;
                    entidadDetalle.FgSystem   = 0;

                    await _detailRepository.SaveAsync(entidadDetalle, cancellationToken);
                    exitosos++;

                    await _audit.InfoAsync(idTramite, SiifPuntoDeControl.InsercionBd,
                        $"✅ [Fase 2] RP {codRp} insertado en DYNTBLCCOMPPTAL.",
                        null, null, null, null, null, usuario, cancellationToken);
                }
                catch (Exception ex)
                {
                    // Error en un RP individual → registrar y CONTINUAR con el siguiente
                    fallidos++;
                    await _audit.ErrorAsync(idTramite, SiifPuntoDeControl.FinError,
                        $"❌ [Fase 2] Error procesando RP {codRp}: {ex.Message}",
                        null, null, null, null, SiifAuditoriaEstadoFinal.Parcial, usuario, ex, cancellationToken);
                }
            }

            // ── Resultado final ──
            totalSw.Stop();
            result.Estado                    = fallidos == 0 ? "OK" : "PARCIAL";
            result.CantidadRegistrosActualizados = exitosos;
            result.Mensaje = $"Proceso completado. Exitosos: {exitosos} | Fallidos: {fallidos} " +
                             $"de {todosLosRps.Count} RPs. Tiempo total: {totalSw.ElapsedMilliseconds}ms.";

            await _audit.InfoAsync(idTramite, SiifPuntoDeControl.FinExitoso, result.Mensaje,
                null, null, null, $"{totalSw.ElapsedMilliseconds}ms",
                exitosos == todosLosRps.Count ? SiifAuditoriaEstadoFinal.Exitoso : SiifAuditoriaEstadoFinal.Parcial,
                usuario, cancellationToken);

            return result;
        }

        // ════════════════════════════════════════════════════════════════════════
        // MAPPER — ListaCompromisoItemDto → DynTblCompromPagin (Maestro)
        // ════════════════════════════════════════════════════════════════════════
        private static IEnumerable<DynTblCompromPagin> MapToPaginEntities(
            IEnumerable<ListaCompromisoItemDto> items, string vigencia)
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

        // ════════════════════════════════════════════════════════════════════════
        // MAPPER — ConsultarCompromisoResponseDto → DynTblCCompPtal (Detalle)
        // Incluye limpieza de campos según requerimiento:
        //   • NnDocSoporte (codigo_soporte): sin puntos
        //   • TnDocumento  (numero_documento): entero limpio
        // ════════════════════════════════════════════════════════════════════════
        private static DynTblCCompPtal MapToDetailEntity(
            ConsultarCompromisoResponseDto siif, string vigencia)
        {
            var ahora = DateTime.Now;

            return new DynTblCCompPtal
            {
                IdCompromiso  = siif.Codigo,
                VigenciaNm    = siif.Vigencia,
                FechaRegistro = siif.FechaRegistro,
                Estado        = siif.Estado,
                CodCdp        = siif.CodigoCdp,
                FechaCdp      = siif.FechaCdp,
                CodMoneda     = siif.CodigoMoneda,
                NmMoneda      = siif.NombreMoneda,
                ValorTasa     = siif.ValorTasa,
                Descripcion   = siif.Descripcion,
                Objeto        = siif.Objeto,

                ValorInicial   = siif.ValorInicial,
                VlIniOriMoneda = siif.ValorInicialOriginalMoneda,
                VlTOperacion   = siif.ValorTotalOperacion,
                ValorActual    = siif.ValorActual,
                SaldoXObligar  = siif.SaldoPorObligar,
                SaldoMoneda    = siif.SaldoMoneda,

                TtDocumento = siif.Tercero?.TipoDocumento,
                // ✅ numero_documento: parsear como decimal y truncar a entero limpio
                TnDocumento = LimpiarNumeroDocumento(siif.Tercero?.NumeroDocumento),
                TerceroNm   = siif.Tercero?.Nombre,
                MedioPago   = siif.MedioPago,

                CuentaNn      = siif.DetalleCuentaBancaria?.Numero,
                CuentaEntFinan = siif.DetalleCuentaBancaria?.EntidadFinanciera,
                CuentaTipo    = siif.DetalleCuentaBancaria?.TipoCuenta,
                CuentaEstado  = siif.DetalleCuentaBancaria?.Estado,

                OrdenadorTDoc  = siif.DetalleOrdenadorGasto?.TipoDocumento,
                OrdenadorNDoc  = siif.DetalleOrdenadorGasto?.NumeroDocumento,
                OrdenadorNm    = siif.DetalleOrdenadorGasto?.Nombre,
                OrdenadorConsec = siif.DetalleOrdenadorGasto?.Consecutivo,
                OrdenadorCodCar = siif.DetalleOrdenadorGasto?.CodigoCargo,
                OrdenadorNmCarg = siif.DetalleOrdenadorGasto?.NombreCargo,

                // ✅ codigo_soporte: guardar sin puntos (separadores de miles)
                NnDocSoporte = LimpiarCodigoSoporte(siif.DatosAdministrativos?.NumeroDocumentoSoporte),
                TDocSoporte  = siif.DatosAdministrativos?.TipoDocumentoSoporte,
                DtDocSoporte = siif.DatosAdministrativos?.Fecha,
                CajaMenor    = siif.CajaMenor?.ToString(),

                FechaCarga   = ahora,
                AnioVigencia = vigencia,

                Items = siif.ListadoItemsAfectacion.Select(x => new DynTblListItemsAfe
                {
                    Oid       = Guid.NewGuid().ToString("N").ToUpperInvariant(),
                    NrVersion = 1,
                    BnCreated = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    FgEnabled = 1,
                    FgSystem  = 0,

                    IdCompromiso = (decimal?)siif.Codigo,
                    CodDepAfecta = x.CodigoDependenciaAfectacion,
                    NmDepAfecta  = x.NombreDependenciaAfectacion,
                    CodPGasto    = x.CodigoPosicionGasto,
                    NmPGasto     = x.NombrePosicionGasto,
                    CodFFinan    = x.CodigoFuenteFinanciacion,
                    NmFFinan     = x.NombreFuenteFinanciacion,
                    CodRPPtal    = x.CodigoRecursoPresupuestal,
                    NmRPPtal     = x.NombreRecursoPresupuestal,
                    CodSFondo    = x.CodigoSituacionFondos,
                    NmSFondo     = x.NombreSituacionFondos,
                    VlInicial    = x.ValorInicial,
                    VlOperaciones = x.ValorOperaciones,
                    VlActual     = x.ValorActual,
                    Saldo        = x.Saldo,
                    FechaCarga   = ahora,
                    AnioVigencia = vigencia
                }).ToList()
            };
        }

        // ════════════════════════════════════════════════════════════════════════
        // HELPERS DE LIMPIEZA DE DATOS
        // ════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Limpia codigo_soporte: elimina todos los puntos del valor.
        /// Ej: "3.050.123" → "3050123"
        /// </summary>
        private static string? LimpiarCodigoSoporte(string? valor)
        {
            if (string.IsNullOrWhiteSpace(valor)) return null;
            return valor.Replace(".", "").Trim();
        }

        /// <summary>
        /// Limpia numero_documento: parsea como decimal y trunca a entero.
        /// Garantiza que "3050.00" o "3.050" se guarden como "3050".
        /// </summary>
        private static string? LimpiarNumeroDocumento(string? valor)
        {
            if (string.IsNullOrWhiteSpace(valor)) return null;

            // Intentar parsear eliminando separadores de miles (puntos) y usando coma/punto como decimal
            var limpio = valor.Trim().Replace(".", "").Replace(",", ".");

            if (decimal.TryParse(limpio, NumberStyles.Any, CultureInfo.InvariantCulture, out var d))
                return ((long)Math.Truncate(d)).ToString(CultureInfo.InvariantCulture);

            // Si no parsea, devolver sin puntos como fallback
            return valor.Replace(".", "").Trim();
        }

        // ════════════════════════════════════════════════════════════════════════
        // HELPERS DE PARSEO
        // ════════════════════════════════════════════════════════════════════════

        private static decimal? ParseMonedaNullable(string? valor)
        {
            if (string.IsNullOrWhiteSpace(valor)) return null;
            var limpio = valor.Trim().Replace(".", "").Replace(",", ".");
            return decimal.TryParse(limpio, NumberStyles.Any, CultureInfo.InvariantCulture, out var r) ? r : null;
        }

        private static decimal? ParseDecimalNullable(string? valor)
        {
            if (string.IsNullOrWhiteSpace(valor)) return null;
            return decimal.TryParse(valor.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out var r) ? r : null;
        }

        private static DateTime? ParseFechaNullable(string? valor)
        {
            if (string.IsNullOrWhiteSpace(valor)) return null;
            string[] formatos = { "yyyy-MM-dd", "dd/MM/yyyy", "dd-MM-yyyy", "yyyy-MM-ddTHH:mm:ss" };
            return DateTime.TryParseExact(valor.Trim(), formatos, CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var f) ? f : null;
        }
    }
}
