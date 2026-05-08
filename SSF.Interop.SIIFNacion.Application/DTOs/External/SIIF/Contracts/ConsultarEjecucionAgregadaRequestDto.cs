using System.Text.Json.Serialization;

namespace SSF.Interop.SIIFNacion.Application.DTOs.External.SIIF.Contracts
{
    public sealed class ConsultarEjecucionAgregadaRequestDto
    {
        [JsonPropertyName("IdentificacionPCI")]
        public string? IdentificacionPCI { get; set; } = default!;

        [JsonPropertyName("NivelInstitucional")]
        public string? NivelInstitucional { get; set; } = default!;

        [JsonPropertyName("ValorInstitucional")]
        public string? ValorInstitucional { get; set; } = default!;

        [JsonPropertyName("AnioFiscal")]
        public int AnioFiscal { get; set; }

        [JsonPropertyName("Mes")]
        public int Mes { get; set; }

        [JsonPropertyName("TipoReporte")]
        public int TipoReporte { get; set; }

        [JsonPropertyName("Vigencia")]
        public int Vigencia { get; set; }

        [JsonPropertyName("NivelNormativo")]
        public int NivelNormativo { get; set; }

        [JsonPropertyName("PosicionGastos")]
        public string? PosicionGastos { get; set; } = default!;

        [JsonPropertyName("Usuario")]
        public string? Usuario { get; set; } = default!;
    }
}
