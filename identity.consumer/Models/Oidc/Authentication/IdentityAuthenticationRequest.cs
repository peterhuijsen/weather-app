namespace Identity.Consumer.Models.Oidc.Authentication;

public class IdentityAuthenticationRequest
{
    public static IdentityAuthenticationRequestBuilder Create(string clientId, string redirectUri)
        => new IdentityAuthenticationRequestBuilder(new IdentityAuthenticationRequest
        {
            ClientId = clientId,
            RedirectUri = redirectUri
        });
    
    /// <summary>
    /// Gets or sets the client id used in the authentication request.
    /// </summary>
    public string ClientId { get; internal set; } = null!;

    /// <summary>
    /// Gets or sets the redirect uri of the authentication request, the uri to which the response from the
    /// OIDC provider will be sent.
    /// </summary>
    public string RedirectUri { get; internal set; } = null!;

    /// <summary>
    /// Gets or sets the scope of the authentication request.
    /// </summary>
    public string? Scope { get; internal set; }
    
    /// <summary>
    /// Gets or sets the state of the authentication request, used to prevent request forgery.
    /// </summary>
    public string? State { get; internal set; }
    
    /// <summary>
    /// Gets or sets the nonce of the authentication request.
    /// </summary>
    public string? Nonce { get; internal set; }
}

public class IdentityAuthenticationRequestBuilder
{
    private readonly IdentityAuthenticationRequest _instance;

    public IdentityAuthenticationRequestBuilder(IdentityAuthenticationRequest instance)
        => _instance = instance;

    public IdentityAuthenticationRequestBuilder WithScope(string scope)
    {
        _instance.Scope = scope;
        return this;
    }

    public IdentityAuthenticationRequestBuilder WithState(string state)
    {
        _instance.State = state;
        return this;
    }
    
    public IdentityAuthenticationRequestBuilder WithNonce(string nonce)
    {
        _instance.Nonce = nonce;
        return this;
    }

    public IdentityAuthenticationRequest Build()
        => _instance;
}