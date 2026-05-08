using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SSF.Interop.SIIFNacion.Application.Contracts.Persistence;
using SSF.Interop.SIIFNacion.Domain.Cdp;
using SSF.Interop.SIIFNacion.Persistence.DBContext;

namespace SSF.Interop.SIIFNacion.Persistence.Repositories.GenericRepositories
{
    public class CdpRepository : ICdpRepository
    {
        private readonly GestordocDbContext _context;

        public CdpRepository(GestordocDbContext context)
        {
            _context = context;
        }

        public async Task SaveCdpAsync(CdpExpedido cabecera, IEnumerable<CdpItem> detalles, CancellationToken cancellationToken)
        {
            await _context.CdpExpedidos.AddAsync(cabecera, cancellationToken);
            await _context.CdpItems.AddRangeAsync(detalles, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}

