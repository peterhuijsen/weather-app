using System.Net.Mail;
using System.Text;
using Identity.Consumer.Models.Code;

namespace Identity.Consumer.Services;

public interface ICodeConsumer<in TCode>
    where TCode : IOneTimeCode
{
    Task<bool> SendEmailVerificationCode(string code, string target);

    string GenerateUri(string secret, string email, long counter = 0);

    string GenerateSecret();

    string GenerateOneTimeCode(string secret, TCode context);
    
    bool VerifyOneTimeCode(string secret, TCode code);
}

public abstract class CodeConsumer<TCode> : ICodeConsumer<TCode> 
    where TCode : IOneTimeCode
{
    /// <inheritdoc cref="ICodeConsumer{TCode}.SendEmailVerificationCode"/>
    public async Task<bool> SendEmailVerificationCode(string code, string target)
    {
        var client = new SmtpClient
        {
            EnableSsl = true
        };

        var from = new MailAddress(
            address: "noreply@thunor.com", 
            displayName: "Thunor", 
            displayNameEncoding: Encoding.UTF8
        );
        
        var to = new MailAddress(target);

        var message = new MailMessage(from, to)
        {
            Subject = "Your identity verification code",
            Body = $"""
                    Hey {target}!
                    
                    You have requested a verification code for your account. 
                    Please use the following code:
                    
                    {code}
                    
                    Thanks,
                    Thunor
                    """
        };

        try
        {
            await client.SendMailAsync(message);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <inheritdoc cref="ICodeConsumer{TCode}.GenerateSecret"/>
    public abstract string GenerateSecret();

    /// <inheritdoc cref="ICodeConsumer{TCode}.GenerateUri"/>
    public abstract string GenerateUri(string secret, string email, long counter = 0);

    /// <inheritdoc cref="ICodeConsumer{TCode}.GenerateOneTimeCode"/>
    public abstract string GenerateOneTimeCode(string secret, TCode context);

    /// <inheritdoc cref="ICodeConsumer{TCode}.VerifyOneTimeCode"/>
    public abstract bool VerifyOneTimeCode(string secret, TCode code);
}