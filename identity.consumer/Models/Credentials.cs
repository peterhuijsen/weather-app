using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Identity.Consumer.Models.Code;
using Identity.Consumer.Models.Oidc;

namespace Identity.Consumer.Models;

/// <summary>
/// A class used to represent the different ways a user could have created an account
/// in the application.
/// </summary>
[Table("Credentials")]
public class Credentials
{
    /// <summary>
    /// The unique id of the credentials, to make the credentials unique even if
    /// the Google id is identical.
    /// </summary>
    [Key]
    [JsonIgnore]
    public Guid Uuid { get; set; }

    /// <summary>
    /// Whether the user has multi-factor authentication enabled.
    /// </summary>
    public bool MFA { get; set; }
    
    /// <summary>
    /// The hashed password of the user who created an account. The first part of the
    /// string will be the salt used for the hash, and the second part will be the
    /// actual hashed password.
    /// </summary>
    public string? Hash { get; set; }

    /// <summary>
    /// The credentials of 
    /// </summary>
    public OneTimeCredential OTP { get; set; } = new OneTimeCredential();

    /// <summary>
    /// The public key credentials of the passkey with wihch the account is registered.
    /// </summary>
    public List<PasskeyCredentials>? Passkeys { get; set; } = new List<PasskeyCredentials>();
    
    /// <summary>
    /// The ID of the Google user who created an account in the application via OIDC.
    /// </summary>
    public string? Google { get; set; }
    
    /// <summary>
    /// The ID of the Microsoft user who created an account in the application via OIDC.
    /// </summary>
    public string? Microsoft { get; set; }

    public Credentials(string hash)
        => Hash = hash;

    public Credentials(byte[] id, byte[] key)
        => Passkeys.Add(new PasskeyCredentials(id, key));

    public Credentials() { }

    public Credentials(string id, OidcProviderType providerType)
    {
        switch (providerType)
        {
            case OidcProviderType.Google:
                Google = id;
                break;
            case OidcProviderType.Microsoft:
                Microsoft = id;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(providerType), providerType, null);
        }
    }
}

public class PasskeyCredentials
{
    public byte[] Id { get; set; }
    public byte[]? PublicKey { get; set; }

    public PasskeyCredentials(byte[] id, byte[] publicKey)
    {
        Id = id;
        PublicKey = publicKey;
    }
}