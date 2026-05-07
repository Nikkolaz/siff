using SSF.Interop.SIIFNacion.Service.Domain.Auditoria;

namespace SSF.Interop.SIIFNacion.Service.Application.Contracts.Persistence
{
    public interface IMaeEstruturasDatosRepository
    {
        Task<MaeEstructurasDatos?> GetByCodigoAsync(string codigoEstuctura);
    }
}
