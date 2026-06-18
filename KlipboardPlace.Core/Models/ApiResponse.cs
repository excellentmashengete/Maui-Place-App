using System.Text.Json.Serialization;

namespace KlipboardPlace.Core.Models;

public class ApiResponse<T>
{
    [JsonPropertyName("data")]
    public T? Data { get; set; }

    [JsonPropertyName("meta")]
    public ResponseMeta? Meta { get; set; }
}

public class ResponseMeta
{
    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("totalCount")]
    public int TotalCount { get; set; }
}
