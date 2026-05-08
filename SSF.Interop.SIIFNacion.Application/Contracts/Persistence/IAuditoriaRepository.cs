using System.Threading;
using System.Threading.Tasks;
using SSF.Interop.SIIFNacion.Domain.Auditoria;

namespace SSF.Interop.SIIFNacion.Application.Contracts.Persistence
{
    public interface IAuditoriaRepository
    {
        Task AddAsync(DynEfwAuditoria row, CancellationToken cancellationToken);
        Task SaveChangesAsync(CancellationToken cancellationToken);
    }
}

