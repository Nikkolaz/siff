using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SSF.Interop.SIIFNacion.Service.Domain.Auditoria
{
    public class MaeEstructurasDatos
    {
        [Key]
        [Column("ID_ESTRUCTURA")]
        public Guid? IdEstructura { get; set; }
        [Column("COD_ESTRUCTURA")]
        public string CodEstructura { get; set; }
        [Column("NOMBRE_ESTRUCTURA")]
        public string NombreEstructura { get; set; }
        [Column("VISTA_ORIGEN")]
        public string VistaOrigen { get; set; }
        [Column("PREFIJO_ARCHIVO")]
        public string PrefijoArchivo { get; set; }
        [Column("ID_PERIODICIDAD")]
        public Guid IdPeriodicidad { get; set; }
        [Column("ID_ESTADO")]
        public Guid IdEstado { get; set; }

    }
}
