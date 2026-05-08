using SSF.Interop.SIIFNacion.Domain.Cdp;

namespace SSF.Interop.SIIFNacion.Application.Contracts.Persistence
{
    /// <summary>
    /// Contrato de persistencia para la tabla GESTORDOC.dbo.DYNTBLCOMPROMPAGIN.
    /// Almacena los registros de la lista paginada de compromisos RP de SIIF.
    /// </summary>
    public interface IDynCompromPaginRepository
    {
        Task SaveRangeAsync(IEnumerable<DynTblCompromPagin> registros, CancellationToken cancellationToken);
        Task<bool> ExistsAsync(decimal codCompromiso, string anioVigencia, CancellationToken cancellationToken);
        Task DeleteByAnioVigenciaAsync(string anioVigencia, CancellationToken cancellationToken);
        Task UpsertCompromisosAsync(IEnumerable<DynTblCompromPagin> registros, CancellationToken cancellationToken);
    }
}