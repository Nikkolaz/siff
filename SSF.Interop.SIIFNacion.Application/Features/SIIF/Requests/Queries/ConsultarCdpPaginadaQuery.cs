using MediatR;
using SSF.Interop.SIIFNacion.Application.DTOs.External.SIIF.Response.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSF.Interop.SIIFNacion.Application.Features.SIIF.Requests.Queries
{
    public sealed class ConsultarCdpPaginadaQuery : IRequest<ConsultaListaCdpPaginadaResponseDto>
    {
        // Headers requeridos por SIIF
        public string? CodPciHeader { get; set; } = default!;
        public string? LoginUsuarioSiifHeader { get; set; } = default!;
        public string? ConsecutivoHeader { get; set; } = default!;
        public string? HashHeader { get; set; }

        // Body requerido por SIIF

        public int Page { get; set; } = 0;
        public int Size { get; set; } = 0;

        public string? PCIConsulta { get; set; }
        public string? PCISubUnidades { get; set; }
        public string? FechaRegistroIni { get; set; }
        public string? FechaRegistroFin { get; set; }
        public string? TipoGasto { get; set; }
        public string? Rango { get; set; }
    }
}
