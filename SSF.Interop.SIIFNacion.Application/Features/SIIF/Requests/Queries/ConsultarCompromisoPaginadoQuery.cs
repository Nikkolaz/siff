using MediatR;
using SSF.Interop.SIIFNacion.Application.DTOs.External.SIIF.Response.Contracts;
using System.Text.Json.Serialization;

namespace SSF.Interop.SIIFNacion.Application.Features.SIIF.Requests.Queries
{
    public sealed class ConsultarCompromisoPaginadoQuery : IRequest<ConsultaListaCompromisoPaginadaResponseDto>
    {
        // Headers SIIF — no vienen del body JSON, los inyecta el controller
        // desde los HTTP headers del request. JsonIgnore evita que aparezcan
        // en el body ni en Swagger.
        [JsonIgnore]
        public string? CodPciHeader { get; set; }
        [JsonIgnore]
        public string? LoginUsuarioSiifHeader { get; set; }
        [JsonIgnore]
        public string? ConsecutivoHeader { get; set; }
        [JsonIgnore]
        public string? HashHeader { get; set; }

        // Filtros de negocio — sí vienen del body
        public int Page { get; set; } = 0;
        public int Size { get; set; } = 0;
        public string? PCI { get; set; }
        public string? FechaInicio { get; set; }
        public string? FechaFin { get; set; }
        public string? TipoGasto { get; set; }
        public string? Rango { get; set; }
        public string? Vigencia { get; set; }
    }
}