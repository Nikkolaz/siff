using MediatR;
using SSF.Interop.SIIFNacion.Application.DTOs.External.SIIF.Response;

namespace SSF.Interop.SIIFNacion.Application.Features.SIIF.Requests.Commands.CoordinadorSincronizacionRp
{
    public class CoordinarSincronizacionRpCommand : IRequest<CoordinarSincronizacionRpResponseDto>
    {
        public int Anio { get; set; }

        public string? CodPciHeader { get; set; }
        public string? LoginUsuarioSiifHeader { get; set; }
        public string? ConsecutivoHeader { get; set; }
        public string? HashHeader { get; set; }
    }
}
