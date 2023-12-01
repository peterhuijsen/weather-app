using Newtonsoft.Json;

namespace Identity.Consumer.Models.Oidc;

public class OidcJwksDocument
{
    [JsonProperty("keys")]
    public OidcJwk[] Keys { get; set; } = null!;
}

public class OidcJwk
{
    [JsonProperty("kty")]
    public string Type { get; set; } = null!;

    [JsonProperty("use")]
    public string Use { get; set; } = null!;

    [JsonProperty("kid")]
    public string KeyId { get; set; } = null!;

    [JsonProperty("n")]
    public string Modulus { get; set; } = null!;
    
    [JsonProperty("e")]
    public string Exponent { get; set; } = null!;
}