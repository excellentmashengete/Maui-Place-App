namespace KlipboardPlace.Core.Settings;

///
public sealed class KlipboardSetting
{
    /// <summary>Base address of the Location API.</summary>
    public string ApiBaseUrl { get; set; } = string.Empty;

    ///
    public string TokenEndpoint { get; set; } = string.Empty;

    /// 
    public string ClientId { get; set; } = string.Empty;

    /// 
    public string ClientSecret { get; set; } = string.Empty;

    /// 
    public string GrantType { get; set; } = string.Empty;

    /// 
    public string Scope { get; set; } = string.Empty;
}
