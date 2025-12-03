using System.Net.Http;
using ApiClient.ApiNinjas;
using ApiClient.Factories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ApiClient;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddGenericApiClient(this IServiceCollection services)
    {
        services.AddHttpClient();
        services.AddOptions<ApiClientOptions>();
        services.AddSingleton<IApiClientFactory, ApiClientFactory>();
        return services;
    }

    public static IServiceCollection AddApiClient(this IServiceCollection services, string name, Action<ApiClientOptions> configure)
    {
        services.AddOptions<ApiClientOptions>(name).Configure(configure);
        services.AddHttpClient(name, (sp, client) =>
        {
            var opts = sp.GetRequiredService<Microsoft.Extensions.Options.IOptionsMonitor<ApiClientOptions>>().Get(name);
            client.BaseAddress = new Uri(opts.BaseUrl);
            foreach (var h in opts.DefaultHeaders)
            {
                client.DefaultRequestHeaders.TryAddWithoutValidation(h.Key, h.Value);
            }
        });
        return services;
    }

    public static IServiceCollection AddApiNinjasClient(this IServiceCollection services, string name, string apiKey, Action<ApiClientOptions> configure)
    {
        services.AddApiClient(name, configure);
        services.AddScoped<IApiNinjasClient>(sp =>
        {
            var factory = sp.GetRequiredService<IApiClientFactory>();
            var inner = factory.Create(name);
            return new ApiNinjasClient(inner, apiKey);
        });
        return services;
    }
}
