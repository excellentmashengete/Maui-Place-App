using System.Text.Json.Serialization;

namespace KlipboardPlace.Core.Models;

/// <summary>
/// A single high-level search result
/// </summary>
public class Place
{
    [JsonPropertyName("placeId")]
    public string PlaceId { get; set; } = string.Empty;
    
    [JsonPropertyName("mainText")]
    public string MainText { get; set; } = string.Empty;

    [JsonPropertyName("secondaryText")]
    public string? SecondaryText { get; set; }
}
