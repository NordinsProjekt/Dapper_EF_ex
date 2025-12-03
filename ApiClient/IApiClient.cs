using System.Net.Http;

namespace ApiClient;

public interface IApiClient
{
    Uri BaseAddress { get; }
    Task<T?> GetAsync<T>(string relativeUrl, CancellationToken cancellationToken = default);
    Task<TResponse?> PostAsync<TRequest, TResponse>(string relativeUrl, TRequest payload, CancellationToken cancellationToken = default);
    Task<TResponse?> PutAsync<TRequest, TResponse>(string relativeUrl, TRequest payload, CancellationToken cancellationToken = default);
    Task DeleteAsync(string relativeUrl, CancellationToken cancellationToken = default);
    Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken = default);
    void SetDefaultHeader(string name, string value);
}
