namespace SSF.Interop.SIIFNacion.Application.DTOs.External.SIIF.Contracts
{
    public sealed class ConsultaListaCdpPaginadaRequestDto
    {
        public PaginationDto PaginationDto { get; set; } = new();
        public ConsultaListaCdpFiltroDto ConsultaListaCDP { get; set; } = new();
    }
}
