using SSF.Interop.SIIFNacion.Domain.Cdp;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public interface ICdpCompromisoRepository
{
    Task SaveAsync(CdpCompromiso comp, CancellationToken cancellationToken);

    /// <summary>
    /// BUG FIX: el parámetro era ValorInicial (decimal), lo cual causaba
    /// falsos positivos/negativos porque dos compromisos distintos pueden
    /// tener el mismo valor monetario. La clave correcta es IdCompromiso.
    /// </summary>
    Task<bool> ExistsAsync(long idCompromiso, CancellationToken cancellationToken);

    Task ResetIdentityIfEmpty();
}