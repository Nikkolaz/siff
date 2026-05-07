using System.Globalization;
using SSF.Interop.SIIFNacion.Application.Exceptions;

namespace SSF.Interop.SIIFNacion.Application.Common.SIIF
{
    public static class SiifVigenciaPeriodo
    {
        public static string ResolveAnioVigenciaFromDateRange(DateTime fechaInicio, DateTime fechaFin)
        {
            // Comparación por componente Year: no se permite cruzar 31-dic con 1-ene del año siguiente.
            if (fechaInicio.Year != fechaFin.Year)
            {
                throw new BadRequestException(
                    "El rango de fechas debe pertenecer a un único año calendario. No se permiten vigencias mezcladas.");
            }

            // Cultura invariante evita separadores de miles o nombres de mes localizados en la cadena persistida.
            return fechaInicio.Year.ToString(CultureInfo.InvariantCulture);
        }

        public static string ResolveAnioVigenciaFromDateStrings(
            string? fechaInicio,
            string? fechaFin,
            string nombreParamInicio,
            string nombreParamFin)
        {
            // Sin ambas fechas no se puede derivar ANIOVIGENCIA de forma determinística.
            if (string.IsNullOrWhiteSpace(fechaInicio) || string.IsNullOrWhiteSpace(fechaFin))
            {
                throw new BadRequestException(
                    $"{nombreParamInicio} y {nombreParamFin} son obligatorios para determinar la vigencia.");
            }

            if (!TryParseFlexible(fechaInicio.Trim(), out var d1) ||
                !TryParseFlexible(fechaFin.Trim(), out var d2))
            {
                throw new BadRequestException(
                    $"{nombreParamInicio} y {nombreParamFin} deben ser fechas válidas.");
            }

            // Misma regla de negocio que con DateTime ya tipados.
            if (d1.Year != d2.Year)
            {
                throw new BadRequestException(
                    "El rango de fechas debe pertenecer a un único año calendario. No se permiten vigencias mezcladas.");
            }

            return d1.Year.ToString(CultureInfo.InvariantCulture);
        }

        private static bool TryParseFlexible(string s, out DateTime dt)
        {
            if (DateTime.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
                return true;
            if (DateTime.TryParse(s, CultureInfo.CurrentCulture, DateTimeStyles.None, out dt))
                return true;
            return DateTime.TryParse(s, out dt);
        }
    }
}
