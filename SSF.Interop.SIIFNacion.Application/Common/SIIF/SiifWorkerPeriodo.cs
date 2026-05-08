using System;
using System.Globalization;

namespace SSF.Interop.SIIFNacion.Application.Common.SIIF
{
    public static class SiifWorkerPeriodo
    {
        public static (DateTime FechaInicio, DateTime FechaFin) YearToDateLocal(DateTime todayLocal)
        {
            // Normalizar a medianoche del día indicado preservando Kind (Unspecified/Utc/Local) del argumento.
            var d = todayLocal.Date;

            // Primer instante del año civil de ese día.
            var inicio = new DateTime(d.Year, 1, 1, 0, 0, 0, d.Kind);

            // Fin del rango inclusivo: el mismo día “hoy” (contrato de negocio: hasta la fecha actual, no hasta fin de mes).
            return (inicio, d);
        }

        public static (string FechaInicioIso, string FechaFinIso) YearToDateIsoStrings(DateTime todayLocal)
        {
            // Reutiliza la lógica única de fechas para no divergir entre obligaciones (DateTime) y CDP paginado (string).
            var (ini, fin) = YearToDateLocal(todayLocal);

            // Formato ISO 8601 fecha-only; SIIF y el resto del código usan este patrón en filtros.
            var fmt = DateTimeFormatInfo.InvariantInfo;
            return (ini.ToString("yyyy-MM-dd", fmt), fin.ToString("yyyy-MM-dd", fmt));
        }
    }
}
