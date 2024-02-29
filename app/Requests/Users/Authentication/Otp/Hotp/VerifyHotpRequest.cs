using System;
using System.Threading;
using System.Threading.Tasks;
using App.Database.Repositories;
using App.Models;
using App.Models.Codes;
using App.Settings;
using Identity.Consumer.Configuration;
using Identity.Consumer.Services;
using MediatR;
using Microsoft.Extensions.Options;
using HashCode = Identity.Consumer.Models.Code.HashCode;

namespace App.Requests.Users.Authentication.Otp.Hotp;

public class VerifyHotpRequest : IRequest<VerifyOneTimeTokenResponse>
{
    public User? User { get; }
    public string Token { get; }

    public VerifyHotpRequest(User? user, string token)
    {
        User = user;
        Token = token;
    }
}

public class VerifyHotpRequestHandler : IRequestHandler<VerifyHotpRequest, VerifyOneTimeTokenResponse>
{
    private readonly IdentityConsumerConfiguration _configuration;
    
    private readonly IRepository<User> _repository;
    private readonly ICodeConsumer<HashCode> _hashCodeConsumer;

    public VerifyHotpRequestHandler(IOptions<IdentityConsumerConfiguration> configuration, IRepository<User> repository, ICodeConsumer<HashCode> codeConsumer)
    {
        _configuration = configuration.Value;
        _repository = repository;
        _hashCodeConsumer = codeConsumer;
    }
    
    public async Task<VerifyOneTimeTokenResponse> Handle(VerifyHotpRequest request, CancellationToken cancellationToken)
    {
        if (request.User is null)
            throw new NullReferenceException("Could not verify a one time code to a non-existent user.");

        foreach (var hashCode in request.User.Credentials.OTP.HashCodes)
        {
            for (var i = hashCode.Counter; i < _configuration.Otp.Window; i++)
            {
                var result = _hashCodeConsumer.VerifyOneTimeCode(
                    secret: hashCode.Secret,
                    code: new HashCode(
                        value: request.Token,
                        counter: i
                    )
                );

                if (!result)
                    continue;

                hashCode.Counter = i + 1;
                await _repository.Update(request.User);
                    
                return new VerifyOneTimeTokenResponse(verified: true);
            }
        }

        return new VerifyOneTimeTokenResponse(verified: false);
    }
}