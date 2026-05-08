namespace SSF.Interop.SIIFNacion.Application.Common.Interfaces
{
    public interface ICustomLogger
    {
        Task LogEtapaAsync(Guid IdEtapaEtl, string codigoEtapa, string codigoEstado, string mensaje, Guid idConvenio, Guid idEstructura, string workerId, string periodo, Guid idTransaccion, long? registrosProcesados = null);
        Task LogErrorAsync(Guid IdEtapaEtl, string message, Exception ex, Guid idTransaccion, string? context = null);

    }
}
