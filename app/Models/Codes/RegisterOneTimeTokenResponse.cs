namespace App.Models.Codes;

public class RegisterOneTimeTokenResponse
{
    public string Secret { get; set; }
    public string Uri { get; set; }
    

    public RegisterOneTimeTokenResponse(string secret, string uri)
    {
        Secret = secret;
        Uri = uri;
    }
}