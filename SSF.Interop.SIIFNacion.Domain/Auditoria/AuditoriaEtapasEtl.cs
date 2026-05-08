using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SSF.Interop.SIIFNacion.Service.Domain.Auditoria
{
    [Table("AUDITORIA_ETAPAS_ETL", Schema = "INTEROP_SSF")]
    public class AuditoriaEtapasEtl
    {
        [Key]
        [Column("ID_ETAPA_ETL")]
        public Guid IdEtapaEtl { get; set; }

        [Required]
        [Column("ID_AUDITORIA_ETL")]
        public Guid IdAuditoriaEtl { get; set; }

        [Required]
        [Column("ID_TIPO_ETAPA")]
        public Guid IdTipoEtapa { get; set; }

        [Required]
        [Column("ID_ESTADO")]
        public Guid IdEstado { get; set; }

        [Required]
        [Column("FECHA_INICIO")]
        public DateTime FechaInicio { get; set; }

        [Column("FECHA_FIN")]
        public DateTime? FechaFin { get; set; }

        [Column("REGISTROS_PROCESADOS")]
        public long? RegistrosProcesados { get; set; }

        [Column("METADATOS_ETAPA")]
        [MaxLength(500)]
        public string? MetadatosEtapa { get; set; }
        [Column("ID_TRANSACCION")]
        public Guid IdTransaccion { get; set; }
        public RefTipoEtapa TipoEtapa { get; set; } = null!;
    }
}
