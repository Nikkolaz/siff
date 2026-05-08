namespace SSF.Interop.SIIFNacion.Application.DTOs.External.SIIF.Response
{
    public class SincronizarDetalleCompromisoRPResponseDto
    {
        public string Estado { get; set; } = "OK";
        public int CantidadRegistrosActualizados { get; set; }
        public string Mensaje { get; set; } = string.Empty;
    }
}
