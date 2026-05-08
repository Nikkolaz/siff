namespace SSF.Interop.SIIFNacion.API.Models.SIIF
{
    public sealed class ConsultarCompromisoPresupuestalRPApiRequest
    {
        public string? CodPciHeader { get; set; } = default!;
        public string? LoginUsuarioSiifHeader { get; set; } = default!;
        public string? ConsecutivoHeader { get; set; } = default!;
        public string? HashHeader { get; set; }

        public string? Pci { get; set; } = default!;
        public int CodCompromisoPptalGastos { get; set; } = 0;

        public string? Vigencia { get; set; } = default!;
        

    }
}