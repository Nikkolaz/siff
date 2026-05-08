namespace SSF.Interop.SIIFNacion.API.Models.SIIF
{
    /// <summary>
    /// Body del endpoint POST /api/siif/sincronizar-detalle-rp.
    /// Único parámetro de negocio: Vigencia ("1" = Actual, "2" = Reserva Presupuestal).
    /// </summary>
    public class SincronizarDetalleCompromisoRPApiRequest
    {
        public string? CodPciHeader { get; set; }
        public string? LoginUsuarioSiifHeader { get; set; }
        public string? ConsecutivoHeader { get; set; }
        public string? HashHeader { get; set; }

        /// <summary>"1" = Actual | "2" = Reserva Presupuestal</summary>
        public string Vigencia { get; set; } = default!;
    }
}
