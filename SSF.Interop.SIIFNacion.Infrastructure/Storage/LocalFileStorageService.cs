using SSF.Interop.SIIFNacion.Application.Contracts.Storage;

namespace SSF.Interop.SIIFNacion.Infrastructure.Storage
{
    public class LocalFileStorageService : IFileStorageService
    {
        public async Task<byte[]?> ReadAsync(string absolutePath, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(absolutePath) || !File.Exists(absolutePath))
                return null;

            return await File.ReadAllBytesAsync(absolutePath, ct);
        }
    }
}
