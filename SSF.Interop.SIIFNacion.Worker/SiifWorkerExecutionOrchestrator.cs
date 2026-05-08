using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SSF.Interop.SIIFNacion.Application.Common.Auditoria;
using SSF.Interop.SIIFNacion.Application.Common.SIIF;
using SSF.Interop.SIIFNacion.Application.Features.SIIF.Requests.Commands.SincronizarListaObligaciones;
using SSF.Interop.SIIFNacion.Application.Features.SIIF.Requests.Queries;

namespace SSF.Interop.SIIFNacion.Worker
{
    /// <summary>
    /// Coordina un “ciclo” de integración: dispara en paralelo los casos de uso SIIF ya implementados como handlers MediatR.
    /// No contiene reglas de negocio duplicadas: solo compone requests y mide/registra a nivel de worker.
    /// </summary>
    public sealed class SiifWorkerExecutionOrchestrator
    {
        // Mapeo explícito pedido por negocio: valor Vigencia en el comando → nombre legible en logs/auditoría.
        private static readonly (string Vigencia, string Etiqueta)[] ObligacionesPorVigencia =
        {
            ("1", "ObligacionesPaginadaActual"),
            ("2", "ObligacionesPaginadaReservas"),
            ("3", "PaginadaCXP")
        };

        // Necesario para crear un scope independiente por tarea paralela (DbContext no es thread-safe).
        private readonly IServiceScopeFactory _scopeFactory;

        // Snapshot de configuración del worker (headers por job, flags Enabled, etc.).
        private readonly IOptionsMonitor<SiifWorkerOptions> _options;

        // Log de aplicación (consola/archivo) complementario a IAuditoriaLogger en base de datos.
        private readonly ILogger<SiifWorkerExecutionOrchestrator> _logger;

        /// <summary>DI estándar.</summary>
        public SiifWorkerExecutionOrchestrator(
            IServiceScopeFactory scopeFactory,
            IOptionsMonitor<SiifWorkerOptions> options,
            ILogger<SiifWorkerExecutionOrchestrator> logger)
        {
            _scopeFactory = scopeFactory;
            _options = options;
            _logger = logger;
        }

        /// <summary>
        /// Ejecuta todos los jobs habilitados en paralelo y escribe auditoría de inicio/fin de ciclo.
        /// </summary>
        /// <param name="idCiclo">Identificador único de la corrida (se propaga como prefijo en id de trámite por flujo).</param>
        /// <param name="cancellationToken">Token de cancelación del host.</param>
        public async Task ExecuteAsync(string idCiclo, CancellationToken cancellationToken)
        {
            // Configuración actual del worker (puede haber cambiado desde el último ciclo si se usa IOptionsMonitor).
            var opt = _options.CurrentValue;

            // Zona para definir “hoy” al construir el rango año-hasta-hoy (YTD) de obligaciones y CDP paginado.
            var tz = ResolveTimeZone(opt.TimeZoneId);

            // Fecha calendario local en esa zona (sin hora; solo importa el día para el rango YTD).
            var todayLocal = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz).Date;

            // Fechas tipo DateTime para el comando de obligaciones (FechaInicio/FechaFin).
            var (fechaIni, fechaFin) = SiifWorkerPeriodo.YearToDateLocal(todayLocal);

            // Mismas fechas como strings ISO para el query de CDP paginado (FechaRegistroIni/Fin).
            var (fechaIniIso, fechaFinIso) = SiifWorkerPeriodo.YearToDateIsoStrings(todayLocal);

            // Usuario en auditoría de ciclo: primer LoginUsuarioSiifHeader no vacío entre jobs (cada flujo usa el suyo).
            var usuarioCiclo = LoginUsuarioParaAuditoriaCiclo(opt);

            // Auditoría de apertura de ciclo: un scope corto solo para IAuditoriaLogger (evita mezclar DbContext con las tareas paralelas).
            await using (var outer = _scopeFactory.CreateAsyncScope())
            {
                var audit = outer.ServiceProvider.GetRequiredService<IAuditoriaLogger>();
                await audit.InfoAsync(
                    idCiclo,
                    SiifPuntoDeControl.WorkerCicloInicio,
                    $"Inicio ciclo worker SIIF. Zona={opt.TimeZoneId}, YTD oblig/CDP paginado: {fechaIni:yyyy-MM-dd}..{fechaFin:yyyy-MM-dd}.",
                    $"Cron={opt.CronExpression}",
                    null,
                    null,
                    null,
                    null,
                    usuarioCiclo,
                    cancellationToken);
            }

            // Cronómetro del ciclo completo (pared): incluye paralelismo (tiempo “calendario”, no suma de hilos).
            var totalSw = Stopwatch.StartNew();

            // Lista de tareas que se van a lanzar juntas con Task.WhenAll.
            var tareas = new List<Task<FlowOutcome>>();

            // Job CDP (no usa fechas YTD; requiere IdentificacionPCI + ConsecutivoCDP en configuración).
            if (opt.Cdp.Enabled)
            {
                if (string.IsNullOrWhiteSpace(opt.Cdp.IdentificacionPCI) || string.IsNullOrWhiteSpace(opt.Cdp.ConsecutivoCDP))
                    _logger.LogWarning("SiifWorker: CDP habilitado pero faltan IdentificacionPCI o ConsecutivoCDP; flujo omitido.");
                else
                    tareas.Add(EjecutarCdpAsync(idCiclo, opt, cancellationToken));
            }

            // Job CDP paginado: fechas YTD inyectadas aquí, no leídas del JSON de configuración.
            if (opt.CdpPaginado.Enabled)
                tareas.Add(EjecutarCdpPaginadoAsync(idCiclo, opt, fechaIniIso, fechaFinIso, cancellationToken));

            // Tres comandos de obligaciones independientes (misma configuración base, distinta Vigencia).
            if (opt.Obligaciones.Enabled)
            {
                foreach (var (vigencia, etiqueta) in ObligacionesPorVigencia)
                {
                    // Copia local para cerrar correctamente sobre el closure de la lambda/Task (evita captura incorrecta en bucles).
                    var v = vigencia;
                    var e = etiqueta;
                    tareas.Add(EjecutarObligacionesAsync(idCiclo, opt, fechaIni, fechaFin, v, e, cancellationToken));
                }
            }

            // Nada que ejecutar: aún así dejamos rastro en auditoría y salimos sin WhenAll vacío.
            if (tareas.Count == 0)
            {
                _logger.LogWarning("SiifWorker: ningún flujo habilitado o configurado; ciclo sin trabajo.");
                totalSw.Stop();
                await RegistrarFinCicloAsync(idCiclo, usuarioCiclo, totalSw.ElapsedMilliseconds, Array.Empty<FlowOutcome>(), cancellationToken);
                return;
            }

            // Paralelismo: cada Task interna crea su propio scope antes de tocar EF o MediatR.
            var resultados = await Task.WhenAll(tareas);
            totalSw.Stop();

            // Resumen agregado en auditoría (éxito global o parcial si algún flujo falló pero otros terminaron).
            await RegistrarFinCicloAsync(idCiclo, usuarioCiclo, totalSw.ElapsedMilliseconds, resultados, cancellationToken);
        }

        /// <summary>
        /// Escribe el registro final del ciclo con tiempos y estado agregado (EXITOSO vs PARCIAL).
        /// </summary>
        private async Task RegistrarFinCicloAsync(
            string idCiclo,
            string? usuario,
            long msTotal,
            IReadOnlyList<FlowOutcome> resultados,
            CancellationToken cancellationToken)
        {
            // Éxito solo si no hubo flujos o todos reportaron Exito=true.
            var ok = resultados.Count == 0 || resultados.All(r => r.Exito);

            // PARCIAL indica que al menos un flujo falló pero el proceso no abortó los demás en paralelo.
            var estado = ok ? SiifAuditoriaEstadoFinal.Exitoso : SiifAuditoriaEstadoFinal.Parcial;

            // Texto compacto para columna MENSAJE / lectura humana en auditoría.
            var detalle = resultados.Count == 0
                ? "Sin flujos."
                : string.Join("; ", resultados.Select(r => $"{r.Nombre}={(r.Exito ? "OK" : "FALLO")} {r.MsFlujo}ms"));

            await using var outer = _scopeFactory.CreateAsyncScope();
            var audit = outer.ServiceProvider.GetRequiredService<IAuditoriaLogger>();
            await audit.InfoAsync(
                idCiclo,
                SiifPuntoDeControl.WorkerCicloFin,
                $"Fin ciclo worker SIIF. msTotal={msTotal}. {detalle}",
                null,
                null,
                null,
                $"{msTotal} ms",
                estado,
                usuario,
                cancellationToken);
        }

        /// <summary>Ejecuta <see cref="ConsultarCdpQuery"/> dentro de un scope propio.</summary>
        private async Task<FlowOutcome> EjecutarCdpAsync(
            string idCiclo,
            SiifWorkerOptions opt,
            CancellationToken cancellationToken)
        {
            // Sufijo en id de trámite para filtrar en auditoría por tipo de flujo.
            var id = $"{idCiclo}:cdp";

            // Medición de este flujo de extremo a extremo (incluye HTTP SIIF + persistencia en handler).
            var sw = Stopwatch.StartNew();

            // Scope dedicado: nueva instancia de DbContext y MediatR pipeline para este hilo.
            await using var scope = _scopeFactory.CreateAsyncScope();
            var audit = scope.ServiceProvider.GetRequiredService<IAuditoriaLogger>();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            var c = opt.Cdp;
            var h = c.Headers;
            var usuario = LoginUsuarioParaAuditoria(h);

            try
            {
                await audit.InfoAsync(
                    id,
                    SiifPuntoDeControl.WorkerFlujoInicio,
                    "Worker: inicio ConsultarCdp (MediatR). Los tiempos por petición SIIF quedan en el handler / auditoría de integración.",
                    $"IdentificacionPCI={c.IdentificacionPCI}; ConsecutivoCDP={c.ConsecutivoCDP}",
                    null,
                    null,
                    null,
                    null,
                    usuario,
                    cancellationToken);

                // Un solo round-trip SIIF; el handler persiste cabecera + ítems si viene expedicionCDPSal.
                await mediator.Send(
                    new ConsultarCdpQuery
                    {
                        CodPciHeader = h.CodPciHeader,
                        LoginUsuarioSiifHeader = h.LoginUsuarioSiifHeader,
                        ConsecutivoHeader = h.ConsecutivoHeader,
                        HashHeader = h.HashHeader,
                        IdentificacionPCI = c.IdentificacionPCI,
                        ConsecutivoCDP = c.ConsecutivoCDP
                    },
                    cancellationToken);

                sw.Stop();
                await audit.InfoAsync(
                    id,
                    SiifPuntoDeControl.WorkerFlujoFin,
                    $"Worker: fin ConsultarCdp OK. msFlujo={sw.ElapsedMilliseconds}",
                    null,
                    null,
                    null,
                    $"{sw.ElapsedMilliseconds} ms",
                    SiifAuditoriaEstadoFinal.Exitoso,
                    usuario,
                    cancellationToken);
                _logger.LogInformation("SiifWorker: CDP completado en {Ms} ms.", sw.ElapsedMilliseconds);
                return new FlowOutcome(true, "CDP", sw.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                sw.Stop();
                await audit.ErrorAsync(
                    id,
                    SiifPuntoDeControl.WorkerFlujoError,
                    $"Worker: error ConsultarCdp. msFlujo={sw.ElapsedMilliseconds}",
                    null,
                    null,
                    null,
                    $"{sw.ElapsedMilliseconds} ms",
                    SiifAuditoriaEstadoFinal.Fallido,
                    usuario,
                    ex,
                    cancellationToken);
                _logger.LogError(ex, "SiifWorker: error en CDP.");
                return new FlowOutcome(false, "CDP", sw.ElapsedMilliseconds);
            }
        }

        /// <summary>Ejecuta <see cref="ConsultarCdpPaginadaQuery"/> con fechas YTD ya resueltas.</summary>
        private async Task<FlowOutcome> EjecutarCdpPaginadoAsync(
            string idCiclo,
            SiifWorkerOptions opt,
            string fechaIniIso,
            string fechaFinIso,
            CancellationToken cancellationToken)
        {
            var id = $"{idCiclo}:cdp-paginado";
            var sw = Stopwatch.StartNew();
            await using var scope = _scopeFactory.CreateAsyncScope();
            var audit = scope.ServiceProvider.GetRequiredService<IAuditoriaLogger>();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            var p = opt.CdpPaginado;
            var h = p.Headers;
            var usuario = LoginUsuarioParaAuditoria(h);

            try
            {
                await audit.InfoAsync(
                    id,
                    SiifPuntoDeControl.WorkerFlujoInicio,
                    "Worker: inicio ConsultarCdpPaginada (MediatR). Rango YTD automático.",
                    $"FechaRegistroIni={fechaIniIso}; FechaRegistroFin={fechaFinIso}",
                    null,
                    null,
                    null,
                    null,
                    usuario,
                    cancellationToken);

                // Page/Size aquí son informativos respecto al contrato HTTP; el handler itera internamente con tamaño fijo.
                await mediator.Send(
                    new ConsultarCdpPaginadaQuery
                    {
                        CodPciHeader = h.CodPciHeader,
                        LoginUsuarioSiifHeader = h.LoginUsuarioSiifHeader,
                        ConsecutivoHeader = h.ConsecutivoHeader,
                        HashHeader = h.HashHeader,
                        Page = 1,
                        Size = 50,
                        PCIConsulta = p.PCIConsulta,
                        PCISubUnidades = p.PCISubUnidades,
                        FechaRegistroIni = fechaIniIso,
                        FechaRegistroFin = fechaFinIso,
                        TipoGasto = p.TipoGasto,
                        Rango = p.Rango
                    },
                    cancellationToken);

                sw.Stop();
                await audit.InfoAsync(
                    id,
                    SiifPuntoDeControl.WorkerFlujoFin,
                    $"Worker: fin CDP paginado OK. msFlujo={sw.ElapsedMilliseconds}",
                    null,
                    null,
                    null,
                    $"{sw.ElapsedMilliseconds} ms",
                    SiifAuditoriaEstadoFinal.Exitoso,
                    usuario,
                    cancellationToken);
                _logger.LogInformation("SiifWorker: CDP paginado completado en {Ms} ms.", sw.ElapsedMilliseconds);
                return new FlowOutcome(true, "CDP_Paginado", sw.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                sw.Stop();
                await audit.ErrorAsync(
                    id,
                    SiifPuntoDeControl.WorkerFlujoError,
                    $"Worker: error CDP paginado. msFlujo={sw.ElapsedMilliseconds}",
                    null,
                    null,
                    null,
                    $"{sw.ElapsedMilliseconds} ms",
                    SiifAuditoriaEstadoFinal.Fallido,
                    usuario,
                    ex,
                    cancellationToken);
                _logger.LogError(ex, "SiifWorker: error en CDP paginado.");
                return new FlowOutcome(false, "CDP_Paginado", sw.ElapsedMilliseconds);
            }
        }

        /// <summary>Ejecuta <see cref="SincronizarListaObligacionesCommand"/> para una vigencia concreta.</summary>
        private async Task<FlowOutcome> EjecutarObligacionesAsync(
            string idCiclo,
            SiifWorkerOptions opt,
            DateTime fechaIni,
            DateTime fechaFin,
            string vigencia,
            string etiqueta,
            CancellationToken cancellationToken)
        {
            var id = $"{idCiclo}:obligaciones:v{vigencia}";
            var sw = Stopwatch.StartNew();
            await using var scope = _scopeFactory.CreateAsyncScope();
            var audit = scope.ServiceProvider.GetRequiredService<IAuditoriaLogger>();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            var o = opt.Obligaciones;
            var h = o.Headers;
            var usuario = LoginUsuarioParaAuditoria(h);

            try
            {
                await audit.InfoAsync(
                    id,
                    SiifPuntoDeControl.WorkerFlujoInicio,
                    $"Worker: inicio SincronizarListaObligaciones ({etiqueta}, Vigencia={vigencia}). Rango YTD.",
                    $"FechaInicio={fechaIni:yyyy-MM-dd}; FechaFin={fechaFin:yyyy-MM-dd}",
                    null,
                    null,
                    null,
                    null,
                    usuario,
                    cancellationToken);

                // Vigencia distingue el “modo” de listado en SIIF (1/2/3); el handler aplica ANIOVIGENCIA + VIGENCIACOD al persistir.
                await mediator.Send(
                    new SincronizarListaObligacionesCommand
                    {
                        CodPciHeader = h.CodPciHeader,
                        LoginUsuarioSiifHeader = h.LoginUsuarioSiifHeader,
                        ConsecutivoHeader = h.ConsecutivoHeader,
                        HashHeader = h.HashHeader,
                        CodPCI = o.CodPCI,
                        FechaInicio = fechaIni,
                        FechaFin = fechaFin,
                        TipoGasto = o.TipoGasto,
                        Rango = o.Rango,
                        Vigencia = vigencia,
                        DetalleUsosPresupuestales = o.DetalleUsosPresupuestales
                    },
                    cancellationToken);

                sw.Stop();
                await audit.InfoAsync(
                    id,
                    SiifPuntoDeControl.WorkerFlujoFin,
                    $"Worker: fin obligaciones ({etiqueta}) OK. msFlujo={sw.ElapsedMilliseconds}",
                    null,
                    null,
                    null,
                    $"{sw.ElapsedMilliseconds} ms",
                    SiifAuditoriaEstadoFinal.Exitoso,
                    usuario,
                    cancellationToken);
                _logger.LogInformation("SiifWorker: Obligaciones vigencia {V} ({Et}) en {Ms} ms.", vigencia, etiqueta, sw.ElapsedMilliseconds);
                return new FlowOutcome(true, $"Obligaciones_V{vigencia}", sw.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                sw.Stop();
                await audit.ErrorAsync(
                    id,
                    SiifPuntoDeControl.WorkerFlujoError,
                    $"Worker: error obligaciones ({etiqueta}). msFlujo={sw.ElapsedMilliseconds}",
                    null,
                    null,
                    null,
                    $"{sw.ElapsedMilliseconds} ms",
                    SiifAuditoriaEstadoFinal.Fallido,
                    usuario,
                    ex,
                    cancellationToken);
                _logger.LogError(ex, "SiifWorker: error en obligaciones vigencia {V}.", vigencia);
                return new FlowOutcome(false, $"Obligaciones_V{vigencia}", sw.ElapsedMilliseconds);
            }
        }

        /// <summary>Login para columnas de auditoría de un flujo concreto.</summary>
        private static string? LoginUsuarioParaAuditoria(SiifWorkerHeaders headers) =>
            string.IsNullOrWhiteSpace(headers.LoginUsuarioSiifHeader) ? null : headers.LoginUsuarioSiifHeader.Trim();

        /// <summary>Primer login no vacío (CDP → paginado → obligaciones) para registros de ciclo.</summary>
        private static string? LoginUsuarioParaAuditoriaCiclo(SiifWorkerOptions opt)
        {
            var u = LoginUsuarioParaAuditoria(opt.Cdp.Headers);
            if (u != null) return u;
            u = LoginUsuarioParaAuditoria(opt.CdpPaginado.Headers);
            if (u != null) return u;
            return LoginUsuarioParaAuditoria(opt.Obligaciones.Headers);
        }

        /// <summary>Misma política que en el BackgroundService: id inválido → UTC.</summary>
        private static TimeZoneInfo ResolveTimeZone(string? timeZoneId)
        {
            var id = string.IsNullOrWhiteSpace(timeZoneId) ? "UTC" : timeZoneId.Trim();
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById(id);
            }
            catch
            {
                return TimeZoneInfo.Utc;
            }
        }

        // Resultado mínimo por flujo: Exito = MediatR sin excepción; Nombre = etiqueta en resumen; MsFlujo = ms de pared del flujo.
        private readonly record struct FlowOutcome(bool Exito, string Nombre, long MsFlujo);
    }
}
