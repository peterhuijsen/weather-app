namespace Identity.Consumer.Models.Oidc.Authentication;

public class IdentityIdTokenVerificationSettings
{
    public string ClientId { get; set; }

    public IdentityIdTokenVerificationSettings(string clientId)
        => ClientId = clientId;
}