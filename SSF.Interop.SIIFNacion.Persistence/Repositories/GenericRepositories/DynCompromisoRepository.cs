using Microsoft.EntityFrameworkCore;
using SSF.Interop.SIIFNacion.Application.Contracts.Persistence;
using SSF.Interop.SIIFNacion.Domain.Cdp;
using SSF.Interop.SIIFNacion.Persistence.DBContext;

namespace SSF.Interop.SIIFNacion.Persistence.Repositories.GenericRepositories
{
    /// <summary>
    /// Repositorio para DYNTBLCCOMPPTAL y DYNTBLLISTITEMSAFE.
    /// Inserta el compromiso padre primero y luego los ítems por separado
    /// para evitar errores de FK con EF Core.
    /// </summary>
    public class DynCompromisoRepository : IDynCompromisoRepository
    {
        private readonly CompromisoDynContext _context;

        public DynCompromisoRepository(CompromisoDynContext context)
        {
            _context = context;
        }

        /// <inheritdoc/>
        public async Task SaveAsync(DynTblCCompPtal compromiso, CancellationToken cancellationToken)
        {
            // Extraer ítems antes de insertar el padre (EF los ignora en el mapeo)
            var items = compromiso.Items.ToList();
            compromiso.Items.Clear();

            // 1. Insertar el compromiso padre
            await _context.Compromisos.AddAsync(compromiso, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            // 2. Insertar los ítems con el OidCompromiso ya confirmado en BD
            if (items.Any())
            {
                await _context.Items.AddRangeAsync(items, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        /// <inheritdoc/>
        public async Task<bool> ExistsAsync(decimal idCompromiso, CancellationToken cancellationToken)
        {
            return await _context.Compromisos
                .AnyAsync(c => c.IdCompromiso.HasValue && c.IdCompromiso.Value == idCompromiso, cancellationToken);
        }
    }
}
