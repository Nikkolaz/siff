using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SSF.Interop.SIIFNacion.Service.Domain.Auditoria
{
    [Table("REF_TIPOS_ETAPA", Schema = "INTEROP_SSF")]
    public class RefTipoEtapa
    {
        [Key]
        [Column("ID_TIPO_ETAPA")]
        public Guid IdTipoEtapa { get; set; }

        [Required]
        [Column("COD_TIPO_ETAPA")]
        [MaxLength(50)]
        public string? CodTipoEtapa { get; set; }

        [Required]
        [Column("NOMBRE_ETAPA")]
        [MaxLength(100)]
        public string? NombreEtapa { get; set; }

        [Column("DESCRIPCION")]
        [MaxLength(255)]
        public string? Descripcion { get; set; }

        [Required]
        [Column("ORDEN")]
        public int Orden { get; set; }

         //🔁 Relaciones(se agregarán cuando envíes más tablas)
         public virtual ICollection<AuditoriaEtapasEtl> Auditorias { get; set; }
    }
}
