using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.ApiClient;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApiClient(this IServiceCollection services)
    {
        services.AddHttpClient<IApiClient, ApiClient>();
        return services;
    }

    public static IServiceCollection AddApiClient(this IServiceCollection services, string baseUri)
    {
        services.AddHttpClient<IApiClient, ApiClient>(client =>
        {
            client.BaseAddress = new Uri(baseUri);
        });
        return services;
    }

    public static IServiceCollection AddApiClient(this IServiceCollection services, Action<HttpClient> configureClient)
    {
        services.AddHttpClient<IApiClient, ApiClient>(configureClient);
        return services;
    }

    public static IServiceCollection AddApiClient(this IServiceCollection services, string baseUri, Dictionary<string, string> defaultHeaders)
    {
        services.AddHttpClient<IApiClient, ApiClient>(client =>
        {
            client.BaseAddress = new Uri(baseUri);
            
            // Add default headers to HttpClient
            foreach (var header in defaultHeaders)
            {
                client.DefaultRequestHeaders.Add(header.Key, header.Value);
            }
        });
        return services;
    }

    public static IServiceCollection AddApiClientWithAuth(this IServiceCollection services, string baseUri, string token, string scheme = "Bearer")
    {
        services.AddHttpClient<IApiClient, ApiClient>(client =>
        {
            client.BaseAddress = new Uri(baseUri);
            client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue(scheme, token);
        });
        return services;
    }

    public static IServiceCollection AddApiClientWithApiKey(this IServiceCollection services, string baseUri, string apiKey, string headerName = "X-API-Key")
    {
        services.AddHttpClient<IApiClient, ApiClient>(client =>
        {
            client.BaseAddress = new Uri(baseUri);
            client.DefaultRequestHeaders.Add(headerName, apiKey);
        });
        return services;
    }
}