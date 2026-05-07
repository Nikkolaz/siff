#pragma warning disable CA1416 // EventLog solo en Windows; el proyecto Worker se despliega en Windows como servicio.
using System.Diagnostics;
using System.Text;

namespace SSF.Interop.SIIFNacion.Worker;

/// <summary>
/// Escribe fallos de arranque en el registro de eventos de Windows cuando aún no hay logging configurado o la BD no está disponible.
/// </summary>
internal static class WorkerEventLogDiagnostics
{
    private const string SourceName = "SSF.Interop.SIIFNacion.Worker";
    private const string LogName = "Application";

    /// <summary>Registra una excepción completa (mensajes anidados y stack traces) en el Visor de eventos.</summary>
    public static void TryWriteException(string phase, Exception ex)
    {
        if (!OperatingSystem.IsWindows())
            return;

        var body = FormatExceptionChain(phase, ex);
        try
        {
            EnsureSourceExists();
            EventLog.WriteEntry(SourceName, body, EventLogEntryType.Error, eventID: 1001);
        }
        catch
        {
            TryWriteFallbackFile(body);
        }
    }

    private static void EnsureSourceExists()
    {
        if (EventLog.SourceExists(SourceName))
            return;
        try
        {
            EventLog.CreateEventSource(new EventSourceCreationData(SourceName, LogName));
        }
        catch
        {
            // Sin permisos para crear origen: el archivo de respaldo queda en %TEMP%.
        }
    }

    private static void TryWriteFallbackFile(string body)
    {
        try
        {
            var path = Path.Combine(Path.GetTempPath(), "SSF.Interop.SIIFNacion.Worker-startup-error.log");
            File.AppendAllText(path, $"[{DateTime.UtcNow:u}] {body}{Environment.NewLine}{Environment.NewLine}", Encoding.UTF8);
        }
        catch
        {
            // ignorado
        }
    }

    internal static string FormatExceptionChain(string phase, Exception ex)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Fase: {phase}");
        sb.AppendLine($"Hora (UTC): {DateTime.UtcNow:O}");
        sb.AppendLine($"Directorio base: {AppContext.BaseDirectory}");
        sb.AppendLine($"Directorio actual: {Environment.CurrentDirectory}");
        sb.AppendLine();

        var depth = 0;
        for (Exception? e = ex; e != null; e = e.InnerException, depth++)
        {
            if (depth > 0)
            {
                sb.AppendLine();
                sb.AppendLine($"--- InnerException #{depth} ---");
            }

            sb.AppendLine($"Tipo: {e.GetType().FullName}");
            sb.AppendLine($"Mensaje: {e.Message}");
            if (!string.IsNullOrEmpty(e.StackTrace))
            {
                sb.AppendLine("StackTrace:");
                sb.AppendLine(e.StackTrace);
            }

            if (e is AggregateException agg)
            {
                var i = 0;
                foreach (var inner in agg.InnerExceptions)
                {
                    sb.AppendLine();
                    sb.AppendLine($"--- AggregateException.InnerExceptions[{i++}] ---");
                    sb.Append(inner.ToString());
                }
            }
        }

        return sb.ToString();
    }
}
