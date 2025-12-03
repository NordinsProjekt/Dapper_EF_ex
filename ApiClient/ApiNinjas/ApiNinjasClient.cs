using System.Net.Http;
using System.Net.Http.Json;

namespace ApiClient.ApiNinjas;

public interface IApiNinjasClient : IApiClient
{
    Task<T?> GetAsync<T>(string endpoint, Dictionary<string,string>? query = null, CancellationToken cancellationToken = default);
}

internal sealed class ApiNinjasClient(IApiClient inner, string apiKey) : IApiNinjasClient
{
    public Uri BaseAddress => inner.BaseAddress;

    public void SetDefaultHeader(string name, string value) => inner.SetDefaultHeader(name, value);

    public Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken = default)
        => inner.SendAsync(request, cancellationToken);

    public Task DeleteAsync(string relativeUrl, CancellationToken cancellationToken = default)
        => inner.DeleteAsync(relativeUrl, cancellationToken);

    public Task<T?> GetAsync<T>(string relativeUrl, CancellationToken cancellationToken = default)
        => inner.GetAsync<T>(relativeUrl, cancellationToken);

    public Task<TResponse?> PostAsync<TRequest, TResponse>(string relativeUrl, TRequest payload, CancellationToken cancellationToken = default)
        => inner.PostAsync<TRequest, TResponse>(relativeUrl, payload, cancellationToken);

    public Task<TResponse?> PutAsync<TRequest, TResponse>(string relativeUrl, TRequest payload, CancellationToken cancellationToken = default)
        => inner.PutAsync<TRequest, TResponse>(relativeUrl, payload, cancellationToken);

    public async Task<T?> GetAsync<T>(string endpoint, Dictionary<string, string>? query = null, CancellationToken cancellationToken = default)
    {
        var builder = new UriBuilder(new Uri(BaseAddress, endpoint));
        if (query is not null && query.Count > 0)
        {
            var queryString = string.Join('&', query.Select(kvp => $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));
            builder.Query = queryString;
        }
        var request = new HttpRequestMessage(HttpMethod.Get, builder.Uri);
        request.Headers.TryAddWithoutValidation("X-Api-Key", apiKey);
        var response = await SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode) return default;
        return await response.Content.ReadFromJsonAsync<T>(cancellationToken: cancellationToken);
    }
}
