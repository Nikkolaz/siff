using SSF.Interop.SIIFNacion.Domain.Cdp;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public interface ICdpPaginadoRepository
{
    // Guardar un solo registro
    Task SaveAsync(CdpPaginado cdp, CancellationToken cancellationToken);

    // Guardar varios registros en un solo llamado
    Task SaveRangeAsync(IEnumerable<CdpPaginado> cdps, CancellationToken cancellationToken);

    // Verificar si ya existe un registro
    Task<bool> ExistsAsync(string codigoPci, int numeroSolicitud, CancellationToken cancellationToken);
}
