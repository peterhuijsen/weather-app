using App.Database.Repositories;
using App.Models;
using App.Models.Codes;
using Identity.Consumer.Models.Code;
using Identity.Consumer.Services;
using MediatR;
using HashCode = Identity.Consumer.Models.Code.HashCode;

namespace App.Requests.Users.Authentication.Otp.Hotp;

public class RegisterHotpRequest : IRequest<RegisterOneTimeTokenResponse>
{
    public User? User { get; }
    
    public RegisterHotpRequest(User? user)
    {
        User = user;
    }
}

public class RegisterHotpRequestHandler : IRequestHandler<RegisterHotpRequest, RegisterOneTimeTokenResponse>
{
    private readonly IRepository<User> _repository;
    private readonly ICodeConsumer<HashCode> _hashCodeConsumer;

    public RegisterHotpRequestHandler(IRepository<User> repository, ICodeConsumer<HashCode> codeConsumer)
    {
        _repository = repository;
        _hashCodeConsumer = codeConsumer;
    }

    public async Task<RegisterOneTimeTokenResponse> Handle(RegisterHotpRequest request, CancellationToken cancellationToken)
    {
        if (request.User is null)
            throw new NullReferenceException("Could not send a one time code to a non-existent user.");
        
        var secret = _hashCodeConsumer.GenerateSecret();
        
        request.User.Credentials.OTP.HashCodes.Add(new HashOneTimeCredential { Secret = secret });
        await _repository.Update(request.User);
        
        var uri = _hashCodeConsumer.GenerateUri(secret, request.User.Email);
        return new RegisterOneTimeTokenResponse(
            secret: secret, 
            uri: uri
        );
    }
}