namespace App.Models.Codes;

public class VerifyOneTimeTokenResponse
{
    public bool Verified { get; set; }

    public VerifyOneTimeTokenResponse(bool verified)
    {
        Verified = verified;
    }
}