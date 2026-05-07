using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SSF.Interop.SIIFNacion.Service.Domain.Auditoria
{
    public class RefTipoEstado
    {
        [Key]
        [Column("ID_TIPO_ESTADO")]
        public Guid IdTipoEstado { get; set; }
        [Column("COD_TIPO_ESTADO")]
        public string CodTipoEstado { get; set; }
        [Column("NOMBRE_TIPO_ESTADO")]
        public string NombreTipoEstado { get; set; }
        [Column("DESCRIPCION")]
        public string Descripcion { get; set; }

        // 🔗 Relación: un tipo tiene muchos estados
        public ICollection<RefEstado> Estados { get; set; }
        public ICollection<RefEstado> RefEstados { get; set; } = new List<RefEstado>();

    }
}
