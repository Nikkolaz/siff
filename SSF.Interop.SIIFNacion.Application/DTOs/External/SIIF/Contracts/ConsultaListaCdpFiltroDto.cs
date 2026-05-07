namespace SSF.Interop.SIIFNacion.Application.DTOs.External.SIIF.Contracts
{
    public sealed class ConsultaListaCdpFiltroDto
    {
        public string PCIConsulta { get; set; } = default!;
        public string PCISubUnidades { get; set; } = default!;
        public string FechaRegistroIni { get; set; } = default!;
        public string FechaRegistroFin { get; set; } = default!;
        public string TipoGasto { get; set; } = default!;
        public string Rango { get; set; } = default!;
    }
}
