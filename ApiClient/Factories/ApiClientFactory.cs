using System.Net.Http;
using Microsoft.Extensions.Options;

namespace ApiClient.Factories;

public interface IApiClientFactory
{
    IApiClient Create(string name);
}

internal sealed class ApiClientFactory : IApiClientFactory
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IOptionsMonitor<ApiClientOptions> _optionsMonitor;

    public ApiClientFactory(IHttpClientFactory httpClientFactory, IOptionsMonitor<ApiClientOptions> optionsMonitor)
    {
        _httpClientFactory = httpClientFactory;
        _optionsMonitor = optionsMonitor;
    }

    public IApiClient Create(string name)
    {
        var client = _httpClientFactory.CreateClient(name);
        var options = _optionsMonitor.Get(name);
        if (client.BaseAddress is null)
        {
            client.BaseAddress = new Uri(options.BaseUrl);
        }
        foreach (var kvp in options.DefaultHeaders)
        {
            if (!client.DefaultRequestHeaders.Contains(kvp.Key))
            {
                client.DefaultRequestHeaders.Add(kvp.Key, kvp.Value);
            }
        }
        return new Http.GenericApiClient(client);
    }
}
