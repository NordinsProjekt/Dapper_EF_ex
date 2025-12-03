using System.Net.Http;
using System.Net.Http.Json;

namespace ApiClient.Http;

internal class GenericApiClient(HttpClient httpClient) : IApiClient
{
    public Uri BaseAddress => httpClient.BaseAddress!;

    public async Task<T?> GetAsync<T>(string relativeUrl, CancellationToken cancellationToken = default)
    {
        return await httpClient.GetFromJsonAsync<T>(relativeUrl, cancellationToken);
    }

    public async Task<TResponse?> PostAsync<TRequest, TResponse>(string relativeUrl, TRequest payload, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsJsonAsync(relativeUrl, payload, cancellationToken);
        if (!response.IsSuccessStatusCode) return default;
        return await response.Content.ReadFromJsonAsync<TResponse>(cancellationToken: cancellationToken);
    }

    public async Task<TResponse?> PutAsync<TRequest, TResponse>(string relativeUrl, TRequest payload, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PutAsJsonAsync(relativeUrl, payload, cancellationToken);
        if (!response.IsSuccessStatusCode) return default;
        return await response.Content.ReadFromJsonAsync<TResponse>(cancellationToken: cancellationToken);
    }

    public async Task DeleteAsync(string relativeUrl, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.DeleteAsync(relativeUrl, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken = default)
    {
        return httpClient.SendAsync(request, cancellationToken);
    }

    public void SetDefaultHeader(string name, string value)
    {
        if (httpClient.DefaultRequestHeaders.Contains(name))
        {
            httpClient.DefaultRequestHeaders.Remove(name);
        }
        httpClient.DefaultRequestHeaders.Add(name, value);
    }
}
