using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SSF.Interop.SIIFNacion.Service.Domain.Auditoria
{
    public class ConveniosInteroperabilidad
    {

        [Key]
        [Column("ID_CONVENIO")]
        public Guid? IdConvenio { get; set; }
        [Column("COD_CONVENIO")]
        public string CodConvenio { get; set; }
        [Column("NOMBRE_CONVENIO")]
        public string NombreConvenio { get; set; }
        [Column("ID_ENTIDAD_PROVEEDORA")]
        public Guid IdEntidadProveedora { get; set; }
        [Column("ID_ENTIDAD_CONSUMIDORA")]
        public Guid IdEntidadConsumidora { get; set; }
        [Column("ID_TIPO_CONVENIO")]
        public Guid IdTipoConvenio { get; set; }
        [Column("ID_ESTADO")]
        public Guid IdEstado { get; set; }
        [Column("FECHA_INICIO_VIGENCIA")]
        public DateTime FechaInicioVigencia { get; set; }
        [Column("FECHA_FIN_VIGENCIA")]
        public DateTime FechaFinVigencia { get; set; }
    }
}
