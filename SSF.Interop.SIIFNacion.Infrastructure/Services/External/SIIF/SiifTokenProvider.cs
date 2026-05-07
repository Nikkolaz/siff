using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using SSF.Interop.SIIFNacion.Application.Common.Interfaces.ExternalServices.SIIF;
using SSF.Interop.SIIFNacion.Application.DTOs.External.SIIF.Auth;
using SSF.Interop.SIIFNacion.Infrastructure.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSF.Interop.SIIFNacion.Infrastructure.Services.External.SIIF
{
    public sealed class SiifTokenProvider : ISiifTokenProvider
    {
        private const string CacheKey = "SIIF_ACCESS_TOKEN";
        private static readonly SemaphoreSlim Semaphore = new(1, 1);

        private readonly IMemoryCache _memoryCache;
        private readonly ISiifAuthenticationService _authenticationService;
        private readonly SiifOptions _options;

        public SiifTokenProvider(
            IMemoryCache memoryCache,
            ISiifAuthenticationService authenticationService,
            IOptions<SiifOptions> options)
        {
            _memoryCache = memoryCache;
            _authenticationService = authenticationService;
            _options = options.Value;
        }

        public async Task<string> GetValidTokenAsync(CancellationToken cancellationToken = default)
        {
            if (_memoryCache.TryGetValue<SiifTokenResultDto>(CacheKey, out var cachedToken) &&
                cachedToken is not null &&
                cachedToken.ExpiresAtUtc > DateTimeOffset.UtcNow.AddSeconds(_options.TokenRenewalSafetyWindowSeconds))
            {
                return cachedToken.BearerToken ?? string.Empty;
            }

            await Semaphore.WaitAsync(cancellationToken);
            try
            {
                if (_memoryCache.TryGetValue<SiifTokenResultDto>(CacheKey, out cachedToken) &&
                    cachedToken is not null &&
                    cachedToken.ExpiresAtUtc > DateTimeOffset.UtcNow.AddSeconds(_options.TokenRenewalSafetyWindowSeconds))
                {
                    return cachedToken.BearerToken ?? string.Empty;
                }

                var newToken = await _authenticationService.AuthenticateAsync(cancellationToken);

                _memoryCache.Set(CacheKey, newToken, newToken.ExpiresAtUtc);

                return newToken.BearerToken ?? string.Empty;
            }
            finally
            {
                Semaphore.Release();
            }
        }

        public Task InvalidateTokenAsync()
        {
            _memoryCache.Remove(CacheKey);
            return Task.CompletedTask;
        }
    }
}
