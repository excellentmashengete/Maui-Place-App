using System.Text.Json.Serialization;

namespace KlipboardPlace.Core.Models;

/// <summary>
/// OAuth2 token endpoint response for the client credentials flow.
/// </summary>
public class TokenResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = string.Empty;

    [JsonPropertyName("token_type")]
    public string TokenType { get; set; }  = string.Empty;
    
    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }

    [JsonPropertyName("scope")]
    public string? Scope { get; set; }
    
    [JsonPropertyName("timestamp")]
    public long Timestamp { get; set; }
}
