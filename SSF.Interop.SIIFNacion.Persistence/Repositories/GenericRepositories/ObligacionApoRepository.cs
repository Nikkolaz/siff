using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SSF.Interop.SIIFNacion.Application.Contracts.Persistence;
using SSF.Interop.SIIFNacion.Domain.Siif;
using SSF.Interop.SIIFNacion.Persistence.DBContext;

namespace SSF.Interop.SIIFNacion.Persistence.Repositories.GenericRepositories
{
    // Implementación de persistencia: una página de obligaciones por AddRange + SaveChanges (usado en el bucle del handler).
    public class ObligacionApoRepository : IObligacionApoRepository
    {
        private readonly GestordocDbContext _context;

        public ObligacionApoRepository(GestordocDbContext context)
        {
            _context = context;
        }

        public Task DisableByAnioVigenciaAsync(string anioVigencia, string? vigenciaCod, CancellationToken cancellationToken)
        {
            var q = _context.SiifObligacionesApo.Where(x => x.AnioVigencia == anioVigencia);
            if (!string.IsNullOrWhiteSpace(vigenciaCod))
                q = q.Where(x => x.VigenciaCod == vigenciaCod);
            return q.ExecuteUpdateAsync(
                setters => setters.SetProperty(x => x.FgEnabled, 0m),
                cancellationToken);
        }

        public async Task AddRangeAsync(IEnumerable<SiifObligacionApo> rows, CancellationToken cancellationToken)
        {
            // Encola inserts sobre el DbSet mapeado a DYNTBLSIIFOBLIGAPO.
            await _context.SiifObligacionesApo.AddRangeAsync(rows, cancellationToken);
        }

        // Ejecuta INSERT en SQL Server para las entidades pendientes del contexto.
        public Task SaveChangesAsync(CancellationToken cancellationToken) =>
            _context.SaveChangesAsync(cancellationToken);
    }
}
