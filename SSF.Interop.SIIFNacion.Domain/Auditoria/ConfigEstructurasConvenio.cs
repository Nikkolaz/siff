namespace SSF.Interop.SIIFNacion.Service.Domain.Auditoria
{
    public class ConfigEstructurasConvenio
    {
        public Guid IdConfigEstructura { get; set; }
        public Guid IdConvenio { get; set; }
        public Guid IdEstructura { get; set; }
        public int OrdenEjecucion { get; set; }
        public Guid IdEstado { get; set; }
    }
}
