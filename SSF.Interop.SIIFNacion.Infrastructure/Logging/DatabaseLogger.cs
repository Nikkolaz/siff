using SSF.Interop.SIIFNacion.Application.Common.Interfaces;

namespace SSF.Interop.SIIFNacion.Service.Infrastructure.Logging
{
    public class DatabaseLogger : ICustomLogger
    {
        private readonly IEnumerable<ICustomLogger> _loggers;

        public DatabaseLogger(IEnumerable<ICustomLogger> loggers)
        {
            _loggers = loggers;
        }

         public async Task LogErrorAsync(Guid IdEtapaEtl, string message, Exception ex, Guid idTransaccion, string? context = null)
        {
            foreach (var logger in _loggers)
                await logger.LogErrorAsync(IdEtapaEtl, message, ex, idTransaccion, context);
        }
        public async Task LogEtapaAsync(Guid IdEtapaEtl, string codigoEtapa, string codigoEstado, string mensaje, Guid idConvenio, Guid idEstructura, string workerId, string periodo, Guid idTransaccion, long? registrosProcesados = null)
        {
            foreach (var logger in _loggers)
                await logger.LogEtapaAsync(IdEtapaEtl, codigoEtapa, codigoEstado, mensaje, idConvenio, idEstructura, workerId, periodo, idTransaccion, registrosProcesados);
        }
    }
}