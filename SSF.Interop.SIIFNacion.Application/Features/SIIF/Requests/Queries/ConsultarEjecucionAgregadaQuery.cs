using MediatR;
using SSF.Interop.SIIFNacion.Application.DTOs.External.SIIF.Response.Contracts;

namespace SSF.Interop.SIIFNacion.Application.Features.SIIF.Requests.Queries
{
    public sealed class ConsultarEjecucionAgregadaQuery : IRequest<ConsultarEjecucionAgregadaResponseDto>
    {
        // Headers requeridos por SIIF
        public string? CodPciHeader { get; set; } = default!;
        public string? LoginUsuarioSiifHeader { get; set; } = default!;
        public string? ConsecutivoHeader { get; set; } = default!;
        public string? HashHeader { get; set; }

        // Body requerido por SIIF
        public string? IdentificacionPCI { get; set; } = default!;
        public string? NivelInstitucional { get; set; } = default!;
        public string? ValorInstitucional { get; set; } = default!;
        public int AnioFiscal { get; set; }
        public int Mes { get; set; }
        public int TipoReporte { get; set; }
        public int Vigencia { get; set; }
        public int NivelNormativo { get; set; }
        public string? PosicionGastos { get; set; } = default!;
        public string? Usuario { get; set; } = default!;
    }
}
