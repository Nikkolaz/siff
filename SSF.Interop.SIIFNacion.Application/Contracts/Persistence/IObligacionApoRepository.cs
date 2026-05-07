using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SSF.Interop.SIIFNacion.Domain.Siif;

namespace SSF.Interop.SIIFNacion.Application.Contracts.Persistence
{
    // Abstracción de persistencia para filas de obligaciones (tabla DYNTBLSIIFOBLIGAPO).
    public interface IObligacionApoRepository
    {
        /// <summary>
        /// Marca FGENABLED = 0 en filas con el mismo ANIOVIGENCIA.
        /// Si <paramref name="vigenciaCod"/> tiene valor, solo se afectan filas cuyo VIGENCIACOD coincide (p. ej. vigencia "1" → VIGENCIACOD = "1").
        /// Si es null o vacío, se deshabilitan todas las filas de ese año (comportamiento anterior).
        /// </summary>
        Task DisableByAnioVigenciaAsync(string anioVigencia, string? vigenciaCod, CancellationToken cancellationToken);

        Task AddRangeAsync(IEnumerable<SiifObligacionApo> rows, CancellationToken cancellationToken);
        Task SaveChangesAsync(CancellationToken cancellationToken);
    }
}
