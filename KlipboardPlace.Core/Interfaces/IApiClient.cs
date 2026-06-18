namespace KlipboardPlace.Core.Interfaces;

/// <summary>
/// Thin HTTP wrapper that performs authenticated Api requests and deserializes the JSON response.
/// </summary>
public interface IApiClient
{
    /// <summary>
    /// Sends an authenticated GET request to <paramref name="requestUri"/> (relative to the
    /// configured API base address) and deserializes the response body to <typeparamref name="T"/>.
    /// </summary>
    Task<T?> GetAsync<T>(string requestUri, CancellationToken cancellationToken = default);
}
