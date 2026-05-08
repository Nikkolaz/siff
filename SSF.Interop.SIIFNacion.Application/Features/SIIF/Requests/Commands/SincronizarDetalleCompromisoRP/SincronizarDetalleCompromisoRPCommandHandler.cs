using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
    public class SincronizarDetalleCompromisoRPCommandHandler
        : IRequestHandler<SincronizarDetalleCompromisoRPCommand, SincronizarDetalleCompromisoRPResponseDto>
    {
        private readonly ISiifBudgetService _siifBudgetService;
        private readonly IDynCompromPaginRepository _paginRepository;
        private readonly IDynCompromisoRepository _detailRepository;
        private readonly IAuditoriaLogger _audit;

        public SincronizarDetalleCompromisoRPCommandHandler(
            ISiifBudgetService siifBudgetService,
            IDynCompromPaginRepository paginRepository,
            IDynCompromisoRepository detailRepository,
            IAuditoriaLogger audit)
        {
            _siifBudgetService = siifBudgetService;
            _paginRepository = paginRepository;
            _detailRepository = detailRepository;
            _audit = audit;
        }

        public async Task<SincronizarDetalleCompromisoRPResponseDto> Handle(
            SincronizarDetalleCompromisoRPCommand request,
            CancellationToken cancellationToken)
        {
            var idTramite = Guid.NewGuid().ToString();
            var usuario = request.LoginUsuarioSiifHeader;
            var result = new SincronizarDetalleCompromisoRPResponseDto();

            await _audit.InfoAsync(idTramite, SiifPuntoDeControl.InicioFlujo,
                $"🚀 Iniciando sincronización de detalle RP (HU 6). Pci={request.Pci}, Vigencia={request.Vigencia}.",
                null, null, null, null, null, usuario, cancellationToken);

            // 1. Obtener registros para sincronizar
            var registros = await _paginRepository.GetRecordsToSyncAsync(request.Pci, request.Vigencia, cancellationToken);
            var listaRegistros = registros.ToList();

            if (!listaRegistros.Any())
            {
                result.Mensaje = "No se encontraron registros nuevos o actualizados para sincronizar.";
                await _audit.InfoAsync(idTramite, SiifPuntoDeControl.FinExitoso, result.Mensaje, null, null, null, null, null, usuario, cancellationToken);
                return result;
            }

            int actualizados = 0;
            var headers = new SiifRequestHeaderDto
            {
                CodPci = request.CodPciHeader,
                LoginUsuarioSiif = request.LoginUsuarioSiifHeader,
                Consecutivo = request.ConsecutivoHeader,
                Hash = request.HashHeader
            };

            // Regla de Vigencia: "Vigencia 1" -> "Actual", "Vigencia 2" -> "Reserva Presupuestal"
            string vigenciaSiif = request.Vigencia == "1" ? "Actual" : "Reserva Presupuestal";

            foreach (var item in listaRegistros)
            {
                try
                {
                    // A. Preparación de Parámetros
                    var body = new ConsultarCompromisoRequestDto
                    {
                        Pci = request.Pci,
                        CodCompromisoPptalGastos = (int)(item.CodCompromiso ?? 0),
                        Vigencia = vigenciaSiif
                    };

                    // C. Consumo de Web Service (SIIF)
                    var response = await _siifBudgetService.ConsultarCompromisoPptalAsync(body, headers, cancellationToken);

                    // D. Normalización y Validación de Respuesta
                    if (response == null || response.Codigo <= 0)
                    {
                        await _audit.WarnAsync(idTramite, SiifPuntoDeControl.ValidacionRespuesta,
                            $"⚠️ El RP {item.CodCompromiso} no existe en SIIF o devolvió error.",
                            null, null, null, null, SiifAuditoriaEstadoFinal.Parcial, usuario, cancellationToken);
                        continue;
                    }

                    // Map to Production Entity
                    var detailEntity = MapToDetailEntity(response, request.Vigencia);

                    // E. Persistencia Transaccional
                    await _detailRepository.UpsertDetailAsync(detailEntity, cancellationToken);
                    actualizados++;

                    // Log Auditoría (INSERT/UPDATE exitoso)
                    await _audit.InfoAsync(idTramite, SiifPuntoDeControl.InsercionBd,
                        $"✅ Sincronizado RP {item.CodCompromiso} (IdCompromiso={response.Codigo}).",
                        null, null, null, null, null, usuario, cancellationToken);
                }
                catch (Exception ex)
                {
                    await _audit.ErrorAsync(idTramite, SiifPuntoDeControl.FinError,
                        $"❌ Error procesando RP {item.CodCompromiso}: {ex.Message}",
                        null, null, null, null, SiifAuditoriaEstadoFinal.Fallido, usuario, ex, cancellationToken);
                    // Continuar con el siguiente registro
                }
            }

            result.CantidadRegistrosActualizados = actualizados;
            result.Mensaje = $"Proceso terminado. Sincronizados: {actualizados} de {listaRegistros.Count}.";

            await _audit.InfoAsync(idTramite, SiifPuntoDeControl.FinExitoso, result.Mensaje, null, null, null, null, SiifAuditoriaEstadoFinal.Exitoso, usuario, cancellationToken);

            return result;
        }

        private DynTblCCompPtal MapToDetailEntity(ConsultarCompromisoResponseDto siif, string vigenciaOriginal)
        {
            var ahora = DateTime.Now;
            var bnNow = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            var entity = new DynTblCCompPtal
            {
                IdCompromiso = siif.Codigo,
                VigenciaNm = siif.Vigencia,
                FechaRegistro = siif.FechaRegistro,
                Estado = siif.Estado,
                CodCdp = siif.CodigoCdp,
                FechaCdp = siif.FechaCdp,
                CodMoneda = siif.CodigoMoneda,
                NmMoneda = siif.NombreMoneda,
                ValorTasa = siif.ValorTasa,
                Descripcion = siif.Descripcion,
                Objeto = siif.Objeto,
                
                // Normalización de valores (Ya vienen como decimal desde el servicio, no requieren Replace si ya son decimal)
                ValorInicial = siif.ValorInicial,
                VlIniOriMoneda = siif.ValorInicialOriginalMoneda,
                VlTOperacion = siif.ValorTotalOperacion,
                ValorActual = siif.ValorActual,
                SaldoXObligar = siif.SaldoPorObligar,
                SaldoMoneda = siif.SaldoMoneda,

                TtDocumento = siif.Tercero?.TipoDocumento,
                TnDocumento = siif.Tercero?.NumeroDocumento,
                TerceroNm = siif.Tercero?.Nombre,
                MedioPago = siif.MedioPago,

                CuentaNn = siif.DetalleCuentaBancaria?.Numero,
                CuentaEntFinan = siif.DetalleCuentaBancaria?.EntidadFinanciera,
                CuentaTipo = siif.DetalleCuentaBancaria?.TipoCuenta,
                CuentaEstado = siif.DetalleCuentaBancaria?.Estado,

                OrdenadorTDoc = siif.DetalleOrdenadorGasto?.TipoDocumento,
                OrdenadorNDoc = siif.DetalleOrdenadorGasto?.NumeroDocumento,
                OrdenadorNm = siif.DetalleOrdenadorGasto?.Nombre,
                OrdenadorConsec = siif.DetalleOrdenadorGasto?.Consecutivo,
                OrdenadorCodCar = siif.DetalleOrdenadorGasto?.CodigoCargo,
                OrdenadorNmCarg = siif.DetalleOrdenadorGasto?.NombreCargo,

                NnDocSoporte = siif.DatosAdministrativos?.NumeroDocumentoSoporte,
                TDocSoporte = siif.DatosAdministrativos?.TipoDocumentoSoporte,
                DtDocSoporte = siif.DatosAdministrativos?.Fecha,
                CajaMenor = siif.CajaMenor?.ToString(),

                FechaCarga = ahora,
                AnioVigencia = vigenciaOriginal,

                Items = siif.ListadoItemsAfectacion.Select(x => new DynTblListItemsAfe
                {
                    CodDepAfecta = x.CodigoDependenciaAfectacion,
                    NmDepAfecta = x.NombreDependenciaAfectacion,
                    CodPGasto = x.CodigoPosicionGasto,
                    NmPGasto = x.NombrePosicionGasto,
                    CodFFinan = x.CodigoFuenteFinanciacion,
                    NmFFinan = x.NombreFuenteFinanciacion,
                    CodRPPtal = x.CodigoRecursoPresupuestal,
                    NmRPPtal = x.NombreRecursoPresupuestal,
                    CodSFondo = x.CodigoSituacionFondos,
                    NmSFondo = x.NombreSituacionFondos,
                    VlInicial = x.ValorInicial,
                    VlOperaciones = x.ValorOperaciones,
                    VlActual = x.ValorActual,
                    Saldo = x.Saldo,
                    FechaCarga = ahora,
                    AnioVigencia = vigenciaOriginal
                }).ToList()
            };

            return entity;
        }
    }
}
