using SSF.Interop.SIIFNacion.Service.Domain.Auditoria;

namespace SSF.Interop.SIIFNacion.Service.Application.Contracts.Persistence
{
    public interface IConveniosInteroperabilidadRepository
    {
        Task<ConveniosInteroperabilidad?> GetByCodigoAsync(string codigoConvenio);
    }
}
