using SSF.Interop.SIIFNacion.Service.Domain.Auditoria;

namespace SSF.Interop.SIIFNacion.Service.Application.Contracts.Persistence
{
    public interface ILogsErroresEtl
    {
        Task<LogsErroresEtl> AddAsync(LogsErroresEtl entity);
    }
}
