using MediatR;
using SSF.Interop.SIIFNacion.Application.DTOs.External.SIIF.Response;

namespace SSF.Interop.SIIFNacion.Application.Features.SIIF.Requests.Commands.SincronizarDetalleCompromisoRP
{
    /// <summary>
    /// Comando que orquesta el flujo completo de sincronización de RPs:
    /// Fase 1 (Wipe + Lista paginada → DYNTBLCOMPROMPAGIN) y
    /// Fase 2 (Iteración + Detalle → DYNTBLCCOMPPTAL).
    /// Único parámetro de negocio: Vigencia ("1" = Actual, "2" = Reserva Presupuestal).
    /// </summary>
    public class SincronizarDetalleCompromisoRPCommand : IRequest<SincronizarDetalleCompromisoRPResponseDto>
    {
        // Headers de autenticación SIIF
        public string? CodPciHeader { get; set; }
        public string? LoginUsuarioSiifHeader { get; set; }
        public string? ConsecutivoHeader { get; set; }
        public string? HashHeader { get; set; }

        /// <summary>
        /// Código de vigencia: "1" = Actual, "2" = Reserva Presupuestal.
        /// Es el único parámetro de negocio del servicio.
        /// </summary>
        public string Vigencia { get; set; } = default!;
    }
}
