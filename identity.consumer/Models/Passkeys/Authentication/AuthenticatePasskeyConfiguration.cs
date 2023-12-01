namespace Identity.Consumer.Models.Passkeys.Authentication;

public class AuthenticatePasskeyConfiguration
{
    public string Challenge { get; set; }

    public AuthenticatePasskeyConfiguration(string challenge)
    {
        Challenge = challenge;
    }
}