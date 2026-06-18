using KlipboardPlace.Core.Models;

namespace KlipboardPlace.Core.Interfaces;

/// <summary>
/// Obtains (and caches) OAuth2 access tokens using the client credentials flow.
/// </summary>
public interface IAuthService
{
    Task<(string token, string tokenType)> GetAccessTokenAsync(CancellationToken cancellationToken = default);
    
    void Invalidate();
}
