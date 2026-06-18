using KlipboardPlace.Core.Constants;
using KlipboardPlace.Core.Interfaces;
using KlipboardPlace.Core.Services;
using KlipboardPlace.Core.Settings;
using KlipboardPlace.Core.Handlers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace KlipboardPlace.Core;

public static class DependencyInjection
{
    /// <summary>
    /// Registers the dependencies for core app 
    /// </summary>
    public static IServiceCollection AddKlipboardCore(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<KlipboardSetting>()
            .Bind(configuration.GetSection("Klipboard"))
            .ValidateDataAnnotations()
            .ValidateOnStart();
        
        services.AddSingleton<IApiClient, ApiClient>();
        services.AddSingleton<IPlaceService, PlaceService>();
        services.AddSingleton<IAuthService, AuthService>();

        services.AddTransient<AuthTokenHandler>();
        
        services.AddHttpClient(HttpClients.KlipboardAuthApi, (sp, client) =>
        {
            var settings = sp.GetRequiredService<IOptions<KlipboardSetting>>().Value;
            client.BaseAddress = new Uri(settings.TokenEndpoint);
        });
        
        services.AddHttpClient(HttpClients.KlipboardApi, (sp, client) =>
        {
            var settings = sp.GetRequiredService<IOptions<KlipboardSetting>>().Value;
            client.BaseAddress = new Uri(settings.ApiBaseUrl);
        }).AddHttpMessageHandler<AuthTokenHandler>();;

        
        return services;
    }
}
