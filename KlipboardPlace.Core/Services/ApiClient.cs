using System.Net.Http.Json;
using System.Text.Json;
using KlipboardPlace.Core.Constants;
using KlipboardPlace.Core.Interfaces;

namespace KlipboardPlace.Core.Services;

/// <summary>
/// Performs authenticated GET requests
/// obtained from <see cref="IAuthService"/>.
/// </summary>
public class ApiClient : IApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private readonly IHttpClientFactory _httpClientFactory;

    public ApiClient(IHttpClientFactory httpClientFactory, IAuthService authService)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<T?> GetAsync<T>(string requestUri, CancellationToken cancellationToken = default)
    {
        var httpClient = _httpClientFactory.CreateClient(HttpClients.KlipboardApi);
        using var response = await httpClient.GetAsync(requestUri, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        return await response.Content
            .ReadFromJsonAsync<T>(JsonOptions, cancellationToken)
            .ConfigureAwait(false);
    }
}
