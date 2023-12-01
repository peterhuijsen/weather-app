using Newtonsoft.Json;

namespace Identity.Consumer.Models.Oidc;

public class OidcCodeExchangeResponse
{
    [JsonProperty("access_token")]
    public string AccessToken { get; set; } = null!;

    [JsonProperty("refresh_token")]
    public string? RefreshToken { get; set; }

    [JsonProperty("id_token")]
    public string IdToken { get; set; } = null!;

    [JsonProperty("token_type")]
    public TokenType TokenType { get; set; }

    [JsonProperty("scope")]
    public string? Scope { get; set; }
}

public enum TokenType
{
    Bearer,
    Mac
}