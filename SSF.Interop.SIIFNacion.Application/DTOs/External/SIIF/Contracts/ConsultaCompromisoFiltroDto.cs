using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SSF.Interop.SIIFNacion.Application.DTOs.External.SIIF.Contracts
{
    public sealed class ConsultaCompromisoFiltroDto
    {
        [JsonPropertyName("PCI")]
        public string? PCI { get; set; } = default!;

        [JsonPropertyName("FechaInicio")]
        public string? FechaInicio { get; set; } = default!;

        [JsonPropertyName("FechaFin")]
        public string? FechaFin { get; set; } = default!;

        [JsonPropertyName("TipoGasto")]
        public string? TipoGasto { get; set; } = default!;

        [JsonPropertyName("Rango")]
        public string? Rango { get; set; } = default!;

        [JsonPropertyName("Vigencia")]
        public string? Vigencia { get; set; } = default!;
    }

    public sealed class ConsultaListaCompromisoPaginadaRequestDto
    {
        [JsonPropertyName("paginationDto")]
        public PaginationDto PaginationDto { get; set; } = new();

        [JsonPropertyName("consultaCompromiso")]
        public ConsultaCompromisoFiltroDto ConsultaCompromiso { get; set; } = new();
    }
}
