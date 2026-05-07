using System.Text.Json.Serialization;

namespace SSF.Interop.SIIFNacion.Application.DTOs.External.SIIF.Contracts
{
    public sealed class ConsultarCompromisoRequestDto
    {
        [JsonPropertyName("Pci")]
        public string? Pci { get; set; } = default!;

        [JsonPropertyName("CodCompromisoPptalGastos")]
        public int CodCompromisoPptalGastos { get; set; }

        [JsonPropertyName("Vigencia")]
        public string? Vigencia { get; set; } = default!;
    }
}
