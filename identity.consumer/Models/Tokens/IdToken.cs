using System.Text;
using Identity.Consumer.Helpers;
using Newtonsoft.Json;

namespace Identity.Consumer.Models.Tokens;

public class IdToken
{
    public JwtHeader Header { get; set; }
    public IdTokenPayload Payload { get; set; }

    public Jwt Raw { get; set; }

    public IdToken(string input)
    {
        var parts = input.Split('.');
        var (header, payload, signature) = (parts[0], parts[1], parts[2]);

        var decodedHeader = Encoding.UTF8.GetString(Base64Url.Decode(header));
        Header = JsonConvert.DeserializeObject<JwtHeader>(decodedHeader);
            
        var decodedPayload = Encoding.UTF8.GetString(Base64Url.Decode(payload));
        Payload = JsonConvert.DeserializeObject<IdTokenPayload>(decodedPayload);
        
        Payload.Claims = JsonConvert.DeserializeObject<Dictionary<string, string>>(decodedPayload);
        Payload.Expiration = DateTimeOffset.FromUnixTimeSeconds(long.Parse(Payload.Claims["exp"]));
        Payload.IssuedAt = DateTimeOffset.FromUnixTimeSeconds(long.Parse(Payload.Claims["iat"]));

        Raw = new Jwt
        {
            Header = header,
            Payload = payload,
            Signature = signature
        };
    }
}

public class IdTokenPayload
{
    [JsonProperty("iss")]
    public string Issuer { get; set; } = null!;

    [JsonProperty("sub")]
    public string Subject { get; set; } = null!;

    [JsonProperty("aud")]
    public string Audience { get; set; } = null!;

    public DateTimeOffset Expiration { get; set; }

    public DateTimeOffset IssuedAt { get; set; }

    [JsonProperty("nonce")]
    public string? Nonce { get; set; }
    
    [JsonProperty("azp")]
    public string? AuthorizedParty { get; set; }

    [JsonProperty("name")]
    public string? Name { get; set; }
    [JsonProperty("preferred_name")]
    public string? PreferredName { get; set; }
    [JsonProperty("email")]
    public string? Email { get; set; }
    [JsonProperty("email_verified")]
    public bool? EmailVerified { get; set; }
    [JsonProperty("picture")]
    public string? Picture { get; set; }

    public Dictionary<string, string> Claims { get; set; } = null!;
}