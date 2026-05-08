using Microsoft.Extensions.Options;
using SSF.Interop.SIIFNacion.Application.Common.Interfaces.ExternalServices.SIIF;
using SSF.Interop.SIIFNacion.Application.DTOs.External.SIIF.Auth;
using SSF.Interop.SIIFNacion.Infrastructure.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SSF.Interop.SIIFNacion.Infrastructure.Services.External.SIIF
{
    public sealed class SiifAuthenticationService : ISiifAuthenticationService
    {
        private readonly HttpClient _httpClient;
        private readonly SiifOptions _options;

        public SiifAuthenticationService(HttpClient httpClient, IOptions<SiifOptions> options)
        {
            _httpClient = httpClient;
            _options = options.Value;
        }

        public async Task<SiifTokenResultDto> AuthenticateAsync(CancellationToken cancellationToken = default)
        {
            var uri = $"{_options.AuthenticationPath}?user={Uri.EscapeDataString(_options.User)}&password={Uri.EscapeDataString(_options.Password)}";

            using var request = new HttpRequestMessage(HttpMethod.Post, uri);

            request.Headers.Accept.Clear();
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Content = new StringContent(string.Empty, System.Text.Encoding.UTF8, "application/json");

            using var response = await _httpClient.SendAsync(request, cancellationToken);

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync(cancellationToken);

            return ParseTokenResponse(content);
        }

        private static string? TryExtractToken(JsonElement root)
        {
            if (root.ValueKind == JsonValueKind.String)
                return root.GetString();

            if (root.TryGetProperty("token", out var tokenElement))
                return tokenElement.GetString();

            if (root.TryGetProperty("access_token", out var accessTokenElement))
                return accessTokenElement.GetString();

            if (root.TryGetProperty("jwt", out var jwtElement))
                return jwtElement.GetString();

            return null;
        }

        private SiifTokenResultDto ParseTokenResponse(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                throw new InvalidOperationException("La respuesta del servicio de autenticación SIIF llegó vacía.");

            var trimmed = content.TrimStart();

            return trimmed.StartsWith("<")
                ? ParseXmlTokenResponse(content)
                : ParseJsonTokenResponse(content);
        }

        private SiifTokenResultDto ParseJsonTokenResponse(string content)
        {
            using var document = JsonDocument.Parse(content);
            var root = document.RootElement;

            var accessToken = root.TryGetProperty("accessToken", out var accessTokenElement)
                ? accessTokenElement.GetString()
                : null;

            var idToken = root.TryGetProperty("idToken", out var idTokenElement)
                ? idTokenElement.GetString()
                : null;

            var tokenType = root.TryGetProperty("tokenType", out var tokenTypeElement)
                ? tokenTypeElement.GetString()
                : "Bearer";

            var expireIn = root.TryGetProperty("expireIn", out var expireInElement)
                ? expireInElement.GetInt32()
                : _options.TokenExpirationSeconds;

            if (string.IsNullOrWhiteSpace(idToken))
                throw new InvalidOperationException("No fue posible obtener idToken desde la respuesta JSON de SIIF.");

            return new SiifTokenResultDto
            {
                AccessToken = accessToken ?? string.Empty,
                IdToken = idToken,
                TokenType = tokenType ?? "Bearer",
                ExpiresInSeconds = expireIn,
                ExpiresAtUtc = DateTimeOffset.UtcNow.AddSeconds(expireIn)
            };
        }

        private SiifTokenResultDto ParseXmlTokenResponse(string content)
        {
            var xml = XDocument.Parse(content);
            var root = xml.Root ?? throw new InvalidOperationException("La respuesta XML de SIIF no tiene nodo raíz.");

            XNamespace ns = root.Name.Namespace;

            var accessToken = root.Element(ns + "accessToken")?.Value;
            var idToken = root.Element(ns + "idToken")?.Value;
            var tokenType = root.Element(ns + "tokenType")?.Value;
            var expireInText = root.Element(ns + "expireIn")?.Value;

            var expireIn = int.TryParse(expireInText, out var seconds)
                ? seconds
                : _options.TokenExpirationSeconds;

            if (string.IsNullOrWhiteSpace(idToken))
                throw new InvalidOperationException("No fue posible obtener idToken desde la respuesta XML de SIIF.");

            return new SiifTokenResultDto
            {
                AccessToken = accessToken ?? string.Empty,
                IdToken = idToken,
                TokenType = string.IsNullOrWhiteSpace(tokenType) ? "Bearer" : tokenType,
                ExpiresInSeconds = expireIn,
                ExpiresAtUtc = DateTimeOffset.UtcNow.AddSeconds(expireIn)
            };
        }
    }
}
