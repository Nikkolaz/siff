using SSF.Interop.SIIFNacion.Application.Common.Interfaces.ExternalServices.SIIF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SSF.Interop.SIIFNacion.Infrastructure.Services.External.Handlers
{
    public sealed class SiifRetryOnUnauthorizedHandler : DelegatingHandler
    {
        private readonly ISiifTokenProvider _tokenProvider;

        public SiifRetryOnUnauthorizedHandler(ISiifTokenProvider tokenProvider)
        {
            _tokenProvider = tokenProvider;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var response = await base.SendAsync(request, cancellationToken);

            if (response.StatusCode != HttpStatusCode.Unauthorized &&
                response.StatusCode != HttpStatusCode.Forbidden)
            {
                return response;
            }

            response.Dispose();

            await _tokenProvider.InvalidateTokenAsync();

            var clonedRequest = await CloneAsync(request, cancellationToken);
            var retryResponse = await base.SendAsync(clonedRequest, cancellationToken);

            return retryResponse;
        }

        private static async Task<HttpRequestMessage> CloneAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var clone = new HttpRequestMessage(request.Method, request.RequestUri);

            foreach (var header in request.Headers)
                clone.Headers.TryAddWithoutValidation(header.Key, header.Value);

            if (request.Content is not null)
            {
                var ms = new MemoryStream();
                await request.Content.CopyToAsync(ms, cancellationToken);
                ms.Position = 0;

                clone.Content = new StreamContent(ms);

                foreach (var header in request.Content.Headers)
                    clone.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            return clone;
        }
    }
}

