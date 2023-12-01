namespace Identity.Consumer.Models.Passkeys.Authentication;

public class ConfirmAuthenticatePasskeyResponse
{
    public bool Success { get; set; }
    public string? Reason { get; set; }

    public static ConfirmAuthenticatePasskeyResponse Failed(string reason) => new ConfirmAuthenticatePasskeyResponse(reason);
    public static ConfirmAuthenticatePasskeyResponse Succeeded() => new ConfirmAuthenticatePasskeyResponse();

    public ConfirmAuthenticatePasskeyResponse(string reason)
        => Reason = reason;
    
    public ConfirmAuthenticatePasskeyResponse()
    {
        Success = true;
    }
}