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
        private readonly IAuditoriaLogger _audit;

        public ConsultarCompromisoPaginadoQueryHandler(
            ISiifBudgetService siifBudgetService,
            IDynCompromPaginRepository repository,
            IAuditoriaLogger audit)
        {
            _siifBudgetService = siifBudgetService;
            _repository        = repository;
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
    }
}
