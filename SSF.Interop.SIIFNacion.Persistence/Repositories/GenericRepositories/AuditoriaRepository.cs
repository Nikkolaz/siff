using System.Threading;
using System.Threading.Tasks;
using SSF.Interop.SIIFNacion.Application.Contracts.Persistence;
using SSF.Interop.SIIFNacion.Domain.Auditoria;
using SSF.Interop.SIIFNacion.Persistence.DBContext;

namespace SSF.Interop.SIIFNacion.Persistence.Repositories.GenericRepositories
{
    public sealed class AuditoriaRepository : IAuditoriaRepository
    {
        private readonly GestordocDbContext _context;

        public AuditoriaRepository(GestordocDbContext context)
        {
            _context = context;
        }

        public Task AddAsync(DynEfwAuditoria row, CancellationToken cancellationToken) =>
            _context.AuditoriaIntegracion.AddAsync(row, cancellationToken).AsTask();

        public Task SaveChangesAsync(CancellationToken cancellationToken) =>
            _context.SaveChangesAsync(cancellationToken);
    }
}

