namespace SSF.Interop.SIIFNacion.API.Models.SIIF
{
    public sealed class ConsultarEjecucionAgregadaApiRequest
    {
        public string? CodPciHeader { get; set; } = default!;
        public string? LoginUsuarioSiifHeader { get; set; } = default!;
        public string? ConsecutivoHeader { get; set; } = default!;
        public string? HashHeader { get; set; }

        public string? IdentificacionPCI { get; set; } = default!;
        public string? NivelInstitucional { get; set; } = default!;
        public string? ValorInstitucional { get; set; } = default!;
        public int AnioFiscal { get; set; }
        public int Mes { get; set; }
        public int TipoReporte { get; set; }
        public int Vigencia { get; set; }
        public int NivelNormativo { get; set; }
        public string? PosicionGastos { get; set; } = default!;
        public string? Usuario { get; set; } = default!;
    }
}
