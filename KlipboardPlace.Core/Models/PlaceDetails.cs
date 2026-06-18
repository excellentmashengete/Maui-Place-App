using System.Text.Json.Serialization;

namespace KlipboardPlace.Core.Models;

/// <summary>
/// Detailed information for a place
/// </summary>
public class PlaceDetails
{
    [JsonPropertyName("placeId")]
    public string PlaceId { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("formattedAddress")]
    public string? FormattedAddress { get; set; }

    [JsonPropertyName("vicinity")]
    public string? Vicinity { get; set; }

    [JsonPropertyName("url")]
    public string? Url { get; set; }

    [JsonPropertyName("latitude")]
    public double Latitude { get; set; }

    [JsonPropertyName("longitude")]
    public double Longitude { get; set; }

    [JsonPropertyName("utcOffset")]
    public int UtcOffset { get; set; }
}
