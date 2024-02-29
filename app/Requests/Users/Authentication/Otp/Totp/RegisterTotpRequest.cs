using System;
using System.Threading;
using System.Threading.Tasks;
using App.Database.Repositories;
using App.Models;
using App.Models.Codes;
using Identity.Consumer.Models.Code;
using Identity.Consumer.Services;
using MediatR;

namespace App.Requests.Users.Authentication.Otp.Totp;

public class RegisterTotpRequest : IRequest<RegisterOneTimeTokenResponse>
{
    public User? User { get; }
    
    public RegisterTotpRequest(User? user)
    {
        User = user;
    }
}

public class RegisterTotpRequestHandler : IRequestHandler<RegisterTotpRequest, RegisterOneTimeTokenResponse>
{
    private readonly IRepository<User> _repository;
    private readonly ICodeConsumer<TimeCode> _timeCodeConsumer;

    public RegisterTotpRequestHandler(IRepository<User> repository, ICodeConsumer<TimeCode> codeConsumer)
    {
        _repository = repository;
        _timeCodeConsumer = codeConsumer;
    }

    public async Task<RegisterOneTimeTokenResponse> Handle(RegisterTotpRequest request, CancellationToken cancellationToken)
    {
        if (request.User is null)
            throw new NullReferenceException("Could not send a one time code to a non-existent user.");
        
        var secret = _timeCodeConsumer.GenerateSecret();
        
        request.User.Credentials.OTP.TimeCodes.Add(new TimeOneTimeCredential { Secret = secret });
        await _repository.Update(request.User);
        
        var uri = _timeCodeConsumer.GenerateUri(secret, request.User.Email);
        return new RegisterOneTimeTokenResponse(
            secret: secret, 
            uri: uri
        );
    }
}