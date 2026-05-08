using System;

namespace SSF.Interop.SIIFNacion.API.Models.SIIF
{
    /// <summary>
    /// Contrato JSON de <c>POST api/siif/sincronizar-lista-obligaciones</c>; espejo de <see cref="Application.Features.SIIF.Requests.Commands.SincronizarListaObligaciones.SincronizarListaObligacionesCommand"/>.
    /// </summary>
    public sealed class SincronizarListaObligacionesApiRequest
    {
        /// <summary>Código PCI (cabecera SIIF).</summary>
        public string CodPciHeader { get; set; } = default!;

        /// <summary>Login usuario SIIF (cabecera).</summary>
        public string LoginUsuarioSiifHeader { get; set; } = default!;

        /// <summary>Consecutivo (cabecera).</summary>
        public string ConsecutivoHeader { get; set; } = default!;

        /// <summary>Hash opcional (cabecera).</summary>
        public string? HashHeader { get; set; }

        /// <summary>PCI del filtro <c>LstObliEnt</c>.</summary>
        public string CodPCI { get; set; } = default!;

        /// <summary>Inicio del rango (mismo año que <see cref="FechaFin"/>).</summary>
        public DateTime FechaInicio { get; set; }

        /// <summary>Fin del rango.</summary>
        public DateTime FechaFin { get; set; }

        /// <summary>Tipo de gasto.</summary>
        public string TipoGasto { get; set; } = default!;

        /// <summary>Rango.</summary>
        public string Rango { get; set; } = default!;

        /// <summary>Vigencia de listado SIIF (1, 2, 3…).</summary>
        public string Vigencia { get; set; } = default!;

        /// <summary>Detalle usos presupuestales.</summary>
        public string DetalleUsosPresupuestales { get; set; } = default!;
    }
}
