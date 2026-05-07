using SSF.Interop.SIIFNacion.Application.DTOs.External.SIIF.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SSF.Interop.SIIFNacion.Infrastructure.Services.External.SIIF
{
    public abstract class SiifClientBase
    {
        protected readonly HttpClient HttpClient;
        protected readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
        {
            PropertyNameCaseInsensitive = true
        };

        protected SiifClientBase(HttpClient httpClient)
        {
            HttpClient = httpClient;
        }

        protected async Task<TResponse> PostAsync<TRequest, TResponse>(
            string endpoint,
            TRequest request,
            SiifRequestHeaderDto headers,
            CancellationToken cancellationToken)
        {
            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, endpoint);

            httpRequest.Headers.TryAddWithoutValidation("codPCI", headers.CodPci);
            httpRequest.Headers.TryAddWithoutValidation("loginUsuarioSIIF", headers.LoginUsuarioSiif);
            httpRequest.Headers.TryAddWithoutValidation("consecutivo", headers.Consecutivo);

            if (!string.IsNullOrWhiteSpace(headers.Hash))
                httpRequest.Headers.TryAddWithoutValidation("hash", headers.Hash);

            var json = JsonSerializer.Serialize(request, JsonOptions);
            httpRequest.Content = new StringContent(json, Encoding.UTF8, "application/json");

            using var response = await HttpClient.SendAsync(httpRequest, cancellationToken);

            // Leer el contenido ANTES de verificar el status, para poder incluirlo en el mensaje de error
            var content = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(
                    $"SIIF responded with {(int)response.StatusCode} {response.ReasonPhrase} " +
                    $"for endpoint '{endpoint}'. Body: {(content.Length > 500 ? content[..500] : content)}",
                    null,
                    response.StatusCode);
            }

            if (string.IsNullOrWhiteSpace(content))
                throw new InvalidOperationException($"La respuesta de '{endpoint}' llegó vacía.");

            var jsonSerializado = JsonSerializer.Deserialize<TResponse>(content, JsonOptions)
                 ?? throw new InvalidOperationException($"No fue posible deserializar la respuesta de '{endpoint}'.");

            return jsonSerializado;
        }
    }
}
