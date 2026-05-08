namespace SSF.Interop.SIIFNacion.Domain.Siif
{
    /// <summary>
    /// Entidad de dominio para la tabla DYNTBLEJECUCIONAGR.
    /// Convención de nombres: PascalCase en C#, columna en MAYÚSCULAS (mapeada en el DBContext).
    /// </summary>
    public class SiifEjecucionAgregada
    {
        // 🔑 Clave primaria varchar(32) — se genera como Guid.N antes del insert
        public string Oid { get; set; } = default!;

        // Campos de control de versión/sistema (patrón Gestordoc)
        public decimal NrVersion { get; set; } = 1;
        public long BnCreated { get; set; }
        public decimal FgEnabled { get; set; } = 1;
        public string? OidRevisionForm { get; set; }
        public decimal FgSystem { get; set; } = 0;
        public long BnUpdated { get; set; } = 0;

        // 📅 Año fiscal y posición de gasto
        public decimal? AnioFiscal { get; set; }
        public string? PosicionGasto { get; set; }

        // 💰 Valores de apropiación
        public decimal? ApropiacionInicial { get; set; }
        public decimal? ApropiacionAdicionada { get; set; }
        public decimal? ApropiacionReducida { get; set; }
        public decimal? ApropiacionVigente { get; set; }
        public decimal? ApropiacionBloqueada { get; set; }
        public decimal? ApropiacionDisponible { get; set; }

        // 📊 Valores de ejecución
        public decimal? VlCdp { get; set; }
        public decimal? VlCompromiso { get; set; }
        public decimal? VlObligacion { get; set; }
        public decimal? VlOrdenPago { get; set; }
        public decimal? VlPago { get; set; }

        // ⏱️ Fecha de carga del registro
        public DateTime? FechaCarga { get; set; }
    }
}
