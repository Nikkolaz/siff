using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SSF.Interop.SIIFNacion.Domain.Cdp;

namespace SSF.Interop.SIIFNacion.Application.Contracts.Persistence
{
    public interface ICdpRepository
    {
        Task SaveCdpAsync(CdpExpedido cabecera, IEnumerable<CdpItem> detalles, CancellationToken cancellationToken);
    }
}

