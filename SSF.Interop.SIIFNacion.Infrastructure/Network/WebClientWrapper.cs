using SSF.Interop.SIIFNacion.Application.Models.Api;
using System.Net;
using System.Text;

namespace SSF.Interop.SIIFNacion.Infrastructure.Network
{
    public class WebClientWrapper : IWebClientWrapper
    {
        public async Task<string> Get(string url, CancellationToken cancellationToken)
        {
            string m_claseMetodo = $"{this.GetType().Name} - {System.Reflection.MethodBase.GetCurrentMethod()?.Name ?? nameof(Get)}";
            try
            {
                using HttpClient client = new();
                HttpResponseMessage result = await client.GetAsync(url, cancellationToken);
                if (result.IsSuccessStatusCode)
                {
                    return await result.Content.ReadAsStringAsync();
                }
                else
                {
                    var response = await result.Content.ReadAsStringAsync();
                    throw new($"StatusCode {result.StatusCode} {m_claseMetodo} Exception {response}");
                }
            }
            catch (WebException exception)
            {
                throw new($"{m_claseMetodo} Exception {exception.Message}");
            }
        }

        public async Task<ApiResponse<string>> Post(string url, string data, CancellationToken cancellationToken)
        {
            string m_claseMetodo = $"{this.GetType().Name} - {System.Reflection.MethodBase.GetCurrentMethod()?.Name ?? nameof(Post)}";
            try
            {
                ApiResponse<string> apiResponse = new();
                var content = new StringContent(data, Encoding.UTF8, "application/json");
                using HttpClient client = new();
                HttpResponseMessage result = await client.PostAsync(url, content, cancellationToken);

                if (result.IsSuccessStatusCode)
                {
                    apiResponse.Data = await result.Content.ReadAsStringAsync();
                    apiResponse.Success = true;
                }
                else
                {
                    apiResponse.Data = string.Empty;
                    string response = await result.Content.ReadAsStringAsync();
                    apiResponse.ValidationErrors = $"StatusCode {result.StatusCode} response {response}";
                }

                return apiResponse;
            }
            catch (WebException ex)
            {
                throw new($"{m_claseMetodo} Exception {ex.Message}");
            }
        }

        public async Task<string> Put(string url, string data, CancellationToken cancellationToken)
        {
            string m_claseMetodo = $"{this.GetType().Name} - {System.Reflection.MethodBase.GetCurrentMethod()?.Name ?? nameof(Put)}";
            try
            {
                var content = new StringContent(data, Encoding.UTF8, "application/json");
                using HttpClient client = new();
                HttpResponseMessage result = await client.PutAsync(url, content, cancellationToken);
                if (result.IsSuccessStatusCode)
                {
                    return await result.Content.ReadAsStringAsync();
                }
                else
                {
                    var response = await result.Content.ReadAsStringAsync();
                    throw new($"{m_claseMetodo} Exception {response}");
                }
            }
            catch (WebException ex)
            {
                throw new($"{m_claseMetodo} Exception {ex.Message}");
            }
        }

        public async Task<string> Delete(string url, CancellationToken cancellationToken)
        {
            string m_claseMetodo = $"{this.GetType().Name} - {System.Reflection.MethodBase.GetCurrentMethod()?.Name ?? nameof(Delete)}";
            try
            {
                using HttpClient client = new();
                HttpResponseMessage result = await client.DeleteAsync(url, cancellationToken);
                if (result.IsSuccessStatusCode)
                {
                    return await result.Content.ReadAsStringAsync();
                }
                else
                {
                    var response = await result.Content.ReadAsStringAsync();
                    throw new($"{m_claseMetodo} Exception {response}");
                }
            }
            catch (WebException ex)
            {
                throw new($"{m_claseMetodo} Exception {ex.Message}");
            }
        }

        public async Task<string> GetWithHeader(List<KeyValuePair<string, string>> headers, string url, CancellationToken cancellationToken)
        {
            string m_claseMetodo = $"{this.GetType().Name} - {System.Reflection.MethodBase.GetCurrentMethod()?.Name ?? nameof(GetWithHeader)}";
            try
            {
                using HttpClient client = new();
                foreach (var header in headers)
                    client.DefaultRequestHeaders.Add(header.Key, header.Value);

                HttpResponseMessage result = await client.GetAsync(url);
                if (result.IsSuccessStatusCode)
                {
                    return await result.Content.ReadAsStringAsync();
                }
                else
                {
                    var response = await result.Content.ReadAsStringAsync();
                    throw new($"{m_claseMetodo} Exception {response}");
                }
            }
            catch (WebException exception)
            {
                throw new($"{m_claseMetodo} Exception {exception.Message}");
            }
        }

        public async Task<ApiResponse<string>> PostWithHeader(List<KeyValuePair<string, string>> headers, string url, string data, CancellationToken cancellationToken)
        {
            string m_claseMetodo = $"{this.GetType().Name} - {System.Reflection.MethodBase.GetCurrentMethod()?.Name ?? nameof(PostWithHeader)}";
            try
            {
                ApiResponse<string> apiResponse = new();
                var content = new StringContent(data, Encoding.UTF8, "application/json");
                using HttpClient client = new();

                foreach (var header in headers)
                    client.DefaultRequestHeaders.Add(header.Key, header.Value);

                HttpResponseMessage result = await client.PostAsync(url, content, cancellationToken);

                if (result.IsSuccessStatusCode)
                {
                    apiResponse.Data = await result.Content.ReadAsStringAsync();
                    apiResponse.Success = true;
                }
                else
                {
                    apiResponse.Data = string.Empty;
                    apiResponse.ValidationErrors = await result.Content.ReadAsStringAsync();
                }

                return apiResponse;
            }
            catch (WebException ex)
            {
                throw new($"{m_claseMetodo} Exception {ex.Message}");
            }
        }

        public async Task<string> GetHealthCheck(string url, CancellationToken cancellationToken)
        {
            string m_claseMetodo = $"{this.GetType().Name} - {System.Reflection.MethodBase.GetCurrentMethod()?.Name ?? nameof(GetHealthCheck)}";
            try
            {
                using HttpClient client = new();
                HttpResponseMessage result = await client.GetAsync(url, cancellationToken);
                return await result.Content.ReadAsStringAsync();
            }
            catch (WebException exception)
            {
                throw new($"{m_claseMetodo} Exception {exception.Message}");
            }
        }
    }
}
