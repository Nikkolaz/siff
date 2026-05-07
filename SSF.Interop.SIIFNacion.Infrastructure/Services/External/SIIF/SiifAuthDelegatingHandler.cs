using SSF.Interop.SIIFNacion.Application.Common.Interfaces.ExternalServices.SIIF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace SSF.Interop.SIIFNacion.Infrastructure.Services.External.SIIF
{
    public sealed class SiifAuthDelegatingHandler : DelegatingHandler
    {
        private readonly ISiifTokenProvider _tokenProvider;

        public SiifAuthDelegatingHandler(ISiifTokenProvider tokenProvider)
        {
            _tokenProvider = tokenProvider;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var token = await _tokenProvider.GetValidTokenAsync(cancellationToken);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
