using Newtonsoft.Json;

namespace Identity.Consumer.Models.Tokens;

public class Jwt
{
    public string Header { get; set; } = null!;
    public string Payload { get; set; } = null!;
    public string Signature { get; set; } = null!;
}

public class JwtHeader
{
    [JsonProperty("typ")] 
    public string Type { get; set; } = null!;
    
    [JsonProperty("alg")] 
    public string Algorithm { get; set; } = null!;

    [JsonProperty("kid")]
    public string KeyId { get; set; } = null!;
}