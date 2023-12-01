using System.Security.Cryptography;
using System.Text;
using Flurl;
using Flurl.Http;
using Identity.Consumer.Helpers;
using Identity.Consumer.Models.Oidc;
using Identity.Consumer.Models.Oidc.Authentication;
using Identity.Consumer.Models.Tokens;
using Newtonsoft.Json;

namespace Identity.Consumer.Services;

/// <summary>
/// A contract used to define the settings an OIDC consumer will need to execute the authorization
/// code flow with the current OIDC provider.
/// </summary>
public interface IIdentityConsumer
{
    /// <summary>
    /// Generate a new anti-forgery state token to prevent request tampering.
    /// </summary>
    /// <returns>The generated state token used in future requests.</returns>
    string GenerateAntiForgeryToken();

    /// <summary>
    /// Verify a given anti-forgery state token.
    /// </summary>
    /// <param name="challenge">The retrieved token from a request which needs to be verified.</param>
    /// <returns>Whether or not the token is valid.</returns>
    bool VerifyAntiForgeryToken(string challenge);
    
    /// <summary>
    /// Verify a given nonce value.
    /// </summary>
    /// <param name="challenge">The retrieved nonce from a request which needs to be verified.</param>
    /// <returns>Whether or not the nonce is valid.</returns>
    bool VerifyNonce(string challenge);

    /// <summary>
    /// Send an authentication request to the OIDC provider of the current consumer. A callback will be called
    /// by the OIDC provider which contains the authorization code needed in the <see cref="SendCodeExchangeRequest"/>.
    /// </summary>
    /// <param name="request">The options for the request which should be sent.</param>
    Task<Uri> BuildAuthenticationRedirectUri(IdentityAuthenticationRequest request);

    /// <summary>
    /// Send a code exchange request to the OIDC provider of the current consumer. A callback will be called
    /// by the OIDC provider which contains the ID-token code (and optional access token) for the current user account.
    /// </summary>
    /// <param name="request">The options for the request which should be sent.</param>
    Task<IdentityCodeExchangeResponse?> SendCodeExchangeRequest(IdentityCodeExchangeRequest request);

    /// <summary>
    /// Verify whether a given <see cref="IdToken"/> is valid according to the
    /// <see href="https://openid.net/specs/openid-connect-core-1_0.html#CodeIDToken">spec</see>.
    /// </summary>
    /// <param name="token">The <see cref="IdToken"/> which should be verified.</param>
    /// <param name="settings">The settings by which the <see cref="IdToken"/> should be verified.</param>
    /// <returns>Whether or not the given <see cref="IdToken"/> is a valid <see cref="IdToken"/>.</returns>
    Task<bool> VerifyIdToken(IdToken token, IdentityIdTokenVerificationSettings settings);
}

public abstract class IdentityConsumer : IIdentityConsumer
{
    private readonly string _discoveryUri;

    private readonly List<string> _stateTokenCache = new();
    private readonly List<string> _nonceCache = new();

    private OidcDiscoveryDocument? _settings;
    private OidcJwksDocument? _keys;
    
    protected IdentityConsumer(string discoveryUri)
        => _discoveryUri = discoveryUri;

    /// <summary>
    /// Initialize the identity consumer by retrieving the settings of the provider
    /// from the given discovery document.
    /// </summary>
    private async Task Initialize()
    {
        var settingsJson = await _discoveryUri.GetStringAsync();
        _settings = JsonConvert.DeserializeObject<OidcDiscoveryDocument>(settingsJson);

        var keysJson = await _settings!.JwksUri.GetStringAsync();
        _keys = JsonConvert.DeserializeObject<OidcJwksDocument>(keysJson);
    }

    /// <inheritdoc cref="IIdentityConsumer.GenerateAntiForgeryToken"/>
    public virtual string GenerateAntiForgeryToken()
    {
        var state = Convert.ToBase64String(RandomNumberGenerator.GetBytes(256));
        _stateTokenCache.Add(state);

        return state;
    }

    /// <inheritdoc cref="IIdentityConsumer.VerifyAntiForgeryToken"/>
    public virtual bool VerifyAntiForgeryToken(string challenge)
    {
        var result = _stateTokenCache.Contains(challenge);
        if (result) _stateTokenCache.Remove(challenge);

        return result;
    }
    
    /// <inheritdoc cref="IIdentityConsumer.VerifyNonce"/>
    public virtual bool VerifyNonce(string challenge)
    {
        var result = _nonceCache.Contains(challenge);
        if (result) _nonceCache.Remove(challenge);

        return result;
    }

    /// <inheritdoc cref="IIdentityConsumer.BuildAuthenticationRedirectUri"/>
    public virtual async Task<Uri> BuildAuthenticationRedirectUri(IdentityAuthenticationRequest request)
    {
        if (_settings is null) await Initialize();

        // Build base redirect url.
        var url = _settings!.AuthorizationEndpoint.SetQueryParams(
            new
            {
                response_type = "code",
                client_id = request.ClientId,
                redirect_uri = request.RedirectUri,
                nonce = request.Nonce
            }
        );

        // Register nonce in the cache for future validation.
        if (request.Nonce is not null) _nonceCache.Add(request.Nonce);
        
        // Add optional query parameters.
        if (request.Scope is not null) url.SetQueryParam("scope", $"openid {request.Scope}");
        if (request.State is not null) url.SetQueryParam("state", request.State);

        // Build uri.
        return url.ToUri();
    }

    /// <inheritdoc cref="IIdentityConsumer.SendCodeExchangeRequest"/>
    public virtual async Task<IdentityCodeExchangeResponse?> SendCodeExchangeRequest(IdentityCodeExchangeRequest request)
    {
        if (_settings is null) await Initialize();

        try
        {
            // Send code exchange request with required parameters.
            var res = await _settings!.TokenEndpoint.PostUrlEncodedAsync(
                new
                {
                    grant_type = "authorization_code",
                    code = request.Code,
                    client_id = request.ClientId,
                    client_secret = request.ClientSecret,
                    redirect_uri = request.RedirectUri
                }
            );

            // Convert the response to a string.
            var content = await res.GetStringAsync();
            if (content is null)
                return null;
            
            // Serialize the content to a mapped object and generate an id token.
            var response = JsonConvert.DeserializeObject<OidcCodeExchangeResponse>(content);
            var idToken = new IdToken(response.IdToken);

            return new IdentityCodeExchangeResponse { IdToken = idToken };
        }
        catch (FlurlHttpException e)
        {
            var res = await e.Call.Response.GetStringAsync();
            Console.WriteLine(res);
            throw;
        }
    }

    /// <inheritdoc cref="IIdentityConsumer.VerifyIdToken"/>
    public async Task<bool> VerifyIdToken(IdToken token, IdentityIdTokenVerificationSettings settings)
    {
        if (_settings is null) await Initialize();

        // Verify token issuer is equal to the issuer in the discovery document.
        if (token.Payload.Issuer != _settings!.Issuer) return 
            false;
        
        // Verify the token has the current application in its audience.
        if (!token.Payload.Audience.Contains(settings.ClientId)) 
            return false;
        
        // Verify that we are the authorized party of the token.
        if (token.Payload.AuthorizedParty is not null && token.Payload.AuthorizedParty != settings.ClientId) 
            return false;
        
        // Verify that the token is not expired.
        if (token.Payload.Expiration < DateTimeOffset.UtcNow) 
            return false;
        
        // Verify that the nonce was a nonce which was registered before.
        if (token.Payload.Nonce is not null && !VerifyNonce(token.Payload.Nonce)) 
            return false;
        
        // Verify the signature of the id token.
        var key = _keys!.Keys.FirstOrDefault(k =>
            k.KeyId == token.Header.KeyId &&
            k.Use == "sig"
        );
        
        switch (key?.Type)
        {
            // We are only using RSA here because all the used identity providers only support RS256
            // hashing for their signature. So why bother implementing the rest? This isn't a fully-featured
            // OIDC client library.
            case "RSA":
                // Generate the parameters for the RSA service with the
                // values found in the key from the discovery document.
                var param = new RSAParameters
                {
                    Modulus = Base64Url.Decode(key.Modulus),
                    Exponent = Base64Url.Decode(key.Exponent)
                };

                var rsa = new RSACryptoServiceProvider();
                rsa.ImportParameters(param);

                // Hash the header and payload of the jwt token.
                var content = Encoding.UTF8.GetBytes($"{token.Raw.Header}.{token.Raw.Payload}");
                var hash = SHA256.HashData(content);
                
                // Create deformatter for verification of the hash and the signature.
                var rsaDeformatter = new RSAPKCS1SignatureDeformatter(rsa);
                rsaDeformatter.SetHashAlgorithm("SHA256");

                // Verify the actual signature.
                var signature = Base64Url.Decode(token.Raw.Signature);
                return rsaDeformatter.VerifySignature(hash, signature);
            default:
                throw new ApplicationException("The application only supports RSA signatures a.t.m.");
        }
    }
}