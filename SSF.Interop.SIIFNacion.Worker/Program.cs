// Punto de entrada del proceso en segundo plano (Worker Service) para integraciones SIIF programadas.
// En Windows como servicio: UseWindowsService evita el error 1053 (SCM sin señal de inicio).

using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.EventLog;
using SSF.Interop.SIIFNacion.Application;
using SSF.Interop.SIIFNacion.Infrastructure;
using SSF.Interop.SIIFNacion.Persistence;
using SSF.Interop.SIIFNacion.Worker;

try
{
    if (OperatingSystem.IsWindows())
    {
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
        {
            if (e.ExceptionObject is Exception ex)
                WorkerEventLogDiagnostics.TryWriteException("AppDomain.UnhandledException", ex);
        };
        TaskScheduler.UnobservedTaskException += (_, e) =>
        {
            e.SetObserved();
            WorkerEventLogDiagnostics.TryWriteException("TaskScheduler.UnobservedTaskException", e.Exception);
        };
    }

    // Al ejecutarse como "sc create ... dotnet.exe ...\Worker.dll" el directorio de trabajo suele ser
    // System32; sin esto no se encuentra appsettings.json y el host falla antes de loguear.
    var contentRoot = AppContext.BaseDirectory;
    if (!string.IsNullOrEmpty(contentRoot))
        Directory.SetCurrentDirectory(contentRoot);

    var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings
    {
        ContentRootPath = contentRoot,
        Args = args,
    });

    // Obligatorio cuando el proceso corre como servicio Windows (integración con Service Control Manager).
    builder.Services.AddWindowsService();

    if (OperatingSystem.IsWindows())
    {
#pragma warning disable CA1416 // EventLogSettings: solo Windows.
        builder.Logging.AddEventLog(settings =>
        {
            settings.SourceName = "SSF.Interop.SIIFNacion.Worker";
            settings.LogName = "Application";
        });
#pragma warning restore CA1416
    }

    builder.Services.Configure<SiifWorkerOptions>(builder.Configuration.GetSection(SiifWorkerOptions.SectionName));
    builder.Services.ConfigureApplicationServices();
    builder.Services.ConfigureInfrastructureServices(builder.Configuration);

    var (auditoria, proceso) = BuildSqlPair(builder.Configuration);
    builder.Services.ConfigurePresistenceServices(auditoria, proceso);

    builder.Services.AddHostedService<SiifScheduledIntegrationWorker>();
    builder.Services.AddScoped<SiifWorkerExecutionOrchestrator>();

    await builder.Build().RunAsync().ConfigureAwait(false);
}
catch (Exception ex)
{
    WorkerEventLogDiagnostics.TryWriteException("Arranque del host (Program.cs)", ex);
    Environment.Exit(1);
}

/// <summary>
/// Obtiene la cadena de proceso (obligatoria) y la de auditoría (opcional: si falta o está en blanco, se usa la de proceso).
/// </summary>
static (string Auditoria, string Proceso) BuildSqlPair(IConfiguration configuration)
{
    var proceso = configuration["ConnectionStrings:SqlServerProceso:Connection"]
        ?? throw new InvalidOperationException("Falta ConnectionStrings:SqlServerProceso:Connection en configuración.");

    var auditoriaRaw = configuration["ConnectionStrings:SqlServerAuditoria:Connection"];
    var auditoria = string.IsNullOrWhiteSpace(auditoriaRaw) ? proceso : auditoriaRaw;

    return (auditoria, proceso);
}
