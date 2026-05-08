using SSF.Interop.SIIFNacion.Service.Domain.Auditoria;


namespace SSF.Interop.SIIFNacion.Service.Application.Contracts.Persistence
{
    public  interface IRefEstadosRepository
    {
        Task<RefEstado?> GetByCodigoAsync(string codigoEstado);
    }
}
