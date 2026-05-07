using Microsoft.EntityFrameworkCore;
using SSF.Interop.SIIFNacion.Domain.Cdp;
using SSF.Interop.SIIFNacion.Persistence.DBContext;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SSF.Interop.SIIFNacion.Persistence.Repositories.GenericRepositories
{
    public class CdpPaginadoRepository : ICdpPaginadoRepository
    {
        private readonly DbContextCdp _context;

        public CdpPaginadoRepository(DbContextCdp context)
        {
            _context = context;
        }

        public async Task SaveAsync(CdpPaginado cdp, CancellationToken cancellationToken)
        {
            await _context.Cdps.AddAsync(cdp, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task SaveRangeAsync(IEnumerable<CdpPaginado> cdps, CancellationToken cancellationToken)
        {
            if (cdps == null || !cdps.Any()) return;

            await _context.Cdps.AddRangeAsync(cdps, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<bool> ExistsAsync(string codigoPci, int numeroSolicitud, CancellationToken cancellationToken)
        {
            return await _context.Cdps
                .AnyAsync(c => c.CodPciConexion == codigoPci
                            && c.NnSolicitudCdp == (decimal?)numeroSolicitud, cancellationToken);
        }
    }
}