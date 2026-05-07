using System;

namespace SSF.Interop.SIIFNacion.API.Models.SIIF
{
    public class CoordinarSincronizacionRpApiRequest
    {
        public int Anio { get; set; }

        // Headers comunes
        public string? CodPciHeader { get; set; }
        public string? LoginUsuarioSiifHeader { get; set; }
        public string? ConsecutivoHeader { get; set; }
        public string? HashHeader { get; set; }
    }
}
