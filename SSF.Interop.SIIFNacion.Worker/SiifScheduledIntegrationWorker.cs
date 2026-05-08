using Cronos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace SSF.Interop.SIIFNacion.Worker
{
    /// <summary>
    /// <see cref="BackgroundService"/> que interpreta <see cref="SiifWorkerOptions.CronExpression"/> con la librería Cronos
    /// y, en cada disparo, crea un scope DI y ejecuta <see cref="SiifWorkerExecutionOrchestrator"/>.
    /// </summary>
    public sealed class SiifScheduledIntegrationWorker : BackgroundService
    {
        // Fábrica de scopes: cada ciclo y cada flujo paralelo interno obtiene sus propios servicios Scoped (DbContext, MediatR pipeline, etc.).
        private readonly IServiceScopeFactory _scopeFactory;

        // Opciones con recarga: si se cambia appsettings en caliente (según proveedor), CurrentValue refleja el nuevo valor.
        private readonly IOptionsMonitor<SiifWorkerOptions> _options;

        // Logger estándar de Microsoft para consola / proveedores configurados en Logging:*.
        private readonly ILogger<SiifScheduledIntegrationWorker> _logger;

        /// <summary>
        /// Constructor con inyección típica de host genérico.
        /// </summary>
        public SiifScheduledIntegrationWorker(
            IServiceScopeFactory scopeFactory,
            IOptionsMonitor<SiifWorkerOptions> options,
            ILogger<SiifScheduledIntegrationWorker> logger)
        {
            _scopeFactory = scopeFactory;
            _options = options;
            _logger = logger;
        }

        /// <summary>
        /// Punto de entrada del runtime del Worker: valida opciones, opcionalmente ejecuta al arranque y entra al bucle de espera por cron.
        /// </summary>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Respeto del interruptor maestro: no hacer trabajo ni ocupar CPU en bucle si está deshabilitado.
            if (!_options.CurrentValue.Enabled)
            {
                _logger.LogInformation("SiifWorker deshabilitado (SiifWorker:Enabled=false).");
                return;
            }

            // Parseo único del cron; si la expresión es inválida, fallar rápido al iniciar (mejor que quedar en estado incierto).
            CronExpression cron;
            try
            {
                cron = CronExpression.Parse(_options.CurrentValue.CronExpression, CronFormat.Standard);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "SiifWorker: expresión cron inválida ({Expr}).", _options.CurrentValue.CronExpression);
                throw;
            }

            // Primer ciclo inmediato útil en desarrollo o para “ponerse al día” tras un despliegue.
            if (_options.CurrentValue.RunOnStartup)
                await EjecutarCicloAsync(stoppingToken).ConfigureAwait(false);

            // Bucle principal: calcular próxima ocurrencia, dormir hasta entonces, ejecutar ciclo, repetir.
            while (!stoppingToken.IsCancellationRequested)
            {
                // Relectura por si Enabled pasó a false en caliente.
                var opts = _options.CurrentValue;
                if (!opts.Enabled)
                    break;

                // Zona usada por Cronos para interpretar “ahora” vs expresión (debe existir en el SO o caerá a UTC en el orquestador también).
                var tz = ResolveTimeZone(opts.TimeZoneId);

                // Siguiente instante estrictamente después de “ahora” UTC según reglas del cron en esa zona.
                var next = cron.GetNextOccurrence(DateTimeOffset.UtcNow, tz, inclusive: false);
                if (next is null)
                {
                    _logger.LogWarning("SiifWorker: la expresión cron no tiene ocurrencias futuras.");
                    break;
                }

                // Cuánto falta hasta esa ocurrencia vista desde el reloj UTC del proceso.
                var delay = next.Value - DateTimeOffset.UtcNow;
                if (delay > TimeSpan.Zero)
                {
                    _logger.LogInformation(
                        "SiifWorker: próxima ejecución local {Local} (UTC {Utc}), espera {Delay}.",
                        TimeZoneInfo.ConvertTimeFromUtc(next.Value.UtcDateTime, tz),
                        next.Value.UtcDateTime,
                        delay);
                    try
                    {
                        // Task.Delay respeta cancelación al apagar el host.
                        await Task.Delay(delay, stoppingToken).ConfigureAwait(false);
                }
                    catch (TaskCanceledException)
                    {
                        // Cierre ordenado del servicio.
                        break;
                    }
                }

                // Un ciclo = correlación + orquestación paralela de todos los jobs habilitados.
                await EjecutarCicloAsync(stoppingToken).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Crea un scope de duración de todo el ciclo para resolver el orquestador Scoped una sola vez por corrida.
        /// </summary>
        private async Task EjecutarCicloAsync(CancellationToken cancellationToken)
        {
            // Identificador de correlación que verás en DYNEFWAUDITORIA enlazando todos los flujos de esta corrida.
            var idCiclo = Guid.NewGuid().ToString("N");

            // Un scope por ciclo: el orquestador es Scoped; los flujos paralelos abren scopes adicionales dentro de él.
            await using var scope = _scopeFactory.CreateAsyncScope();
            var orchestrator = scope.ServiceProvider.GetRequiredService<SiifWorkerExecutionOrchestrator>();

            // Delegación: toda la lógica de negocio vive en Application vía MediatR; aquí solo se agenda.
            await orchestrator.ExecuteAsync(idCiclo, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Resuelve la zona horaria por id; si el id no existe en el SO, se usa UTC para no detener el servicio.
        /// </summary>
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
    }
}
