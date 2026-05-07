namespace SSF.Interop.SIIFNacion.API.Models.SIIF
{
    public sealed class ConsultarCdpApiRequest
    {
        public string CodPciHeader { get; set; } = default!;
        public string LoginUsuarioSiifHeader { get; set; } = default!;
        public string ConsecutivoHeader { get; set; } = default!;
        public string? HashHeader { get; set; }

        public string IdentificacionPCI { get; set; } = default!;
        public string ConsecutivoCDP { get; set; } = default!;
    }
}
