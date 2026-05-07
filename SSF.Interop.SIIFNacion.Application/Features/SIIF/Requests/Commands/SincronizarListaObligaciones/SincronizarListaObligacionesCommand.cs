using System;
using MediatR;

namespace SSF.Interop.SIIFNacion.Application.Features.SIIF.Requests.Commands.SincronizarListaObligaciones
{
    /// <summary>
    /// Entrada del caso de uso “sincronizar lista de obligaciones”: headers SIIF + filtro <c>LstObliEnt</c> reutilizado en cada página.
    /// </summary>
    public sealed class SincronizarListaObligacionesCommand : IRequest<SincronizarListaObligacionesResultDto>
    {
        // --- Cabeceras HTTP requeridas por SIIF ---

        /// <summary>Código PCI en cabecera.</summary>
        public string CodPciHeader { get; set; } = default!;

        /// <summary>Usuario SIIF.</summary>
        public string LoginUsuarioSiifHeader { get; set; } = default!;

        /// <summary>Consecutivo de petición.</summary>
        public string ConsecutivoHeader { get; set; } = default!;

        /// <summary>Hash opcional.</summary>
        public string? HashHeader { get; set; }

        // --- Campos del filtro de obligaciones (mismo objeto en cada POST paginado) ---

        /// <summary>PCI del ente en el cuerpo del filtro.</summary>
        public string CodPCI { get; set; } = default!;

        /// <summary>Inicio del rango; debe ser el mismo año calendario que <see cref="FechaFin"/>.</summary>
        public DateTime FechaInicio { get; set; }

        /// <summary>Fin del rango inclusive; mismo año que <see cref="FechaInicio"/>.</summary>
        public DateTime FechaFin { get; set; }

        /// <summary>Tipo de gasto.</summary>
        public string TipoGasto { get; set; } = default!;

        /// <summary>Rango.</summary>
        public string Rango { get; set; } = default!;

        /// <summary>Código de vigencia de listado en SIIF (1, 2, 3, …); se persiste en <c>VIGENCIACOD</c>.</summary>
        public string Vigencia { get; set; } = default!;

        /// <summary>Detalle de usos presupuestales requerido por el contrato SIIF.</summary>
        public string DetalleUsosPresupuestales { get; set; } = default!;
    }
}
