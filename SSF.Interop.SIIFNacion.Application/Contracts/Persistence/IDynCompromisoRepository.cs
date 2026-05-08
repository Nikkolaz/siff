using SSF.Interop.SIIFNacion.Domain.Cdp;

namespace SSF.Interop.SIIFNacion.Application.Contracts.Persistence
{
    /// <summary>
    /// Contrato de persistencia para las nuevas tablas de producción
    /// DYNTBLCCOMPPTAL y DYNTBLLISTITEMSAFE.
    /// </summary>
    public interface IDynCompromisoRepository
    {
        /// <summary>Persiste un compromiso con sus ítems en las nuevas tablas.</summary>
        Task SaveAsync(DynTblCCompPtal compromiso, CancellationToken cancellationToken);

        /// <summary>Verifica si ya existe un compromiso por su IdCompromiso.</summary>
        Task<bool> ExistsAsync(decimal idCompromiso, CancellationToken cancellationToken);

        /// <summary>Realiza un Insert o Update del detalle del compromiso (HU 6).</summary>
        Task UpsertDetailAsync(DynTblCCompPtal compromiso, CancellationToken cancellationToken);

        /// <summary>
        /// Elimina todos los registros de DYNTBLCCOMPPTAL y DYNTBLLISTITEMSAFE para
        /// la vigencia indicada. Se usa en la estrategia Wipe &amp; Load antes de repoblar.
        /// </summary>
        Task WipeByVigenciaAsync(string vigencia, CancellationToken cancellationToken);
    }
}
