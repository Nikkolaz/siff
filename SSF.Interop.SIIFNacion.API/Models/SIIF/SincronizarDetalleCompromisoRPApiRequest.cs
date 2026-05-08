namespace SSF.Interop.SIIFNacion.API.Models.SIIF
{
    public class SincronizarDetalleCompromisoRPApiRequest
    {
        public string? CodPciHeader { get; set; }
        public string? LoginUsuarioSiifHeader { get; set; }
        public string? ConsecutivoHeader { get; set; }
        public string? HashHeader { get; set; }

        public string Pci { get; set; } = default!;
        public string Vigencia { get; set; } = default!;
    }
}
