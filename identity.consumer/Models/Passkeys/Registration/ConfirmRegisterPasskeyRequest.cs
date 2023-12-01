using Newtonsoft.Json;

namespace Identity.Consumer.Models.Passkeys.Registration;

public class ConfirmPasskeyRegistrationAttestationResponse
{
    [JsonProperty("clientDataJSON")]
    public string ClientDataJson { get; set; }

    [JsonProperty("attestationObject")]
    public string AttestationObject { get; set; }
} 

public class ConfirmRegisterPasskeyRequest
{
    [JsonProperty("id")]
    public string Id { get; set; }
    
    [JsonProperty("type")]
    public string Type { get; set; }
    
    [JsonProperty("authenticatorAttachment")]
    public string AuthenticatorAttachment { get; set; }
    
    [JsonProperty("response")]
    public ConfirmPasskeyRegistrationAttestationResponse Response { get; set; }
}