using System.Text.Json.Serialization;

namespace SSF.Interop.SIIFNacion.Application.DTOs.External.SIIF.Contracts
{
    public sealed class ConsultarCdpRequestDto
    {
        [JsonPropertyName("IdentificacionPCI")]
        public string? IdentificacionPCI { get; set; } = default!;

        [JsonPropertyName("ConsecutivoCDP")]
        public string? ConsecutivoCDP { get; set; } = default!;
    }
}
