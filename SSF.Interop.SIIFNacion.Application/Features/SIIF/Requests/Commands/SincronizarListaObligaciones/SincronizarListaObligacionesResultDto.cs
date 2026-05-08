namespace SSF.Interop.SIIFNacion.Application.Features.SIIF.Requests.Commands.SincronizarListaObligaciones
{
    /// <summary>
    /// DTO de salida del comando de sincronización de obligaciones (resumen para API o worker).
    /// </summary>
    public sealed class SincronizarListaObligacionesResultDto
    {
        /// <summary>Total que reportó SIIF en <c>Data.Total</c>; si no vino, el handler puede sustituir por insertados.</summary>
        public int TotalReportadoPorSiif { get; set; }

        /// <summary>Cantidad de filas insertadas en <c>DYNTBLSIIFOBLIGAPO</c> en esta ejecución.</summary>
        public int RegistrosInsertados { get; set; }

        /// <summary>Número de páginas consumidas del servicio paginado (cada una hasta 50 ítems).</summary>
        public int PaginasProcesadas { get; set; }
    }
}
