using System.Text.Json.Serialization;

namespace SSF.Interop.SIIFNacion.API.Models.SIIF 
{
    public sealed class ListaCompromisoPresupuestalRPApiRequest
    {
        public PaginationDto PaginationDto { get; set; } = new();
        public ConsultaCompromisoDto ConsultaCompromiso { get; set; } = new();

        // No vienen del body — los inyecta el controller desde los HTTP headers
        [JsonIgnore]
        public string? CodPciHeader { get; set; }
        [JsonIgnore]
        public string? LoginUsuarioSiifHeader { get; set; }
        [JsonIgnore]
        public string? ConsecutivoHeader { get; set; }
        [JsonIgnore]
        public string? HashHeader { get; set; }
    }

    public class PaginationDto
    {
        public int Page { get; set; }
        public int Size { get; set; }
    }

    public class ConsultaCompromisoDto
    {
        public string? PCI { get; set; }
        public string? FechaInicio { get; set; }
        public string? FechaFin { get; set; }
        public string? TipoGasto { get; set; }
        public string? Rango { get; set; }
        public string? Vigencia { get; set; }
    }
}