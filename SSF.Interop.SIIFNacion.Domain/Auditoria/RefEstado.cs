using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SSF.Interop.SIIFNacion.Service.Domain.Auditoria
{
    public class RefEstado
    {
        [Key]
        [Column("ID_ESTADO")]
        public Guid IdEstado { get; set; }
        [Column("COD_ESTADO")]
        public string CodEstado { get; set; } = null!;
        [Column("NOMBRE_ESTADO")]
        public string NombreEstado { get; set; } = null!;
        [Column("ID_TIPO_ESTADO")]
        public Guid IdTipoEstado { get; set; }
        [Column("ES_ERROR")] 
        public bool EsError { get; set; }
        [Column("ES_FINAL")]
        public bool EsFinal { get; set; }
        [Column("ORDEN")]
        public int Orden { get; set; }
        [Column("ACTIVO")]
        public bool Activo { get; set; }

        // Navigation property
        public RefTipoEstado TipoEstado { get; set; } = null!;
        public ICollection<AuditoriaEtapasEtl> Etapas { get; set; } = new List<AuditoriaEtapasEtl>();
    }
}
