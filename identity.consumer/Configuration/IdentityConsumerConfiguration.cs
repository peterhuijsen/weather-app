namespace Identity.Consumer.Configuration;

public class IdentityConsumerConfiguration
{
    public OneTimeCodeConfiguration Otp { get; set; } = new OneTimeCodeConfiguration();
    public OidcConfiguration Oidc { get; set; } = new OidcConfiguration();
    public WebAuthnConfiguration WebAuthn { get; set; } = new WebAuthnConfiguration();
}

public class OneTimeCodeConfiguration
{
    public int Window { get; set; }
    public int Digits { get; set; }
    public int Period { get; set; }

    public string Issuer { get; set; } = null!;
}

public class OidcConfiguration
{
    public string Google { get; set; } = null!;
    public string Microsoft { get; set; } = null!;
}

public class WebAuthnConfiguration
{
    public string Id { get; set; } = null!;
    public string Origin { get; set; } = null!;
}