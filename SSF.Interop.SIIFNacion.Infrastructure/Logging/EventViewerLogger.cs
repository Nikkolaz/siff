using Microsoft.Extensions.Logging;
using SSF.Interop.SIIFNacion.Application.Common.Interfaces;

namespace SSF.Interop.SIIFNacion.Service.Infrastructure.Logging
{
    public class EventViewerLogger : ICustomLogger
    {
        private readonly ILogger<EventViewerLogger> _logger;

        public EventViewerLogger(ILogger<EventViewerLogger> logger)
        {
            _logger = logger;
        }
              
        public Task LogErrorAsync(Guid IdEtapaEtl, string message, Exception ex, Guid idTransaccion, string? context = null)
        {
            var enrichedMessage = $"{message} | Context: {context ?? "General"}";
            _logger.LogError(ex, enrichedMessage);
            return Task.CompletedTask;
        }
        public Task LogEtapaAsync(Guid IdEtapaEtl, string codigoEtapa, string codigoEstado, string mensaje, Guid idConvenio, Guid idEstructura, string workerId, string periodo, Guid idTransaccion, long? registrosProcesados = null)
        {
            var enrichedMessage = $"{mensaje} | nombreEtapa: {codigoEtapa ?? "General"} | nombreEtapa: {codigoEstado} | registrosProcesados: {registrosProcesados}";
            _logger.LogInformation(mensaje, enrichedMessage);
            return Task.CompletedTask;
        }
    }
}
