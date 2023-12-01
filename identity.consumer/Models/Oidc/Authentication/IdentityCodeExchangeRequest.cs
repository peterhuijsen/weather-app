namespace Identity.Consumer.Models.Oidc.Authentication;

public class IdentityCodeExchangeRequest
{
    public static IdentityCodeExchangeRequestBuilder Create(string code, string clientId, string clientSecret, string redirectUri)
        => new IdentityCodeExchangeRequestBuilder(new IdentityCodeExchangeRequest
        {
            Code = code,
            ClientId = clientId,
            ClientSecret = clientSecret,
            RedirectUri = redirectUri
        });

    /// <summary>
    /// Gets or sets the authorization code retrieved from the OIDC provider.
    /// </summary>
    public string Code { get; private set; } = null!;
    
    /// <summary>
    /// Gets or sets the client id used in the exchange request.
    /// </summary>
    public string ClientId { get; private set; } = null!;
    
    /// <summary>
    /// Gets or sets the client secret used in the exchange request.
    /// </summary>
    public string ClientSecret { get; private set; } = null!;

    /// <summary>
    /// Gets or sets the redirect uri of the exchange request, the uri to which the response from the
    /// OIDC provider will be sent.
    /// </summary>
    public string RedirectUri { get; private set; } = null!;
}

public class IdentityCodeExchangeRequestBuilder
{
    private readonly IdentityCodeExchangeRequest _instance;

    public IdentityCodeExchangeRequestBuilder(IdentityCodeExchangeRequest instance)
        => _instance = instance;

    public IdentityCodeExchangeRequest Build()
        => _instance;
}