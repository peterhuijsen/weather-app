namespace Identity.Consumer.Models.Passkeys.Registration;

public class ConfirmRegisterPasskeyResponse
{
    public bool Success { get; set; }
    public string? Reason { get; set; }

    public byte[]? CredentialId { get; set; }
    public byte[]? PublicKey { get; set; }

    public static ConfirmRegisterPasskeyResponse Failed(string reason) => new ConfirmRegisterPasskeyResponse(reason);
    public static ConfirmRegisterPasskeyResponse Succeeded(byte[] credentialId, byte[] publicKey) => new ConfirmRegisterPasskeyResponse(credentialId, publicKey);

    public ConfirmRegisterPasskeyResponse(string reason)
        => Reason = reason;
    
    public ConfirmRegisterPasskeyResponse(byte[] credentialId, byte[] publicKey)
    {
        Success = true;
        CredentialId = credentialId;
        PublicKey = publicKey;
    }
}