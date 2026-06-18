using System.Net.Http.Json;
using KlipboardPlace.Core.Constants;
using KlipboardPlace.Core.Interfaces;
using KlipboardPlace.Core.Models;
using KlipboardPlace.Core.Settings;
using Microsoft.Extensions.Options;

namespace KlipboardPlace.Core.Services;

/// <summary>
/// Acquires OAuth2 access tokens via the client credentials flow and caches them in memory
/// until shortly before they expire.
/// </summary>
public class AuthService : IAuthService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly KlipboardSetting _settings;
    private readonly SemaphoreSlim _lock = new(1, 1);

    private TokenResponse? _cachedToken;
    
    public AuthService(IHttpClientFactory httpClientFactory, IOptions<KlipboardSetting> settings)
    {
        _httpClientFactory = httpClientFactory;
        _settings = settings.Value;
    }
    
    public async Task<(string token, string tokenType)> GetAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        if (_cachedToken is { } cached && !IsExpired(cached))
        {
            return (cached.AccessToken, cached.TokenType);
        }

        await _lock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            // Re-check inside the lock: another caller may have refreshed it.
            if (_cachedToken is { } current && !IsExpired(current))
            {
                return (current.AccessToken, current.TokenType);
            }

            var token = await RequestTokenAsync(cancellationToken).ConfigureAwait(false);
            _cachedToken = token;
            return (token.AccessToken, token.TokenType);
        }
        finally
        {
            _lock.Release();
        }
    }
    
    public void Invalidate() => _cachedToken = null;

    private bool IsExpired(TokenResponse token)
    {
        var expiresAt = DateTimeOffset
            .FromUnixTimeMilliseconds(token.Timestamp)
            .AddSeconds(token.ExpiresIn);
        
        return DateTimeOffset.UtcNow >= expiresAt;
    }
    
    private async Task<TokenResponse> RequestTokenAsync(CancellationToken cancellationToken)
    {
        var form = new Dictionary<string, string>
        {
            ["grant_type"] = _settings.GrantType,
            ["client_id"] = _settings.ClientId,
            ["client_secret"] = _settings.ClientSecret,
            ["scope"] = _settings.Scope
        };
        
        var httpClient = _httpClientFactory.CreateClient(HttpClients.KlipboardAuthApi);
        using var content = new FormUrlEncodedContent(form);
        
        using var response = await httpClient.PostAsync(
            "/connect/token", 
            content,
            cancellationToken
            ).ConfigureAwait(false);
        
        response.EnsureSuccessStatusCode();

        var token = await response.Content
            .ReadFromJsonAsync<TokenResponse>(cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        
        if (token is null || string.IsNullOrEmpty(token.AccessToken))
        {
            throw new InvalidOperationException("The identity service returned an empty access token.");
        }
        
        return token;
    }
}
