namespace SSF.Interop.SIIFNacion.API.Models.SIIF
{
    public class EjecutarProcesoActualizacionApiRequest
    {
        public string TipoProceso { get; set; } = string.Empty;
        public object? Parametros { get; set; }
        public string UsuarioSolicitante { get; set; } = string.Empty;
        public string? IdProceso { get; set; }
        public string? IdTransaccionOrigen { get; set; }
        
        // Headers comunes
        public string? CodPciHeader { get; set; }
        public string? LoginUsuarioSiifHeader { get; set; }
        public string? ConsecutivoHeader { get; set; }
        public string? HashHeader { get; set; }
    }
}
