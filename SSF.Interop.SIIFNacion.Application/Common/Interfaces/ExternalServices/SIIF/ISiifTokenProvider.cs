using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSF.Interop.SIIFNacion.Application.Common.Interfaces.ExternalServices.SIIF
{
    public interface ISiifTokenProvider
    {
        Task<string> GetValidTokenAsync(CancellationToken cancellationToken = default);
        Task InvalidateTokenAsync();
    }
}
