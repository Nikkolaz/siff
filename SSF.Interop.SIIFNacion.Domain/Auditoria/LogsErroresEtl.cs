using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SSF.Interop.SIIFNacion.Service.Domain.Auditoria
{
    public class LogsErroresEtl
    {
        [Key]
        [Column("ID_LOG_ETL")]
        public Guid IdLogEtl { get; set; }
        [Column("ID_ETAPA_ETL")]
        public Guid IdEtapaEtl { get; set; }
        [Column("CODIGO_ERROR")]
        public string? CodigoError { get; set; }
        [Column("DESCRIPCION_ERROR")]
        public string DescripcionError { get; set; } = default!;
        [Column("DETALLE_TECNICO")]
        public string? DetalleTecnico { get; set; }
        [Column("FECHA_ERROR")]
        public DateTime FechaError { get; set; }

        [Column("ID_TRANSACCION")]
        public Guid IdTransaccion { get; set; }
    }
}
