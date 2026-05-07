using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SSF.Interop.SIIFNacion.Service.Domain.Auditoria
{
    public class AuditoriaProcesosEtl
    {
        [Key]
        [Column("ID_AUDITORIA_ETL")]
        public Guid IdAuditoriaEtl { get; set; }
        [Column("ID_CONVENIO")]
        public Guid IdConvenio { get; set; }
        [Column("ID_ESTRUCTURA")]
        public Guid IdEstructura { get; set; }
        [Column("PERIODO")]
        public string Periodo { get; set; } = null!;
        [Column("ID_ESTADO")]
        public Guid IdEstado { get; set; }
        [Column("FECHA_INICIO")]
        public DateTime FechaInicio { get; set; }
        [Column("FECHA_FIN")]
        public DateTime? FechaFin { get; set; }
        [Column("WORKER_ID")]
        public string? WorkerId { get; set; }
    }

}
