using System;

namespace SSF.Interop.SIIFNacion.Application.DTOs.External.SIIF.Response
{
    public class CoordinarSincronizacionRpResponseDto
    {
        public bool Exito { get; set; }
        public string Mensaje { get; set; } = string.Empty;
        public int RegistrosProcesadosTotales { get; set; }
        public int RegistrosGuardados { get; set; }
    }
}
