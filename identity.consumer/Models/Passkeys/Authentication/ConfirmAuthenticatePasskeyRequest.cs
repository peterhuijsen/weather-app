using Newtonsoft.Json;

namespace Identity.Consumer.Models.Passkeys.Authentication;

public class ConfirmPasskeyRegistrationAssertionResponse
{
    [JsonProperty("clientDataJSON")]
    public string ClientDataJson { get; set; }

    [JsonProperty("authenticatorData")]
    public string AuthenticatorData { get; set; }
    
    [JsonProperty("signature")]
    public string Signature { get; set; }
    
    [JsonProperty("user")]
    public string User { get; set; }
} 

public class ConfirmAuthenticatePasskeyRequest
{
    [JsonProperty("id")]
    public string Id { get; set; }
    
    [JsonProperty("type")]
    public string Type { get; set; }
    
    [JsonProperty("authenticatorAttachment")]
    public string AuthenticatorAttachment { get; set; }

    [JsonProperty("response")]
    public ConfirmPasskeyRegistrationAssertionResponse Response { get; set; }
}