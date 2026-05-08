using MediatR;
using SSF.Interop.SIIFNacion.Application.DTOs.External.SIIF.Response.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSF.Interop.SIIFNacion.Application.Features.SIIF.Requests.Queries
{
    public sealed class ConsultarCdpQuery : IRequest<ConsultarCdpResponseDto>
    {
        // Headers requeridos por SIIF
        public string? CodPciHeader { get; set; } = default!;
        public string? LoginUsuarioSiifHeader { get; set; } = default!;
        public string? ConsecutivoHeader { get; set; } = default!;
        public string? HashHeader { get; set; }

        // Body requerido por SIIF
        public string? IdentificacionPCI { get; set; } = default!;
        public string? ConsecutivoCDP { get; set; } = default!;
    }
}
