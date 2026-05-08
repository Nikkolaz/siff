using SSF.Interop.SIIFNacion.Application.DTOs.External.SIIF.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSF.Interop.SIIFNacion.Application.Common.Interfaces.ExternalServices.SIIF
{
    public interface ISiifAuthenticationService
    {
        Task<SiifTokenResultDto> AuthenticateAsync(CancellationToken cancellationToken = default);
    }
}
