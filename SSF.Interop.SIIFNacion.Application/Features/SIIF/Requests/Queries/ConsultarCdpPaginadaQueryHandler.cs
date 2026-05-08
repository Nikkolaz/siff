using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SSF.Interop.SIIFNacion.Application.Common.Auditoria;
using SSF.Interop.SIIFNacion.Application.Common.Interfaces.ExternalServices.SIIF;
using SSF.Interop.SIIFNacion.Application.Common.SIIF;
using SSF.Interop.SIIFNacion.Application.Contracts.Persistence;
using SSF.Interop.SIIFNacion.Application.DTOs.External.SIIF.Common;
using SSF.Interop.SIIFNacion.Application.DTOs.External.SIIF.Contracts;
using SSF.Interop.SIIFNacion.Application.DTOs.External.SIIF.Response.Contracts;
using SSF.Interop.SIIFNacion.Domain.Cdp;

namespace SSF.Interop.SIIFNacion.Application.Features.SIIF.Requests.Queries
{
    public sealed class ConsultarCdpPaginadaQueryHandler : IRequestHandler<ConsultarCdpPaginadaQuery, ConsultaListaCdpPaginadaResponseDto>
    {
        private const int CdpPageSizeMax = 50;
        private readonly ISiifBudgetService _siifBudgetService;
        private readonly ICdpPaginadoRepository _repository;
        private readonly IAuditoriaLogger _audit;

        public ConsultarCdpPaginadaQueryHandler(
            ISiifBudgetService siifBudgetService, 
            ICdpPaginadoRepository repository,
            IAuditoriaLogger audit)
        {
            _siifBudgetService = siifBudgetService;
            _repository = repository;
            _audit = audit;
        }

        public async Task<ConsultaListaCdpPaginadaResponseDto> Handle(ConsultarCdpPaginadaQuery request, CancellationToken cancellationToken)
        {
            var idTramite = Guid.NewGuid().ToString();
            var usuario = request.LoginUsuarioSiifHeader;
            var totalSw = Stopwatch.StartNew();

            await _audit.InfoAsync(
                idTramite,
                SiifPuntoDeControl.InicioFlujo,
                "Inicio consulta CDP paginada (Sincronización).",
                peticion: null,
                respuesta: null,
                tiempoRq: null,
                tiempoRs: null,
                estadoFinal: null,
                nmUserUpdate: usuario,
                cancellationToken);

            var headers = new SiifRequestHeaderDto
            {
                CodPci = request.CodPciHeader,
                LoginUsuarioSiif = request.LoginUsuarioSiifHeader,
                Consecutivo = request.ConsecutivoHeader,
                Hash = request.HashHeader
            };

            var bnCreated = (decimal)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var fechaCarga = DateTime.Now;
            ConsultaListaCdpPaginadaResponseDto? ultimaRespuesta = null;
            var page = 1;
            var totalInsertados = 0;
            var iteraciones = 0;
            var totalMsConsumoSiif = 0L;

            try
            {
                while (true)
                {
                    var body = new ConsultaListaCdpPaginadaRequestDto
                    {
                        PaginationDto = new PaginationDto { Page = page, Size = CdpPageSizeMax },
                        ConsultaListaCDP = new ConsultaListaCdpFiltroDto
                        {
                            PCIConsulta = request.PCIConsulta,
                            PCISubUnidades = request.PCISubUnidades,
                            FechaRegistroIni = request.FechaRegistroIni,
                            FechaRegistroFin = request.FechaRegistroFin,
                            TipoGasto = request.TipoGasto,
                            Rango = request.Rango
                        }
                    };

                    await _audit.InfoAsync(
                        idTramite,
                        SiifPuntoDeControl.EnvioRequestSiif,
                        $"Envío request SIIF CDP (página {page}).",
                        peticion: JsonSerializer.Serialize(body),
                        respuesta: null,
                        tiempoRq: null,
                        tiempoRs: null,
                        estadoFinal: null,
                        nmUserUpdate: usuario,
                        cancellationToken);

                    var siifSw = Stopwatch.StartNew();
                    var response = await _siifBudgetService.ConsultarListaCdpPaginadaAsync(body, headers, cancellationToken);
                    siifSw.Stop();
                    
                    iteraciones++;
                    totalMsConsumoSiif += siifSw.ElapsedMilliseconds;
                    ultimaRespuesta = response;

                    if (!response.Success)
                    {
                        await _audit.ErrorAsync(
                            idTramite,
                            SiifPuntoDeControl.ValidacionRespuesta,
                            $"SIIF respondió con error en página {page}: {response.Message}",
                            peticion: JsonSerializer.Serialize(body),
                            respuesta: response.Data.ToString(),
                            tiempoRq: null,
                            tiempoRs: $"{siifSw.ElapsedMilliseconds} ms",
                            estadoFinal: SiifAuditoriaEstadoFinal.Fallido,
                            nmUserUpdate: usuario,
                            exception: null,
                            cancellationToken);
                        break; 
                    }

                    var data = response.Data;
                    if (data.ValueKind == JsonValueKind.Object && data.TryGetProperty("ConsultaCDPSal", out JsonElement cdps))
                    {
                        var nuevosCdps = new List<CdpPaginado>();
                        foreach (var cdp in cdps.EnumerateArray())
                        {
                            var codigo = cdp.GetProperty("CodigoPCIConexion").GetString();
                            var numero = cdp.GetProperty("NumeroSolicitudCDP").GetInt32();

                            if (await _repository.ExistsAsync(codigo, numero, cancellationToken))
                                continue;

                            var nuevoCdp = new CdpPaginado
                            {
                                Oid = Guid.NewGuid().ToString(),
                                CodPciConexion = codigo,
                                DescPciConexion = cdp.GetProperty("DescripcionPCIConexion").GetString(),
                                CodSubNidad = cdp.GetProperty("CodigoSubunidad").GetString(),
                                DescSubUnidad = cdp.GetProperty("DescripcionSubunidad").GetString(),
                                NnSolicitudCdp = numero,
                                NnDocumento = cdp.GetProperty("NumeroDocumento").GetInt32(),
                                DtRegistro = cdp.GetProperty("FechaRegistro").GetDateTime(),
                                DtCreacion = cdp.GetProperty("FechaCreacion").GetDateTime(),
                                TipoCdp = cdp.GetProperty("TipoCDP").GetString(),
                                Estado = cdp.GetProperty("Estado").GetString(),
                                Objeto = cdp.GetProperty("Objeto").GetString(),
                                CodDepAfectacio = cdp.GetProperty("CodigoDependenciaAfectacion").GetString(),
                                DesDepAfectacio = cdp.GetProperty("DescripcionDependenciaAfectacion").GetString(),
                                CodPosGasto = cdp.GetProperty("CodigoPosicionGasto").GetString(),
                                DesPosGasto = cdp.GetProperty("DescripcionPosicionGasto").GetString(),
                                CodFuente = cdp.GetProperty("CodigoFuente").GetString(),
                                DesFuente = cdp.GetProperty("DescripcionFuente").GetString(),
                                CodRecurso = cdp.GetProperty("CodigoRecurso").GetString(),
                                DesRecurso = cdp.GetProperty("DescripcionRecurso").GetString(),
                                CodigoSituacion = cdp.GetProperty("CodigoSituacion").GetString(),
                                DesSituacion = cdp.GetProperty("DescripcionSituacion").GetString(),
                                VlOperaciones = Math.Round(cdp.GetProperty("ValorOperaciones").GetDecimal(), 2, MidpointRounding.AwayFromZero),
                                VlActual = Math.Round(cdp.GetProperty("ValorActual").GetDecimal(), 2, MidpointRounding.AwayFromZero),
                                SaldoPorComp = Math.Round(cdp.GetProperty("SaldoporComp").GetDecimal(), 2, MidpointRounding.AwayFromZero),
                                VlBloqueado = Math.Round(cdp.GetProperty("ValorBloqueado").GetDecimal(), 2, MidpointRounding.AwayFromZero),
                                ListCuentasXpagar = cdp.GetProperty("ListaCuentasPorPagar").GetString(),
                                ListaObligacion = cdp.GetProperty("ListaObligaciones").GetString(),
                                ListaOrdenDePago = cdp.GetProperty("ListaOrdenDePago").GetString(),
                                ListaReintegro = cdp.GetProperty("ListaReintegro").GetString(),
                                FechaCarga = fechaCarga,
                                VlInicial = Math.Round(cdp.GetProperty("ValorInicial").GetDecimal(), 2, MidpointRounding.AwayFromZero),
                                BnCreated = bnCreated,
                                FgEnabled = 1,
                                NrVersion = 1
                            };

                            nuevosCdps.Add(nuevoCdp);
                        }

                        if (nuevosCdps.Any())
                        {
                            await _repository.SaveRangeAsync(nuevosCdps, cancellationToken);
                            totalInsertados += nuevosCdps.Count;
                        }

                        if (cdps.GetArrayLength() < CdpPageSizeMax)
                            break;

                        page++;
                    }
                    else
                    {
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                totalSw.Stop();
                await _audit.ErrorAsync(
                    idTramite,
                    SiifPuntoDeControl.FinError,
                    $"Error procesando CDP paginado: {ex.Message}",
                    peticion: null,
                    respuesta: null,
                    tiempoRq: null,
                    tiempoRs: $"{totalSw.ElapsedMilliseconds} ms",
                    estadoFinal: SiifAuditoriaEstadoFinal.Fallido,
                    nmUserUpdate: usuario,
                    exception: ex,
                    cancellationToken);
                throw;
            }

            totalSw.Stop();
            await _audit.InfoAsync(
                idTramite,
                SiifPuntoDeControl.FinExitoso,
                $"Sincronización terminada. Insertados: {totalInsertados}. Páginas: {iteraciones}.",
                peticion: null,
                respuesta: null,
                tiempoRq: null,
                tiempoRs: $"{totalSw.ElapsedMilliseconds} ms",
                estadoFinal: SiifAuditoriaEstadoFinal.Exitoso,
                nmUserUpdate: usuario,
                cancellationToken);

            return ultimaRespuesta ?? new ConsultaListaCdpPaginadaResponseDto { Success = false, Message = "No se recibió respuesta de SIIF" };
        }
    }
}
