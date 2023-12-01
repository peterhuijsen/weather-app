using System.Security.Cryptography;
using System.Text;
using Dahomey.Cbor;
using Dahomey.Cbor.ObjectModel;
using Identity.Consumer.Helpers;
using Identity.Consumer.Models;
using Identity.Consumer.Models.Passkeys;
using Identity.Consumer.Models.Passkeys.Attestation;
using Identity.Consumer.Models.Passkeys.Authentication;
using Identity.Consumer.Models.Passkeys.Cbor;
using Identity.Consumer.Models.Passkeys.Registration;
using Newtonsoft.Json;

namespace Identity.Consumer.Services;

public interface IWebAuthnConsumer
{
    /// <summary>
    /// Generate a configuration file for a client to create a passkey.
    /// </summary>
    /// <param name="settings">The settings from which the configuration file can be generated.</param>
    /// <returns>The configuration file for the client.</returns>
    RegisterPasskeyConfiguration GeneratePasskeyRegistrationConfiguration(GenerateAttestationPasskeySettings settings);

    /// <summary>
    /// Generate a configuration file for a client to authenticatie with a passkey.
    /// </summary>
    /// <param name="settings">The settings from which the configuration file can be generated.</param>
    /// <returns>The configuration file for the client.</returns>
    AuthenticatePasskeyConfiguration GeneratePasskeyAuthenticationConfiguration(GenerateAssertionPasskeySettings settings);

    /// <summary>
    /// Confirm the received passkey attestation to guarantee the integriteit and authenticity
    /// of the received attestation.
    /// </summary>
    /// <param name="uuid">The uuid of the user to which the passkey should be registered.</param>
    /// <param name="request">The attestation request which should be confirmed.</param>
    /// <returns>A response containing whether the request was verified and the public key data of the request.</returns>
    ConfirmRegisterPasskeyResponse ConfirmPasskeyRegistration(Guid uuid, ConfirmRegisterPasskeyRequest request);
    
    /// <summary>
    /// Confirm the received assertion to authenticatie and verify the given passkey and the given user.
    /// </summary>
    /// <param name="state">The state of the current session in which the challenge was also generated.</param>
    /// <param name="credentials">The credentials of the user of the given assertion.</param>
    /// <param name="request">The request settings for the verification of the assertion.</param>
    /// <returns>A response containing whether the request was verified.</returns>
    ConfirmAuthenticatePasskeyResponse ConfirmPasskeyAuthentication(string state, PasskeyCredentials credentials, ConfirmAuthenticatePasskeyRequest request);
}

public class WebAuthnConsumer : IWebAuthnConsumer
{
    private readonly Dictionary<Guid, List<string>> _registrationChallengeCache = new Dictionary<Guid, List<string>>();
    private readonly Dictionary<string, List<string>> _authenticationChallengeCache = new Dictionary<string, List<string>>();

    private readonly string _id;
    private readonly string _origin;
    
    protected WebAuthnConsumer(string id, string origin)
    {
        _id = id;
        _origin = origin;
    }

    /// <inheritdoc cref="IWebAuthnConsumer.GeneratePasskeyRegistrationConfiguration"/>
    public RegisterPasskeyConfiguration GeneratePasskeyRegistrationConfiguration(GenerateAttestationPasskeySettings settings)
    {
        var response = new RegisterPasskeyConfiguration(
            challenge: GenerateChallenge(),
            user: new RegisterPasskeyResponseUser(
                id: settings.Uuid.ToString("D"),
                name: settings.Name,
                displayName: settings.DisplayName
            )
        );
        
        if (_registrationChallengeCache.TryGetValue(settings.Uuid, out var cache)) cache.Add(response.Challenge);
        else _registrationChallengeCache.Add(settings.Uuid, new List<string> { response.Challenge });

        return response;
    }

    /// <inheritdoc cref="IWebAuthnConsumer.GeneratePasskeyAuthenticationConfiguration"/>
    public AuthenticatePasskeyConfiguration GeneratePasskeyAuthenticationConfiguration(GenerateAssertionPasskeySettings settings)
    {
        var response = new AuthenticatePasskeyConfiguration(
            challenge: GenerateChallenge()
        );
        
        if (_authenticationChallengeCache.TryGetValue(settings.State, out var cache)) cache.Add(response.Challenge);
        else _authenticationChallengeCache.Add(settings.State, new List<string> { response.Challenge });

        return response;
    }
    
    /// <inheritdoc cref="IWebAuthnConsumer.ConfirmPasskeyRegistration"/>
    public ConfirmRegisterPasskeyResponse ConfirmPasskeyRegistration(Guid uuid, ConfirmRegisterPasskeyRequest request)
    {
        var clientData = ParseClientData(request.Response.ClientDataJson);

        // Check the type of the client request, make sure it is trying to attest.
        if (clientData.Type != "webauthn.create")
            return ConfirmRegisterPasskeyResponse.Failed("The given WebAuthn client data is of an invalid type.");
        
        if (!_registrationChallengeCache.ContainsKey(uuid) || !_registrationChallengeCache[uuid].Contains(clientData.Challenge))
            return ConfirmRegisterPasskeyResponse.Failed("The given WebAuthn client data contains an invalid challenge.");

        if (_registrationChallengeCache[uuid].Count > 0) _registrationChallengeCache[uuid].Remove(clientData.Challenge);
        else _registrationChallengeCache.Remove(uuid);

        if (clientData.Origin != _origin)
            return ConfirmRegisterPasskeyResponse.Failed("The given WebAuthn client data origin is invalid.");

        var attestation = Cbor.Deserialize<PasskeyAttestationData>(Convert.FromBase64String(request.Response.AttestationObject));
        var authData = attestation.Data;

        var trueRpId = SHA256.HashData(Encoding.UTF8.GetBytes(_id));
        var rpId = authData[..32];

        if (!rpId.SequenceEqual(trueRpId))
            return ConfirmRegisterPasskeyResponse.Failed("No relying party id could be found in the attestation data.");

        var flag = authData[32];
        if ((flag & 1) == 0)
            return ConfirmRegisterPasskeyResponse.Failed("No UP flag was found in the attestation data, while being required.");

        if (((flag >> 2) & 1) == 0 && ((flag >> 3) & 1) == 0)
            return ConfirmRegisterPasskeyResponse.Failed("No BE or BS flag was found in the attestation data, while one is required.");

        var credentialIdLengthBytes = authData[53..55].Reverse().ToArray();
        var credentialIdLength = BitConverter.ToInt16(credentialIdLengthBytes, 0);
        
        if (credentialIdLength > 1023)
            return ConfirmRegisterPasskeyResponse.Failed("The length of the credential id was invalid, it was too long.");
        
        var credentialId = authData[55..(55 + credentialIdLength)];
        var publicKey = authData[(55 + credentialIdLength)..];

        var hash = SHA256.HashData(Base64Url.Decode(request.Response.ClientDataJson));
        var key = new PasskeyPublicKeyData(
            Cbor.Deserialize<CborObject>(publicKey)
        );
        
        switch (attestation.Format)
        {
            case "none":
                break;
            case "packed":
                try
                {
                    var packedAttestationStatement = attestation.Statement.ToObject<PackedAttestationStatement>();
                    if (key.Algorithm != packedAttestationStatement.Algorithm)
                        return ConfirmRegisterPasskeyResponse.Failed("The given packed attestation algorithm does not match the algorithm of the public key.");
                    
                    var (isValidSignature, error) = VerifySignature(
                        key: key, 
                        signature: packedAttestationStatement.Signature, 
                        authData: authData, 
                        hashedClientData: hash
                    );
                    
                    if (!isValidSignature)
                        return ConfirmRegisterPasskeyResponse.Failed(error!);
                }
                catch (Exception)
                {
                    return ConfirmRegisterPasskeyResponse.Failed("The given packed attestation is of an invalid format.");
                }
                
                break;
        }
        
        return ConfirmRegisterPasskeyResponse.Succeeded(
            credentialId,
            publicKey
        );
    }
    
    /// <inheritdoc cref="IWebAuthnConsumer.ConfirmPasskeyAuthentication"/>
    public ConfirmAuthenticatePasskeyResponse ConfirmPasskeyAuthentication(string state, PasskeyCredentials credentials, ConfirmAuthenticatePasskeyRequest request)
    {
        var clientData = ParseClientData(request.Response.ClientDataJson);
        
        // Check the type of the client request, make sure it is trying to assert.
        if (clientData.Type != "webauthn.get")
            return ConfirmAuthenticatePasskeyResponse.Failed("The given WebAuthn client data is of an invalid type.");
        
        // Check whether the client has generated a challenge in the given state session.
        if (!_authenticationChallengeCache.ContainsKey(state) || !_authenticationChallengeCache[state].Contains(clientData.Challenge))
            return ConfirmAuthenticatePasskeyResponse.Failed("The given WebAuthn client data contains an invalid challenge.");
       
        if (_authenticationChallengeCache[state].Count > 0) _authenticationChallengeCache[state].Remove(clientData.Challenge);
        else _authenticationChallengeCache.Remove(state);
        
        // Check whether the origin of the client is from the client application.
        if (clientData.Origin != _origin)
            return ConfirmAuthenticatePasskeyResponse.Failed("The given WebAuthn client data origin is invalid.");
        
        var authData = Base64Url.Decode(request.Response.AuthenticatorData);
        var trueRpId = SHA256.HashData(Encoding.UTF8.GetBytes(_id));
        var rpId = authData[..32];

        // Check whether the relying pary id is correct.
        if (!rpId.SequenceEqual(trueRpId))
            return ConfirmAuthenticatePasskeyResponse.Failed("No relying party id could be found in the assertion data.");
        
        var flag = authData[32];
        
        // Check whether the first bit, the User Present flag, is present.
        if ((flag & 1) == 0)
            return ConfirmAuthenticatePasskeyResponse.Failed("No UP flag was found in the assertion data, while being required.");
        
        var hash = SHA256.HashData(Base64Url.Decode(request.Response.ClientDataJson));
        var key = new PasskeyPublicKeyData(
            Cbor.Deserialize<CborObject>(credentials.PublicKey)
        );

        // Check whether the signature of the data is valid.
        var (isValidSignature, error) = VerifySignature(
            key: key, 
            signature: Base64Url.Decode(request.Response.Signature), 
            authData: authData, 
            hashedClientData: hash
        );
        
        return isValidSignature
            ? ConfirmAuthenticatePasskeyResponse.Succeeded()
            : ConfirmAuthenticatePasskeyResponse.Failed(error!);
    }
    
    /// <summary>
    /// Generate a new challengen token for the client to sign with their private key.
    /// </summary>
    /// <returns>The generated challenge token for the client.</returns>
    private string GenerateChallenge()
        => Base64Url.Encode(RandomNumberGenerator.GetBytes(256));
    
    /// <summary>
    /// Parse a Base64Url encoded JSON client data object form the authenticator.
    /// </summary>
    /// <param name="encodedData">The string with the encoded client data.</param>
    /// <returns>An object representing the client data from the authenticator.</returns>
    /// <exception cref="NullReferenceException">Throw when the client data does not match the expected client data model.</exception>
    private PasskeyClientData ParseClientData(string encodedData)
    {
        var clientDataJson = Encoding.UTF8.GetString(Base64Url.Decode(encodedData));
        var clientData = JsonConvert.DeserializeObject<PasskeyClientData>(clientDataJson);
        if (clientData is null)
            throw new NullReferenceException();

        return clientData;
    }

    /// <summary>
    /// Verify the signature of an authenticator message by a given public key.
    /// </summary>
    /// <param name="key">The public key containing data needed to verify the integrity and authenticity of the signature.</param>
    /// <param name="signature">The actual bytes of the decoded signature.</param>
    /// <param name="authData">The bytes of the decoded authenticator data.</param>
    /// <param name="hashedClientData">The hash of the given client data</param>
    /// <returns>A tuple containing a. whether the signature is valid, and b. an error message if it was not valid.</returns>
    private (bool, string?) VerifySignature(PasskeyPublicKeyData key, byte[] signature, byte[] authData, byte[] hashedClientData)
    {
        if (key.Type != 2 || key.Curve != 1)
            return (false, "The given curve or type of the public key is unsupported.");
                    
        using var ecDsa = ECDsa.Create(new ECParameters
        {
            Curve = ECCurve.NamedCurves.nistP256,
            Q = new ECPoint
            {
                X = key.X, 
                Y = key.Y
            }
        });

        var data = authData.Concat(hashedClientData).ToArray();
        var rsSignature = ASN1.ConvertToRS(signature);
                    
        var verified = ecDsa.VerifyData(data, rsSignature, HashAlgorithmName.SHA256);
        return verified
            ? (true, null)
            : (false, "The given signature does not match the given public key.");
    }
}