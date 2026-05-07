using SSF.Interop.SIIFNacion.Domain.Siif;

namespace SSF.Interop.SIIFNacion.Application.Contracts.Persistence
{
    /// <summary>
    /// Contrato de persistencia para la tabla DYNTBLEJECUCIONAGR.
    /// Sigue el mismo patrón que <see cref="ICdpCompromisoRepository"/>.
    /// </summary>
    public interface IEjecucionAgregadaRepository
    {
        /// <summary>Persiste una lista de registros de ejecución agregada.</summary>
        Task SaveRangeAsync(IEnumerable<SiifEjecucionAgregada> registros, CancellationToken cancellationToken);

        /// <summary>Verifica si ya existe un registro para el año fiscal y posición de gasto indicados.</summary>
        Task<bool> ExistsAsync(decimal anioFiscal, string posicionGasto, CancellationToken cancellationToken);
    }
}
