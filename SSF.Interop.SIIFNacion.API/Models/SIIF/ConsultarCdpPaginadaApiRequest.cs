namespace SSF.Interop.SIIFNacion.API.Models.SIIF
{
    public sealed class ConsultarCdpPaginadaApiRequest
    {
        public string? CodPciHeader { get; set; } = default!;
        public string? LoginUsuarioSiifHeader { get; set; } = default!;
        public string? ConsecutivoHeader { get; set; } = default!;
        public string? HashHeader { get; set; }

        public int Page { get; set; } = 0;
        public int Size { get; set; } = 0;

        public string? PCIConsulta { get; set; }
        public string? PCISubUnidades { get; set; }
        public string? FechaRegistroIni { get; set; }
        public string? FechaRegistroFin { get; set; }
        public string? TipoGasto { get; set; }
        public string? Rango { get; set; }

    }
}