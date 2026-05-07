using SSF.Interop.SIIFNacion.Application.Models.Api;

namespace SSF.Interop.SIIFNacion.Infrastructure.Network
{
    public interface IWebClientWrapper
    {
        Task<string> Get(string url, CancellationToken cancellationToken);
        Task<ApiResponse<string>> Post(string url, string data, CancellationToken cancellationToken);
        Task<string> Put(string url, string data, CancellationToken cancellationToken);
        Task<string> Delete(string url, CancellationToken cancellationToken);
        Task<string> GetWithHeader(List<KeyValuePair<string, string>> headers, string url, CancellationToken cancellationToken);
        Task<ApiResponse<string>> PostWithHeader(List<KeyValuePair<string, string>> headers, string url, string data, CancellationToken cancellationToken);
        Task<string> GetHealthCheck(string url, CancellationToken cancellationToken);
    }
}