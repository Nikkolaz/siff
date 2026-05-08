using System.Text.Json;
using SSF.Interop.SIIFNacion.Application.Common.Interfaces.ExternalServices.SIIF;
using SSF.Interop.SIIFNacion.Application.DTOs.External.SIIF.Common;
using SSF.Interop.SIIFNacion.Application.DTOs.External.SIIF.Contracts;
using SSF.Interop.SIIFNacion.Application.DTOs.External.SIIF.Response.Contracts;

namespace SSF.Interop.SIIFNacion.Infrastructure.Services.External.SIIF
{
    public sealed class SiifBudgetService : SiifClientBase, ISiifBudgetService
    {
        public SiifBudgetService(HttpClient httpClient) : base(httpClient) { }

        public async Task<ConsultarCdpResponseDto> ConsultarCdpAsync(
            ConsultarCdpRequestDto request,
            SiifRequestHeaderDto headers,
            CancellationToken cancellationToken = default)
        {
            var raw = await PostAsync<ConsultarCdpRequestDto, JsonElement>(
                "/siif/api/epg/consultarcdp", request, headers, cancellationToken);

            var response = new ConsultarCdpResponseDto { Data = raw };

            if (raw.ValueKind == JsonValueKind.Object)
            {
                if (raw.TryGetProperty("Codigo", out var codigoEl))
                    response.Success = string.Equals(codigoEl.GetString(), "200", StringComparison.OrdinalIgnoreCase);

                if (raw.TryGetProperty("Mensaje", out var mensajeEl))
                    response.Message = mensajeEl.GetString();
            }

            return response;
        }

        public async Task<ConsultaListaCdpPaginadaResponseDto> ConsultarListaCdpPaginadaAsync(
            ConsultaListaCdpPaginadaRequestDto request,
            SiifRequestHeaderDto headers,
            CancellationToken cancellationToken = default)
        {
            var raw = await PostAsync<ConsultaListaCdpPaginadaRequestDto, JsonElement>(
                "/siif/api/epg/consultalistacdppaginada", request, headers, cancellationToken);

            var response = new ConsultaListaCdpPaginadaResponseDto { Data = raw };

            if (raw.ValueKind == JsonValueKind.Object)
            {
                if (raw.TryGetProperty("State", out var stateEl))
                    response.Success = stateEl.ValueKind == JsonValueKind.True;

                if (raw.TryGetProperty("Code", out var codeEl) && codeEl.ValueKind == JsonValueKind.Number)
                    response.Success &= codeEl.GetInt32() == 200;

                if (raw.TryGetProperty("Message", out var msgEl))
                    response.Message = msgEl.GetString();
            }

            return response;
        }

        /// <summary>
        /// BUG FIX: SIIF envuelve la respuesta de consultacompromisopptal en un objeto JSON raíz
        /// que puede tener distintas formas según la versión del WS:
        ///
        ///   Forma A (objeto directo):  { "Codigo": 3626, "FechaRegistro": "...", ... }
        ///   Forma B (wrapper):         { "State": true, "Code": 200, "Data": { "Codigo": 3626, ... } }
        ///   Forma C (wrapper Data/data):{ "data": { "Codigo": 3626, ... } }
        ///
        /// Deserializar directamente al DTO tipado funcionaba solo en Forma A.
        /// Ahora leemos como JsonElement y luego buscamos el objeto correcto antes de deserializar.
        /// </summary>
        public async Task<ConsultarCompromisoResponseDto> ConsultarCompromisoPptalAsync(
            ConsultarCompromisoRequestDto request,
            SiifRequestHeaderDto headers,
            CancellationToken cancellationToken = default)
        {
            var raw = await PostAsync<ConsultarCompromisoRequestDto, JsonElement>(
                "/siif/api/epg/consultacompromisopptal", request, headers, cancellationToken);

            // Extraer el objeto real del compromiso (maneja wrapper o respuesta directa)
            var compromisoElement = ExtractCompromisoObject(raw);

            var response = JsonSerializer.Deserialize<ConsultarCompromisoResponseDto>(
                compromisoElement.GetRawText(), JsonOptions)
                ?? new ConsultarCompromisoResponseDto();

            return response;
        }

        /// <summary>
        /// Busca el objeto JSON que contiene los campos del compromiso (Codigo, FechaRegistro, etc.).
        /// Soporta respuesta directa y wrappers con Data/data/consultaCompromisoPptalSal.
        /// </summary>
        private static JsonElement ExtractCompromisoObject(JsonElement root)
        {
            if (root.ValueKind != JsonValueKind.Object)
                return root;

            // Si el objeto raíz ya tiene "Codigo" como número o "FechaRegistro", es el compromiso directo
            if (root.TryGetProperty("Codigo", out var codigoProp) &&
                codigoProp.ValueKind == JsonValueKind.Number)
                return root;

            // Buscar en wrappers conocidos: Data, data, consultaCompromisoPptalSal, etc.
            foreach (var wrapperName in new[] { "Data", "data", "consultaCompromisoPptalSal", "compromiso", "Compromiso" })
            {
                if (root.TryGetProperty(wrapperName, out var nested) &&
                    nested.ValueKind == JsonValueKind.Object)
                {
                    // Verificar que el nested tenga campos de compromiso
                    if (nested.TryGetProperty("Codigo", out var c) && c.ValueKind == JsonValueKind.Number)
                        return nested;

                    // Un nivel más adentro por si acaso
                    foreach (var prop in nested.EnumerateObject())
                    {
                        if (prop.Value.ValueKind == JsonValueKind.Object &&
                            prop.Value.TryGetProperty("Codigo", out var cc) &&
                            cc.ValueKind == JsonValueKind.Number)
                            return prop.Value;
                    }
                }
            }

            // Buscar en cualquier propiedad objeto que tenga "Codigo" numérico
            foreach (var prop in root.EnumerateObject())
            {
                if (prop.Value.ValueKind == JsonValueKind.Object &&
                    prop.Value.TryGetProperty("Codigo", out var c) &&
                    c.ValueKind == JsonValueKind.Number)
                    return prop.Value;
            }

            // Fallback: devolver el root y que el deserializador intente lo que pueda
            return root;
        }

       /* public async Task<ConsultaListaCompromisoPaginadaResponseDto> ConsultarListaCompromisoPaginadaAsync(
            ConsultaListaCompromisoPaginadaRequestDto request,
            SiifRequestHeaderDto headers,
            CancellationToken cancellationToken = default)
        {
            var raw = await PostAsync<ConsultaListaCompromisoPaginadaRequestDto, JsonElement>(
                "/siif/api/epg/consultalistacompromisopaginada", request, headers, cancellationToken);

           /* var response = new ConsultaListaCompromisoPaginadaResponseDto { Data = raw };

            if (raw.ValueKind == JsonValueKind.Object)
            {
                if (raw.TryGetProperty("State", out var stateEl))
                    response.Success = stateEl.ValueKind == JsonValueKind.True;

                if (raw.TryGetProperty("Code", out var codeEl) && codeEl.ValueKind == JsonValueKind.Number)
                    response.Success &= codeEl.GetInt32() == 200;

                if (raw.TryGetProperty("Message", out var msgEl))
                    response.Message = msgEl.GetString();
            }

            return response;
        } */

      public Task<ConsultaListaCompromisoPaginadaResponseDto> ConsultarListaCompromisoPaginadaAsync(
            ConsultaListaCompromisoPaginadaRequestDto request,
            SiifRequestHeaderDto headers,
            CancellationToken cancellationToken = default)
            => PostAsync<ConsultaListaCompromisoPaginadaRequestDto, ConsultaListaCompromisoPaginadaResponseDto>(
                "/siif/api/epg/consultalistacompromisopaginada", request, headers, cancellationToken);

        public async Task<ConsultaListaObligacionesPaginadaResponseDto> ConsultarListaObligacionesPaginadaAsync(
            ConsultaListaObligacionesPaginadaRequestDto request,
            SiifRequestHeaderDto headers,
            CancellationToken cancellationToken = default)
        {
            var raw = await PostAsync<ConsultaListaObligacionesPaginadaRequestDto, JsonElement>(
                "/siif/api/epg/consultalistaobligacionespaginada", request, headers, cancellationToken);

            var response = new ConsultaListaObligacionesPaginadaResponseDto { Data = raw };

            if (raw.ValueKind == JsonValueKind.Object)
            {
                if (raw.TryGetProperty("State", out var stateEl))
                    response.Success = stateEl.ValueKind == JsonValueKind.True;

                if (raw.TryGetProperty("Code", out var codeEl) && codeEl.ValueKind == JsonValueKind.Number)
                    response.Success &= codeEl.GetInt32() == 200;

                if (raw.TryGetProperty("Message", out var msgEl))
                    response.Message = msgEl.GetString();
            }

            return response;
        }

        public async Task<ConsultarEjecucionAgregadaResponseDto> ConsultarEjecucionAgregadaAsync(
            ConsultarEjecucionAgregadaRequestDto request,
            SiifRequestHeaderDto headers,
            CancellationToken cancellationToken = default)
        {
            var raw = await PostAsync<ConsultarEjecucionAgregadaRequestDto, JsonElement>(
                "/siif/api/epg/consultarejecucionagregada", request, headers, cancellationToken);

            var response = new ConsultarEjecucionAgregadaResponseDto { Data = raw };

            if (raw.ValueKind == JsonValueKind.Object)
            {
                if (raw.TryGetProperty("success", out var successEl))
                    response.Success = successEl.ValueKind == JsonValueKind.True;

                if (raw.TryGetProperty("message", out var msgEl))
                    response.Message = msgEl.GetString();
            }

            return response;
        }
    }
}