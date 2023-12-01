using Newtonsoft.Json;

namespace Identity.Consumer.Models.Oidc;

public class OidcDiscoveryDocument
{
    [JsonProperty("issuer")]
    public string Issuer { get; set; } = null!;
    
    [JsonProperty("token_endpoint")]
    public string TokenEndpoint { get; set; } = null!;
    
    [JsonProperty("authorization_endpoint")]
    public string AuthorizationEndpoint { get; set; } = null!;
    
    [JsonProperty("userinfo_endpoint")]
    public string? UserInfoEndpoint { get; set; }
    

    [JsonProperty("jwks_uri")]
    public string JwksUri { get; set; } = null!;
    

    [JsonProperty("subject_types_supported")]
    public string[] SubjectTypes { get; set; } = null!;

    [JsonProperty("response_types_supported")]
    public string[] ResponseTypes { get; set; } = null!;

    [JsonProperty("id_token_signing_alg_values_supported")]
    public string[] IdTokenSigningAlgorithms { get; set; } = null!;
    
    [JsonProperty("scopes_supported")]
    public string[]? Scopes { get; set; }

    [JsonProperty("claims_supported")]
    public string[]? Claims { get; set; }
    
    [JsonProperty("response_modes_supported")]
    public string[]? ReponseModes { get; set; }
    
    [JsonProperty("token_endpoint_auth_methods_supported")]
    public string[]? TokenEndpointMethods { get; set; }

    [JsonProperty("request_uri_parameter_supported")]
    public bool? IsRequestUriParameterAvailable { get; set; } = true;
}