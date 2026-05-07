using System;

namespace SSF.Interop.SIIFNacion.Domain.Siif
{
    /// <summary>
    /// Obligación / posición presupuestal persistida en la tabla <c>DYNTBLSIIFOBLIGAPO</c> (esquema Gestordoc).
    /// </summary>
    public class SiifObligacionApo
    {
        /// <summary>Clave primaria (OID); se genera en el handler antes del insert.</summary>
        public string Oid { get; set; } = default!;

        /// <summary>Número de versión de fila Gestordoc (típicamente 1 en altas).</summary>
        public decimal NrVersion { get; set; } = 1;

        /// <summary>Marca de creación en milisegundos Unix (columna BNCREATED).</summary>
        public long BnCreated { get; set; }

        /// <summary>1 = activo para consumo; 0 = histórico deshabilitado por nueva carga de la misma vigencia.</summary>
        public decimal FgEnabled { get; set; } = 1;

        /// <summary>Identificador de posición u obligación según payload SIIF (mapeo legacy).</summary>
        public decimal? IdObligacionPosicio { get; set; }

        /// <summary>Identificador de obligación parseado desde texto SIIF.</summary>
        public decimal? IdObliga { get; set; }

        /// <summary>Código numérico de obligación.</summary>
        public decimal? CodObligacion { get; set; }

        /// <summary>Código de vigencia de listado enviado a SIIF; columna <c>VIGENCIACOD</c>.</summary>
        public string? VigenciaCod { get; set; }

        /// <summary>Año de la vigencia de carga derivado del rango de fechas; columna <c>ANIOVIGENCIA</c>.</summary>
        public string? AnioVigencia { get; set; }

        /// <summary>Código dependencia de afectación.</summary>
        public string? CodDepAfectacio { get; set; }

        /// <summary>Descripción dependencia de afectación.</summary>
        public string? DesDepAfectacio { get; set; }

        /// <summary>Código posición del gasto.</summary>
        public string? CodPosicionGast { get; set; }

        /// <summary>Descripción posición del gasto.</summary>
        public string? DesPosicionGast { get; set; }

        /// <summary>Código fuente de financiación.</summary>
        public string? CodFuenteFinan { get; set; }

        /// <summary>Descripción fuente de financiación.</summary>
        public string? DesFuenteFinan { get; set; }

        /// <summary>Código recurso presupuestal principal.</summary>
        public string? CodRecursoPpal { get; set; }

        /// <summary>Descripción recurso presupuestal principal.</summary>
        public string? DesRecursoPpal { get; set; }

        /// <summary>Código situación de fondos.</summary>
        public string? CodSituacionFon { get; set; }

        /// <summary>Descripción situación de fondos.</summary>
        public string? DesSituacionFon { get; set; }

        /// <summary>Valor inicial de la posición.</summary>
        public decimal? VlInicialPosicion { get; set; }

        /// <summary>Valor de operaciones.</summary>
        public decimal? VlOperaciones { get; set; }

        /// <summary>Valor actual de la posición.</summary>
        public decimal? VlActualPosicio { get; set; }

        /// <summary>Saldo por utilizar.</summary>
        public decimal? SaldoXUtilizar { get; set; }

        /// <summary>Fecha/hora en que esta fila se cargó desde la integración.</summary>
        public DateTime FechaCarga { get; set; }
    }
}
