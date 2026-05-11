namespace SSF.Interop.SIIFNacion.API.Models.SIIF
{
    /// <summary>
    /// Body del endpoint POST /api/siif/sincronizar-detalle-rp.
    /// </summary>
    public class SincronizarDetalleCompromisoRPApiRequest
    {
        public string? CodPciHeader { get; set; }
        public string? LoginUsuarioSiifHeader { get; set; }
        public string? ConsecutivoHeader { get; set; }
        public string? HashHeader { get; set; }

        /// <summary>"1" = Actual | "2" = Reserva Presupuestal</summary>
        public string Vigencia { get; set; } = default!;

        /// <summary>PCI de consulta (Ej: "36-01-07")</summary>
        public string Pci { get; set; } = "36-01-07";

        /// <summary>Fecha inicio (Ej: "2026-01-01")</summary>
        public string FechaInicio { get; set; } = "2026-01-01";

        /// <summary>Fecha fin (Ej: "2026-12-31")</summary>
        public string FechaFin { get; set; } = "2026-12-31";
    }
}
