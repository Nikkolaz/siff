using MediatR;
using SSF.Interop.SIIFNacion.Application.DTOs.External.SIIF.Response;

namespace SSF.Interop.SIIFNacion.Application.Features.SIIF.Requests.Commands.SincronizarDetalleCompromisoRP
{
    public class SincronizarDetalleCompromisoRPCommand : IRequest<SincronizarDetalleCompromisoRPResponseDto>
    {
        public string? CodPciHeader { get; set; }
        public string? LoginUsuarioSiifHeader { get; set; }
        public string? ConsecutivoHeader { get; set; }
        public string? HashHeader { get; set; }

        public string Pci { get; set; } = default!;
        public string Vigencia { get; set; } = default!;
    }
}
