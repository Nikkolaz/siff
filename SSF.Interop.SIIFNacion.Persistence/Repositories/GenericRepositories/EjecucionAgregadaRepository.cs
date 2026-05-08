using Microsoft.EntityFrameworkCore;
using SSF.Interop.SIIFNacion.Application.Contracts.Persistence;
using SSF.Interop.SIIFNacion.Domain.Siif;
using SSF.Interop.SIIFNacion.Persistence.DBContext;

namespace SSF.Interop.SIIFNacion.Persistence.Repositories.GenericRepositories
{
    /// <summary>
    /// Repositorio concreto para DYNTBLEJECUCIONAGR.
    /// Reutiliza GestordocDbContext igual que ObligacionApoRepository.
    /// </summary>
    public class EjecucionAgregadaRepository : IEjecucionAgregadaRepository
    {
        private readonly GestordocDbContext _context;

        public EjecucionAgregadaRepository(GestordocDbContext context)
        {
            _context = context;
        }

        /// <inheritdoc/>
        public async Task SaveRangeAsync(IEnumerable<SiifEjecucionAgregada> registros, CancellationToken cancellationToken)
        {
            await _context.EjecucionesAgregadas.AddRangeAsync(registros, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<bool> ExistsAsync(decimal anioFiscal, string posicionGasto, CancellationToken cancellationToken)
        {
            return await _context.EjecucionesAgregadas
                .AnyAsync(e => e.AnioFiscal == anioFiscal && e.PosicionGasto == posicionGasto, cancellationToken);
        }
    }
}
