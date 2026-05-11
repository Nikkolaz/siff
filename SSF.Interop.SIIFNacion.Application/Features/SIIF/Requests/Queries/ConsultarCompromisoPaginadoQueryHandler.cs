using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
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

namespace SSF.Interop.SIIFNacion.Application.Features.SIIF.Requests.Queries
{
    public sealed class ConsultarCompromisoPaginadoQueryHandler
        : IRequestHandler<ConsultarCompromisoPaginadoQuery, ConsultaListaCompromisoPaginadaResponseDto>
    {
        private readonly ISiifBudgetService _siifBudgetService;
        private readonly IDynCompromPaginRepository _repository;
        private readonly IDynCompromisoRepository _detailRepository;
        private readonly IAuditoriaLogger _audit;

        public ConsultarCompromisoPaginadoQueryHandler(
            ISiifBudgetService siifBudgetService,
            IDynCompromPaginRepository repository,
            IDynCompromisoRepository detailRepository,
            IAuditoriaLogger audit)
        {
            _siifBudgetService = siifBudgetService;
            _repository        = repository;
            _detailRepository  = detailRepository;
            _audit             = audit;
        }

        public async Task<ConsultaListaCompromisoPaginadaResponseDto> Handle(
            ConsultarCompromisoPaginadoQuery request,
            CancellationToken cancellationToken)
        {
            var idTramite = Guid.NewGuid().ToString();
            var usuario   = request.LoginUsuarioSiifHeader;
            var totalSw   = Stopwatch.StartNew();

            await _audit.InfoAsync(
                idTramite,
                SiifPuntoDeControl.InicioFlujo,
                $"🚀 Inicio consulta CompromisoPaginado. PCI={request.PCI}, Vigencia={request.Vigencia}, Page={request.Page}, Size={request.Size}.",
                peticion: null, respuesta: null, tiempoRq: null, tiempoRs: null,
                estadoFinal: null, nmUserUpdate: usuario, cancellationToken);

            // ── 1. Construir headers ─────────────────────────────────────────────────────
            var headers = new SiifRequestHeaderDto
            {
                CodPci           = request.CodPciHeader,
                LoginUsuarioSiif = request.LoginUsuarioSiifHeader,
                Consecutivo      = request.ConsecutivoHeader,
                Hash             = request.HashHeader
            };

            // ── 2. Construir body ────────────────────────────────────────────────────────
            var body = new ConsultaListaCompromisoPaginadaRequestDto
            {
                PaginationDto = new PaginationDto
                {
                    Page = request.Page,
                    Size = request.Size
                },
                ConsultaCompromiso = new ConsultaCompromisoFiltroDto
                {
                    PCI         = request.PCI,
                    FechaInicio = request.FechaInicio,
                    FechaFin    = request.FechaFin,
                    TipoGasto   = request.TipoGasto,
                    Rango       = request.Rango,
                    Vigencia    = request.Vigencia
                }
            };

            await _audit.InfoAsync(
                idTramite,
                SiifPuntoDeControl.EnvioRequestSiif,
                $"📤 Enviando request a SIIF ConsultarListaCompromisoPaginadaAsync. Page={request.Page}.",
                peticion: JsonSerializer.Serialize(body), respuesta: null,
                tiempoRq: null, tiempoRs: null, estadoFinal: null,
                nmUserUpdate: usuario, cancellationToken);

            // ── 3. Bucle de Paginación ───────────────────────────────────────────────────
            int currentPage = request.Page > 0 ? request.Page : 1;
            int pageSize = request.Size > 0 ? request.Size : 50;
            bool hasMorePages = true;
            
            var todosLosRegistros = new List<ListaCompromisoItemDto>();
            ConsultaListaCompromisoPaginadaResponseDto? lastResponse = null;
            var siifSw = Stopwatch.StartNew();

            try
            {
                while (hasMorePages)
                {
                    body.PaginationDto.Page = currentPage;
                    body.PaginationDto.Size = pageSize;

                    var response = await _siifBudgetService
                        .ConsultarListaCompromisoPaginadaAsync(body, headers, cancellationToken);
                    
                    lastResponse = response;

                    if (response?.consultaCompromisoSal != null && response.consultaCompromisoSal.Count > 0)
                    {
                        todosLosRegistros.AddRange(response.consultaCompromisoSal);
                        
                        if (response.consultaCompromisoSal.Count == pageSize)
                        {
                            currentPage++;
                        }
                        else
                        {
                            hasMorePages = false;
                        }
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
                    $"💥 Error llamando SIIF CompromisoPaginado. {ex.Message}",
                    peticion: JsonSerializer.Serialize(body), respuesta: null,
                    tiempoRq: null, tiempoRs: $"{totalSw.ElapsedMilliseconds} ms",
                    estadoFinal: SiifAuditoriaEstadoFinal.Fallido,
                    nmUserUpdate: usuario, exception: ex, cancellationToken);

                throw;
            }

            await _audit.InfoAsync(
                idTramite,
                SiifPuntoDeControl.RespuestaSiifRecibida,
                $"📥 Respuesta consolidada. Items Totales={todosLosRegistros.Count}.",
                peticion: null, respuesta: null,
                tiempoRq: null, tiempoRs: $"{siifSw.ElapsedMilliseconds} ms",
                estadoFinal: null, nmUserUpdate: usuario, cancellationToken);

            // ── 4. Persistir con Upsert ──────────────────────────────────────────────────
            if (todosLosRegistros.Count > 0)
            {
                var anioVigencia = request.Vigencia ?? DateTime.UtcNow.Year.ToString();
                var entidades    = MapToEntities(todosLosRegistros, anioVigencia);

                var swInsert = Stopwatch.StartNew();
                await _repository.UpsertCompromisosAsync(entidades, cancellationToken);
                swInsert.Stop();

                await _audit.InfoAsync(
                    idTramite,
                    SiifPuntoDeControl.InsercionBd,
                    $"💾 Inserción BD CompromisoPaginado OK. Count={todosLosRegistros.Count}, AnioVigencia={anioVigencia}.",
                    peticion: null, respuesta: null, tiempoRq: null,
                    tiempoRs: $"{swInsert.ElapsedMilliseconds} ms",
                    estadoFinal: null, nmUserUpdate: usuario, cancellationToken);
            }
            else
            {
                await _audit.InfoAsync(
                    idTramite,
                    SiifPuntoDeControl.ValidacionRespuesta,
                    "📭 SIIF devolvió lista de compromisos vacía. No se persiste nada.",
                    peticion: null, respuesta: null, tiempoRq: null, tiempoRs: null,
                    estadoFinal: null, nmUserUpdate: usuario, cancellationToken);
            }

            // ── 5. Fin exitoso ───────────────────────────────────────────────────────────
            totalSw.Stop();

            await _audit.InfoAsync(
                idTramite,
                SiifPuntoDeControl.FinExitoso,
                $"✅ Fin flujo CompromisoPaginado exitoso. msTotal={totalSw.ElapsedMilliseconds}.",
                peticion: null, respuesta: null, tiempoRq: null,
                tiempoRs: $"{totalSw.ElapsedMilliseconds} ms",
                estadoFinal: SiifAuditoriaEstadoFinal.Exitoso,
                nmUserUpdate: usuario, cancellationToken);

            if (lastResponse != null)
            {
                lastResponse.consultaCompromisoSal = todosLosRegistros;
            }

            // ── 6. FASE 2: Wipe detalle + consulta individual por cada RP ──────────────
            if (todosLosRegistros.Count > 0)
            {
                var anioVigForWipe = request.Vigencia ?? DateTime.UtcNow.Year.ToString();
                var vigenciaSiif   = request.Vigencia == "1" ? "Actual" : "Reserva Presupuestal";

                // Limpiar detalle anterior
                await _detailRepository.WipeByVigenciaAsync(anioVigForWipe, cancellationToken);

                int exitosos = 0, fallidos = 0;

                foreach (var rp in todosLosRegistros)
                {
                    if (string.IsNullOrWhiteSpace(rp.CodigoCompromiso) ||
                        !int.TryParse(rp.CodigoCompromiso.Trim(), out var codRp) ||
                        codRp <= 0)
                    {
                        fallidos++;
                        continue;
                    }

                    try
                    {
                        var bodyDetalle = new ConsultarCompromisoRequestDto
                        {
                            Pci                      = request.PCI,
                            CodCompromisoPptalGastos = codRp,
                            Vigencia                 = vigenciaSiif
                        };

                        var detalle = await _siifBudgetService
                            .ConsultarCompromisoPptalAsync(bodyDetalle, headers, cancellationToken);

                        if (detalle == null || detalle.Codigo <= 0)
                        {
                            fallidos++;
                            continue;
                        }

                        var entidadDetalle = MapToDetailEntity(detalle, anioVigForWipe);
                        entidadDetalle.Oid       = Guid.NewGuid().ToString("N").ToUpperInvariant();
                        entidadDetalle.BnCreated = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                        entidadDetalle.NrVersion = 1;
                        entidadDetalle.FgEnabled = 1;
                        entidadDetalle.FgSystem  = 0;

                        await _detailRepository.SaveAsync(entidadDetalle, cancellationToken);
                        exitosos++;

                        await _audit.InfoAsync(idTramite, SiifPuntoDeControl.InsercionBd,
                            $"✅ [Fase 2] RP {codRp} guardado en DYNTBLCCOMPPTAL.",
                            null, null, null, null, null, usuario, cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        fallidos++;
                        await _audit.ErrorAsync(idTramite, SiifPuntoDeControl.FinError,
                            $"❌ [Fase 2] Error RP {codRp}: {ex.Message}",
                            null, null, null, null, SiifAuditoriaEstadoFinal.Parcial,
                            usuario, ex, cancellationToken);
                    }
                }

                await _audit.InfoAsync(idTramite, SiifPuntoDeControl.FinExitoso,
                    $"📊 [Fase 2] Detalle completo. Exitosos={exitosos}, Fallidos={fallidos}.",
                    null, null, null, null, SiifAuditoriaEstadoFinal.Exitoso, usuario, cancellationToken);
            }

            return lastResponse ?? new ConsultaListaCompromisoPaginadaResponseDto();
        }

        private static IEnumerable<DynTblCompromPagin> MapToEntities(
            IEnumerable<ListaCompromisoItemDto> items,
            string anioVigencia)
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
                    VigenciaCod    = anioVigencia,
                    VigenciaNm     = null,

                    FechaRegistro = ParseFechaNullable(item.FechaRegistro),
                    FechaCreacion = ParseFechaNullable(item.FechaCreacion),

                    Estado = item.Estado,

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

                    FechaDocSoporte  = ParseFechaNullable(item.FechaDocumentoSoporte),
                    TipoDocSoporte   = item.TipoDocumentoSoporte,
                    NumeroDocSoport  = item.NumeroDocumentoSoporte,
                    Observaciones    = item.Observaciones,

                    FechaCarga   = ahora,
                    AnioVigencia = anioVigencia
                };
            }
        }

        private static decimal? ParseMonedaNullable(string? valor)
        {
            if (string.IsNullOrWhiteSpace(valor)) return null;

            var limpio = valor.Trim()
                              .Replace(".", "")
                              .Replace(",", ".");

            return decimal.TryParse(limpio, NumberStyles.Any,
                                    CultureInfo.InvariantCulture, out var resultado)
                ? resultado
                : null;
        }

        private static decimal? ParseDecimalNullable(string? valor)
        {
            if (string.IsNullOrWhiteSpace(valor)) return null;
            return decimal.TryParse(valor.Trim(), NumberStyles.Any,
                                    CultureInfo.InvariantCulture, out var resultado)
                ? resultado
                : null;
        }

        private static DateTime? ParseFechaNullable(string? valor)
        {
            if (string.IsNullOrWhiteSpace(valor)) return null;

            string[] formatos = {
                "yyyy-MM-dd", "dd/MM/yyyy", "dd-MM-yyyy",
                "yyyy-MM-ddTHH:mm:ss", "dd/MM/yyyy HH:mm:ss"
            };

            return DateTime.TryParseExact(valor.Trim(), formatos,
                                          CultureInfo.InvariantCulture,
                                          DateTimeStyles.None, out var fecha)
                ? fecha
                : null;
        }

        private static DynTblCCompPtal MapToDetailEntity(
            ConsultarCompromisoResponseDto siif, string vigencia)
        {
            var ahora = DateTime.Now;
            return new DynTblCCompPtal
            {
                IdCompromiso   = siif.Codigo,
                VigenciaNm     = siif.Vigencia,
                FechaRegistro  = siif.FechaRegistro,
                Estado         = siif.Estado,
                CodCdp         = siif.CodigoCdp,
                FechaCdp       = siif.FechaCdp,
                CodMoneda      = siif.CodigoMoneda,
                NmMoneda       = siif.NombreMoneda,
                ValorTasa      = siif.ValorTasa,
                Descripcion    = siif.Descripcion,
                Objeto         = siif.Objeto,
                ValorInicial   = siif.ValorInicial,
                VlIniOriMoneda = siif.ValorInicialOriginalMoneda,
                VlTOperacion   = siif.ValorTotalOperacion,
                ValorActual    = siif.ValorActual,
                SaldoXObligar  = siif.SaldoPorObligar,
                SaldoMoneda    = siif.SaldoMoneda,
                TtDocumento    = siif.Tercero?.TipoDocumento,
                TnDocumento    = LimpiarNumeroDocumento(siif.Tercero?.NumeroDocumento),
                TerceroNm      = siif.Tercero?.Nombre,
                MedioPago      = siif.MedioPago,
                CuentaNn       = siif.DetalleCuentaBancaria?.Numero,
                CuentaEntFinan = siif.DetalleCuentaBancaria?.EntidadFinanciera,
                CuentaTipo     = siif.DetalleCuentaBancaria?.TipoCuenta,
                CuentaEstado   = siif.DetalleCuentaBancaria?.Estado,
                OrdenadorTDoc  = siif.DetalleOrdenadorGasto?.TipoDocumento,
                OrdenadorNDoc  = siif.DetalleOrdenadorGasto?.NumeroDocumento,
                OrdenadorNm    = siif.DetalleOrdenadorGasto?.Nombre,
                OrdenadorConsec = siif.DetalleOrdenadorGasto?.Consecutivo,
                OrdenadorCodCar = siif.DetalleOrdenadorGasto?.CodigoCargo,
                OrdenadorNmCarg = siif.DetalleOrdenadorGasto?.NombreCargo,
                NnDocSoporte   = siif.DatosAdministrativos?.NumeroDocumentoSoporte?.Replace(".", "").Trim(),
                TDocSoporte    = siif.DatosAdministrativos?.TipoDocumentoSoporte,
                DtDocSoporte   = siif.DatosAdministrativos?.Fecha,
                CajaMenor      = siif.CajaMenor?.ToString(),
                FechaCarga     = ahora,
                AnioVigencia   = vigencia,
                Items = siif.ListadoItemsAfectacion.Select(x => new DynTblListItemsAfe
                {
                    Oid          = Guid.NewGuid().ToString("N").ToUpperInvariant(),
                    NrVersion    = 1,
                    BnCreated    = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    FgEnabled    = 1,
                    FgSystem     = 0,
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

        private static string? LimpiarNumeroDocumento(string? valor)
        {
            if (string.IsNullOrWhiteSpace(valor)) return null;
            var limpio = valor.Trim().Replace(".", "").Replace(",", ".");
            if (decimal.TryParse(limpio, NumberStyles.Any, CultureInfo.InvariantCulture, out var d))
                return ((long)Math.Truncate(d)).ToString(CultureInfo.InvariantCulture);
            return valor.Replace(".", "").Trim();
        }
    }
}
