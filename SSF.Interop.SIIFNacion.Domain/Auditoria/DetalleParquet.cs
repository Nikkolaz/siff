using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SSF.Interop.SIIFNacion.Service.Domain.Auditoria
{
    public class DetalleParquet
    {
        [Key]
        [Column("ID_DETALLE_PARQUET")]
        public Guid IdDetalleParquet { get; set; }
        [Column("ID_ETAPA_ETL")]
        public Guid IdEtapaEtl { get; set; }
        [Column("NOMBRE_ARCHIVO")]
        public string? NombreArchivo { get; set; } = null!;
        [Column("TAMANIO_BYTES")]
        public long TamanioBytes { get; set; }
        [Column("HASH_ARCHIVO")]
        public string? HashArchivo { get; set; }
        [Column("COMPRESION")]
        public string? Compresion { get; set; }

        [Column("ID_TRANSACCION")]
        public Guid IdTransaccion { get; set; }
        [Column("ID_ESTADO")]
        public Guid IdEstado { get; set; }

        [Column("FECHA_CREACION")]
        public DateTime FechaCreacion { get; set; }
        [Column("FECHA_MODIFICACION")]
        public DateTime? FechaModificacion { get; set; }
        [Column("CANTIDAD_REGISTROS")]
        public int CantidadRegistros { get; set; }
        [Column("WK_MES")]
        public int? WkMes { get; set; }
        [Column("COD_ESTRUCTURA")]
        public string? CodEstructura { get; set; }

    }
}
