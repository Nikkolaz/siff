namespace SSF.Interop.SIIFNacion.Application.Contracts.Storage
{
    public interface IFileStorageService
    {
        Task<byte[]?> ReadAsync(string absolutePath, CancellationToken ct = default);
        // (opcional) SaveAsync, ExistsAsync, DeleteAsync…
    }
}
