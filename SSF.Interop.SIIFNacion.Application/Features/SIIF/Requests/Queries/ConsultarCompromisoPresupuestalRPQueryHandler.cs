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
using SSF.Interop.SIIFNacion.Application.Contracts.Persistence;
using SSF.Interop.SIIFNacion.Application.DTOs.External.SIIF.Common;
using SSF.Interop.SIIFNacion.Application.DTOs.External.SIIF.Contracts;
using SSF.Interop.SIIFNacion.Application.DTOs.External.SIIF.Response;
using SSF.Interop.SIIFNacion.Application.DTOs.External.SIIF.Response.Contracts;
using SSF.Interop.SIIFNacion.Domain.Cdp;

namespace SSF.Interop.SIIFNacion.Application.Features.SIIF.Requests.Queries
{
 
    public sealed class ConsultarCompromisoPresupuestalRPQueryHandler
        : IRequestHandler<ConsultarCompromisoPresupuestalRPQuery, ConsultarCompromisoResponseDto>
    {
        private readonly ISiifBudgetService _siifBudgetService;
        private readonly ICdpCompromisoRepository _repository;
        private readonly IDynCompromisoRepository _dynRepository;

        private readonly IAuditoriaLogger _audit;

        public ConsultarCompromisoPresupuestalRPQueryHandler(
            ISiifBudgetService siifBudgetService,
            ICdpCompromisoRepository repository,
            IDynCompromisoRepository dynRepository,
            IAuditoriaLogger audit)
        {
            _siifBudgetService = siifBudgetService;
            _repository        = repository;
            _dynRepository     = dynRepository;
            _audit             = audit;
        }

        public async Task<ConsultarCompromisoResponseDto> Handle(
            ConsultarCompromisoPresupuestalRPQuery request,
            CancellationToken cancellationToken)
        {
            var idTramite = Guid.NewGuid().ToString();
            var usuario   = request.LoginUsuarioSiifHeader;
            var totalSw   = Stopwatch.StartNew();

            await _audit.InfoAsync(
                idTramite,
                SiifPuntoDeControl.InicioFlujo,
                $"🚀 Inicio consulta CompromisoPresupuestalRP. Pci={request.Pci}, CodCompromiso={request.CodCompromisoPptalGastos}, Vigencia={request.Vigencia}.",
                peticion: null, respuesta: null, tiempoRq: null, tiempoRs: null,
                estadoFinal: null, nmUserUpdate: usuario, cancellationToken);

            // ── 1. Construir headers y body ─────────────────────────────────────────────
            var headers = new SiifRequestHeaderDto
            {
                CodPci           = request.CodPciHeader,
                LoginUsuarioSiif = request.LoginUsuarioSiifHeader,
                Consecutivo      = request.ConsecutivoHeader,
                Hash             = request.HashHeader
            };

            var body = new ConsultarCompromisoRequestDto
            {
                Pci                      = request.Pci,
                CodCompromisoPptalGastos = request.CodCompromisoPptalGastos,
                Vigencia                 = request.Vigencia
            };

            await _audit.InfoAsync(
                idTramite,
                SiifPuntoDeControl.EnvioRequestSiif,
                $"📤 Enviando request a SIIF ConsultarCompromisoPptalAsync. CodCompromiso={request.CodCompromisoPptalGastos}.",
                peticion: JsonSerializer.Serialize(body), respuesta: null,
                tiempoRq: null, tiempoRs: null, estadoFinal: null,
                nmUserUpdate: usuario, cancellationToken);

            // ── 2. Llamar a SIIF ────────────────────────────────────────────────────────
            ConsultarCompromisoResponseDto response;
            var siifSw = Stopwatch.StartNew();

            try
            {
                response = await _siifBudgetService.ConsultarCompromisoPptalAsync(body, headers, cancellationToken);
                siifSw.Stop();
            }
            catch (Exception ex)
            {
                siifSw.Stop();
                totalSw.Stop();

                await _audit.ErrorAsync(
                    idTramite,
                    SiifPuntoDeControl.FinError,
                    $"💥 Error al llamar ConsultarCompromisoPptalAsync en SIIF. {ex.Message}",
                    peticion: JsonSerializer.Serialize(body), respuesta: null,
                    tiempoRq: null, tiempoRs: $"{totalSw.ElapsedMilliseconds} ms",
                    estadoFinal: SiifAuditoriaEstadoFinal.Fallido,
                    nmUserUpdate: usuario, exception: ex, cancellationToken);

                throw;
            }

            await _audit.InfoAsync(
                idTramite,
                SiifPuntoDeControl.RespuestaSiifRecibida,
                $"📥 Respuesta recibida de SIIF. Codigo={response?.Codigo}, esNull={response is null}.",
                peticion: null, respuesta: null,
                tiempoRq: null, tiempoRs: $"{siifSw.ElapsedMilliseconds} ms",
                estadoFinal: null, nmUserUpdate: usuario, cancellationToken);

            // ── 3. Validar respuesta ────────────────────────────────────────────────────
            if (response is null || response.Codigo <= 0)
            {
                // ⚠️ SIIF devolvió null o Codigo=0: no hay datos que persistir.
                // Se considera un resultado válido de negocio (el compromiso no existe en SIIF).
                totalSw.Stop();

                await _audit.WarnAsync(
                    idTramite,
                    SiifPuntoDeControl.ValidacionRespuesta,
                    $"⚠️ SIIF devolvió respuesta vacía o sin código. Codigo={response?.Codigo}. No se persiste nada.",
                    peticion: null, respuesta: null, tiempoRq: null,
                    tiempoRs: $"{totalSw.ElapsedMilliseconds} ms",
                    estadoFinal: SiifAuditoriaEstadoFinal.Parcial,
                    nmUserUpdate: usuario, cancellationToken);

                return response ?? new ConsultarCompromisoResponseDto();
            }

            await _audit.InfoAsync(
                idTramite,
                SiifPuntoDeControl.ValidacionRespuesta,
                $"✅ Compromiso validado. Codigo={response.Codigo}, ValorInicial={response.ValorInicial}, Vigencia={response.Vigencia}.",
                peticion: null, respuesta: null, tiempoRq: null, tiempoRs: null,
                estadoFinal: null, nmUserUpdate: usuario, cancellationToken);

            // ── 4. Preparar datos comunes ───────────────────────────────────────────────
            var ahora     = DateTime.Now;
            var bnCreated = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            // 🛡️ Null-safety: SIIF a veces omite el nodo ListadoPlanesPago por completo.
            if (response.ListadoPlanesPagoRaw is null)
                response.ListadoPlanesPagoRaw = new List<PlanPagoDto>();

            // ── 5. Persistir tabla legacy (tblccompptal) ────────────────────────────────
            // 🐛 BUG FIX histórico: ExistsAsync usaba ValorInicial como clave de unicidad.
            // Dos compromisos distintos pueden tener el mismo valor monetario.
            // La clave correcta es IdCompromiso (= response.Codigo). ✅
            var swLegacy   = Stopwatch.StartNew();
            bool legacyExiste = await _repository.ExistsAsync(response.Codigo, cancellationToken);
            swLegacy.Stop();

            if (!legacyExiste)
            {
                try
                {
                    await _repository.ResetIdentityIfEmpty();

                    var nuevoComp = new CdpCompromiso
                    {
                        IdCompromiso    = response.Codigo,
                        vigencianm      = response.Vigencia,
                        FechaRegistro   = response.FechaRegistro,
                        Estado          = response.Estado,
                        codcdp          = response.CodigoCdp,
                        FechaCdp        = response.FechaCdp,
                        codmoneda       = response.CodigoMoneda,
                        nmmoneda        = response.NombreMoneda,
                        ValorTasa       = Math.Round(response.ValorTasa, 2, MidpointRounding.AwayFromZero),
                        Descripcion     = response.Descripcion,
                        Objeto          = response.Objeto,
                        ValorInicial    = Math.Round(response.ValorInicial, 2, MidpointRounding.AwayFromZero),
                        vliniorimoneda  = Math.Round(response.ValorInicialOriginalMoneda, 2, MidpointRounding.AwayFromZero),
                        vltoperacion    = Math.Round(response.ValorTotalOperacion, 2, MidpointRounding.AwayFromZero),
                        ValorActual     = Math.Round(response.ValorActual, 2, MidpointRounding.AwayFromZero),
                        SaldoxObligar   = Math.Round(response.SaldoPorObligar, 2, MidpointRounding.AwayFromZero),
                        saldomoneda     = Math.Round(response.SaldoMoneda, 2, MidpointRounding.AwayFromZero),
                        ttdocumento     = response.Tercero?.TipoDocumento,
                        tndocumento     = response.Tercero?.NumeroDocumento,
                        terceronm       = response.Tercero?.Nombre,
                        mediopago       = response.MedioPago,
                        cuentann        = response.DetalleCuentaBancaria?.Numero,
                        cuentaentfinan  = response.DetalleCuentaBancaria?.EntidadFinanciera,
                        CuentaTipo      = response.DetalleCuentaBancaria?.TipoCuenta,
                        CuentaEstado    = response.DetalleCuentaBancaria?.Estado,
                        ordenadortdoc   = response.DetalleOrdenadorGasto?.TipoDocumento,
                        ordenadorndoc   = response.DetalleOrdenadorGasto?.NumeroDocumento,
                        ordenadornm     = response.DetalleOrdenadorGasto?.Nombre,
                        ordenadorconsec = response.DetalleOrdenadorGasto?.Consecutivo,
                        ordenadorcodcar = response.DetalleOrdenadorGasto?.CodigoCargo,
                        ordenadornmcarg = response.DetalleOrdenadorGasto?.NombreCargo,
                        nndocsoporte    = response.DatosAdministrativos?.NumeroDocumentoSoporte,
                        tdocsoporte     = response.DatosAdministrativos?.TipoDocumentoSoporte,
                        dtdocsoporte    = response.DatosAdministrativos?.Fecha,
                        cajamenor       = response.CajaMenor?.ToString(),
                        fechacarga      = ahora,
                        Items = response.ListadoItemsAfectacion.Select(x => new CdpCompromisoItem
                        {
                            coddepafecta  = x.CodigoDependenciaAfectacion,
                            nmdepafecta   = x.NombreDependenciaAfectacion,
                            codpgasto     = x.CodigoPosicionGasto,
                            nmpgasto      = x.NombrePosicionGasto,
                            codffinan     = x.CodigoFuenteFinanciacion,
                            nmffinan      = x.NombreFuenteFinanciacion,
                            codrpptal     = x.CodigoRecursoPresupuestal,
                            nmrpptal      = x.NombreRecursoPresupuestal,
                            codsfondo     = x.CodigoSituacionFondos,
                            nmsfondo      = x.NombreSituacionFondos,
                            vlinicial     = x.ValorInicial,
                            vloperaciones = x.ValorOperaciones,
                            vlactual      = x.ValorActual,
                            saldo         = x.Saldo,
                        }).ToList(),
                        PlanesPago = response.ListadoPlanesPago.Select(x => new CdpCompromisoPlanPago
                        {
                            FechaPago       = x.FechaPago,
                            coddepafepac    = x.CodigoDependenciaAfectacionPAC,
                            nmdepafepac     = x.NombreDependenciaAfectacionPAC,
                            codposipac      = x.CodigoPosicionCatalogoPAC,
                            nmposipac       = x.NombrePosicionCatalogoPAC,
                            Valor           = x.Valor,
                            SaldoPorObligar = x.SaldoPorObligar,
                            codlineapago    = x.CodigoLineaPago,
                            nmlineapago     = x.NombreLineaPago
                        }).ToList()
                    };

                    var swSaveLegacy = Stopwatch.StartNew();
                    await _repository.SaveAsync(nuevoComp, cancellationToken);
                    swSaveLegacy.Stop();

                    await _audit.InfoAsync(
                        idTramite,
                        SiifPuntoDeControl.InsercionBd,
                        $"💾 Tabla legacy (tblccompptal) guardada OK. IdCompromiso={response.Codigo}.",
                        peticion: null, respuesta: null, tiempoRq: null,
                        tiempoRs: $"{swSaveLegacy.ElapsedMilliseconds} ms",
                        estadoFinal: null, nmUserUpdate: usuario, cancellationToken);
                }
                catch (Exception ex)
                {
                    totalSw.Stop();

                    // 💥 Error al insertar en tabla legacy: se registra con stack trace completo
                    // y se re-lanza para que el caller pueda manejar el fallo correctamente.
                    await _audit.ErrorAsync(
                        idTramite,
                        SiifPuntoDeControl.FinError,
                        $"💥 Fallo al guardar tabla legacy (tblccompptal). IdCompromiso={response.Codigo}. {ex.Message}",
                        peticion: null, respuesta: null, tiempoRq: null,
                        tiempoRs: $"{totalSw.ElapsedMilliseconds} ms",
                        estadoFinal: SiifAuditoriaEstadoFinal.Fallido,
                        nmUserUpdate: usuario, exception: ex, cancellationToken);

                    throw;
                }
            }
            else
            {
                // 📌 El registro ya existe en la tabla legacy: se omite la inserción (idempotencia).
                await _audit.InfoAsync(
                    idTramite,
                    SiifPuntoDeControl.ActualizacionBd,
                    $"♻️ IdCompromiso={response.Codigo} ya existe en tabla legacy. Se omite inserción (idempotente).",
                    peticion: null, respuesta: null, tiempoRq: null,
                    tiempoRs: $"{swLegacy.ElapsedMilliseconds} ms",
                    estadoFinal: null, nmUserUpdate: usuario, cancellationToken);
            }

            // ── 6. Persistir tablas producción (DYNTBLCCOMPPTAL + DYNTBLLISTITEMSAFE) ────
            var swDynCheck  = Stopwatch.StartNew();
            bool dynExiste  = await _dynRepository.ExistsAsync(response.Codigo, cancellationToken);
            swDynCheck.Stop();

            if (!dynExiste)
            {
                try
                {
                    var dynComp = new DynTblCCompPtal
                    {
                        // 🆔 Gestordoc: Oid = GUID sin guiones en uppercase (convención del esquema)
                        Oid             = Guid.NewGuid().ToString("N").ToUpperInvariant(),
                        NrVersion       = 1,
                        BnCreated       = bnCreated,
                        FgEnabled       = 1,
                        FgSystem        = 0,
                        BnUpdated       = 0,
                        IdCompromiso    = response.Codigo,
                        VigenciaNm      = response.Vigencia,
                        FechaRegistro   = response.FechaRegistro,
                        Estado          = response.Estado,
                        CodCdp          = response.CodigoCdp,
                        FechaCdp        = response.FechaCdp,
                        CodMoneda       = response.CodigoMoneda,
                        NmMoneda        = response.NombreMoneda,
                        ValorTasa       = response.ValorTasa,
                        Descripcion     = response.Descripcion,
                        Objeto          = response.Objeto,
                        ValorInicial    = response.ValorInicial,
                        VlIniOriMoneda  = response.ValorInicialOriginalMoneda,
                        VlTOperacion    = response.ValorTotalOperacion,
                        ValorActual     = response.ValorActual,
                        SaldoXObligar   = response.SaldoPorObligar,
                        SaldoMoneda     = response.SaldoMoneda,
                        TtDocumento     = response.Tercero?.TipoDocumento,
                        TnDocumento     = response.Tercero?.NumeroDocumento,
                        TerceroNm       = response.Tercero?.Nombre,
                        MedioPago       = response.MedioPago,
                        CuentaNn        = response.DetalleCuentaBancaria?.Numero,
                        CuentaEntFinan  = response.DetalleCuentaBancaria?.EntidadFinanciera,
                        CuentaTipo      = response.DetalleCuentaBancaria?.TipoCuenta,
                        CuentaEstado    = response.DetalleCuentaBancaria?.Estado,
                        OrdenadorTDoc   = response.DetalleOrdenadorGasto?.TipoDocumento,
                        OrdenadorNDoc   = response.DetalleOrdenadorGasto?.NumeroDocumento,
                        OrdenadorNm     = response.DetalleOrdenadorGasto?.Nombre,
                        OrdenadorConsec = response.DetalleOrdenadorGasto?.Consecutivo,
                        OrdenadorCodCar = response.DetalleOrdenadorGasto?.CodigoCargo,
                        OrdenadorNmCarg = response.DetalleOrdenadorGasto?.NombreCargo,
                        NnDocSoporte    = response.DatosAdministrativos?.NumeroDocumentoSoporte,
                        TDocSoporte     = response.DatosAdministrativos?.TipoDocumentoSoporte,
                        DtDocSoporte    = response.DatosAdministrativos?.Fecha,
                        CajaMenor       = response.CajaMenor?.ToString(),
                        FechaCarga      = ahora,
                        Items = response.ListadoItemsAfectacion.Select(x => new DynTblListItemsAfe
                        {
                            // 🆔 Cada ítem de afectación tiene su propio Oid único
                            Oid           = Guid.NewGuid().ToString("N").ToUpperInvariant(),
                            NrVersion     = 1,
                            BnCreated     = bnCreated,
                            FgEnabled     = 1,
                            FgSystem      = 0,
                            BnUpdated     = 0,
                            IdCompromiso  = response.Codigo,
                            CodDepAfecta  = x.CodigoDependenciaAfectacion,
                            NmDepAfecta   = x.NombreDependenciaAfectacion,
                            CodPGasto     = x.CodigoPosicionGasto,
                            NmPGasto      = x.NombrePosicionGasto,
                            CodFFinan     = x.CodigoFuenteFinanciacion,
                            NmFFinan      = x.NombreFuenteFinanciacion,
                            CodRPPtal     = x.CodigoRecursoPresupuestal,
                            NmRPPtal      = x.NombreRecursoPresupuestal,
                            CodSFondo     = x.CodigoSituacionFondos,
                            NmSFondo      = x.NombreSituacionFondos,
                            VlInicial     = x.ValorInicial,
                            VlOperaciones = x.ValorOperaciones,
                            VlActual      = x.ValorActual,
                            Saldo         = x.Saldo,
                            FechaCarga    = ahora,
                        }).ToList()
                    };

                    var swDynSave = Stopwatch.StartNew();
                    await _dynRepository.SaveAsync(dynComp, cancellationToken);
                    swDynSave.Stop();

                    await _audit.InfoAsync(
                        idTramite,
                        SiifPuntoDeControl.InsercionBd,
                        $"💾 Tablas producción (DYNTBLCCOMPPTAL + DYNTBLLISTITEMSAFE) guardadas OK. IdCompromiso={response.Codigo}, Items={dynComp.Items.Count}.",
                        peticion: null, respuesta: null, tiempoRq: null,
                        tiempoRs: $"{swDynSave.ElapsedMilliseconds} ms",
                        estadoFinal: null, nmUserUpdate: usuario, cancellationToken);
                }
                catch (Exception ex)
                {
                    totalSw.Stop();

                    // 💥 Error al insertar en tablas DYN: registrado con detalle completo.
                    await _audit.ErrorAsync(
                        idTramite,
                        SiifPuntoDeControl.FinError,
                        $"💥 Fallo DYN SaveAsync. IdCompromiso={response.Codigo}. Inner: {ex.InnerException?.Message ?? ex.Message}",
                        peticion: null, respuesta: null, tiempoRq: null,
                        tiempoRs: $"{totalSw.ElapsedMilliseconds} ms",
                        estadoFinal: SiifAuditoriaEstadoFinal.Fallido,
                        nmUserUpdate: usuario, exception: ex, cancellationToken);

                    throw;
                }
            }
            else
            {
                // 📌 Idempotencia: el compromiso ya existía en DYNTBLCCOMPPTAL.
                await _audit.InfoAsync(
                    idTramite,
                    SiifPuntoDeControl.ActualizacionBd,
                    $"♻️ IdCompromiso={response.Codigo} ya existe en DYNTBLCCOMPPTAL. Se omite inserción (idempotente).",
                    peticion: null, respuesta: null, tiempoRq: null,
                    tiempoRs: $"{swDynCheck.ElapsedMilliseconds} ms",
                    estadoFinal: null, nmUserUpdate: usuario, cancellationToken);
            }

            // ── 7. Fin exitoso ──────────────────────────────────────────────────────────
            totalSw.Stop();

            await _audit.InfoAsync(
                idTramite,
                SiifPuntoDeControl.FinExitoso,
                $"✅ Fin flujo CompromisoPresupuestalRP exitoso. IdCompromiso={response.Codigo}, msTotal={totalSw.ElapsedMilliseconds}.",
                peticion: null, respuesta: null, tiempoRq: null,
                tiempoRs: $"{totalSw.ElapsedMilliseconds} ms",
                estadoFinal: SiifAuditoriaEstadoFinal.Exitoso,
                nmUserUpdate: usuario, cancellationToken);

            return response;
        }
    }
}