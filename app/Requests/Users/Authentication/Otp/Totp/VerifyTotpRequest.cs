using App.Models;
using App.Models.Codes;
using Identity.Consumer.Models.Code;
using Identity.Consumer.Services;
using MediatR;

namespace App.Requests.Users.Authentication.Otp.Totp;

public class VerifyTotpRequest : IRequest<VerifyOneTimeTokenResponse>
{
    public User? User { get; }
    public string Token { get; }

    public VerifyTotpRequest(User? user, string token)
    {
        User = user;
        Token = token;
    }
}

public class VerifyTotpRequestHandler : IRequestHandler<VerifyTotpRequest, VerifyOneTimeTokenResponse>
{
    private readonly ICodeConsumer<TimeCode> _timeCodeConsumer;

    public VerifyTotpRequestHandler(ICodeConsumer<TimeCode> codeConsumer)
    {
        _timeCodeConsumer = codeConsumer;
    }
    
    public Task<VerifyOneTimeTokenResponse> Handle(VerifyTotpRequest request, CancellationToken cancellationToken)
    {
        if (request.User is null)
            throw new NullReferenceException("Could not verify a one time code to a non-existent user.");

        foreach (var timeCode in request.User.Credentials.OTP.TimeCodes)
        {
            var result = _timeCodeConsumer.VerifyOneTimeCode(
                secret: timeCode.Secret,
                code: new TimeCode(
                    value: request.Token,
                    timestamp: DateTime.UtcNow
                )
            );

            if (!result)
                continue;
            
            return Task.FromResult(
                new VerifyOneTimeTokenResponse(
                    verified: true
                )
            );
        }

        return Task.FromResult(
            new VerifyOneTimeTokenResponse(
                verified: false
            )
        );
    }
}