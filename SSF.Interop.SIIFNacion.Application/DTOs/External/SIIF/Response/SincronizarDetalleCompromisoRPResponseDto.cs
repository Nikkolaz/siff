namespace SSF.Interop.SIIFNacion.Application.DTOs.External.SIIF.Response
{
    public class SincronizarDetalleCompromisoRPResponseDto
    {
        /// <summary>"OK" | "PARCIAL" | "ERROR"</summary>
        public string Estado { get; set; } = "OK";

        public int CantidadRegistrosActualizados { get; set; }

        public string Mensaje { get; set; } = string.Empty;
    }
}
