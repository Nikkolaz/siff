using MediatR;
using SSF.Interop.SIIFNacion.Application.DTOs.External.SIIF.Response.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSF.Interop.SIIFNacion.Application.Features.SIIF.Requests.Queries
{
    public sealed class ConsultarCompromisoPresupuestalRPQuery : IRequest<ConsultarCompromisoResponseDto >
    {
        // Headers requeridos por SIIF
        public string? CodPciHeader { get; set; } = default!;
        public string? LoginUsuarioSiifHeader { get; set; } = default!;
        public string? ConsecutivoHeader { get; set; } = default!;
        public string? HashHeader { get; set; }

        // Body requerido por SIIF
        public string? Pci { get; set; } = default!;
        public int CodCompromisoPptalGastos { get; set; } = 0;

        public string? Vigencia { get; set; } = default!;
                    
    }
}
