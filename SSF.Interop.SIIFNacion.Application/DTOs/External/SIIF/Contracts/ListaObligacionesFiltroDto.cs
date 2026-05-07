using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SSF.Interop.SIIFNacion.Application.DTOs.External.SIIF.Contracts
{
    public sealed class ListaObligacionesFiltroDto
    {
        [JsonPropertyName("CodPCI")]
        public string? CodPCI { get; set; } = default!;

        [JsonPropertyName("FechaInicio")]
        public string? FechaInicio { get; set; }

        [JsonPropertyName("FechaFin")]
        public string? FechaFin { get; set; }

        [JsonPropertyName("TipoGasto")]
        public string? TipoGasto { get; set; } = default!;

        [JsonPropertyName("Rango")]
        public string? Rango { get; set; } = default!;

        [JsonPropertyName("Vigencia")]
        public string? Vigencia { get; set; } = default!;

        [JsonPropertyName("DetalleUsosPresupuestales")]
        public string? DetalleUsosPresupuestales { get; set; } = default!;
    }

    public sealed class ConsultaListaObligacionesPaginadaRequestDto
    {
        [JsonPropertyName("Pagination")]
        public PaginationDto Pagination { get; set; } = new();

        [JsonPropertyName("lstObliEnt")]
        public ListaObligacionesFiltroDto LstObliEnt { get; set; } = new();
    }
}
