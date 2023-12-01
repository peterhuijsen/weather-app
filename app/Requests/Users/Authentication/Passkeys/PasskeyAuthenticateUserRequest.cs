using System.Text;
using App.Database.Repositories;
using App.Models;
using App.Models.Controllers.Users;
using App.Services;
using Identity.Consumer.Helpers;
using Identity.Consumer.Models.Passkeys.Authentication;
using Identity.Consumer.Services;
using MediatR;

namespace App.Requests.Users.Authentication.Passkeys;

public class PasskeyAuthenticateUserRequest : IRequest<LoginUserResponse?>
{
    public string State { get; set; }
    public ConfirmAuthenticatePasskeyRequest Request { get; set; }

    public PasskeyAuthenticateUserRequest(string state, ConfirmAuthenticatePasskeyRequest request)
    {
        State = state;
        Request = request;
    }
}

public class PasskeyAuthenticateUserRequestHandler : IRequestHandler<PasskeyAuthenticateUserRequest, LoginUserResponse?>
{
    private readonly IRepository<User> _repository;
    private readonly IAuthenticationService _authenticationService;
    private readonly IWebAuthnConsumer _consumer;

    public PasskeyAuthenticateUserRequestHandler(IRepository<User> repository, IAuthenticationService authenticationService, IWebAuthnConsumer consumer)
    {
        _repository = repository;
        _authenticationService = authenticationService;
        _consumer = consumer;
    }

    public async Task<LoginUserResponse?> Handle(PasskeyAuthenticateUserRequest request, CancellationToken cancellationToken)
    {
        var uuid = Encoding.UTF8.GetString(Base64Url.Decode(request.Request.Response.User));
        if (!Guid.TryParse(uuid, out var guid))
            throw new NullReferenceException("The given user of the passkey could not be found.");

        var user = await _repository.Get(guid);
        if (user is null)
            throw new NullReferenceException("The given user of the passkey could not be found.");

        var passkey = user.Credentials.Passkeys?.FirstOrDefault(p => p.Id.SequenceEqual(Base64Url.Decode(request.Request.Id)));
        if (passkey is null)
            throw new NullReferenceException("The given passkey of the user could not be found.");
        
        var res = _consumer.ConfirmPasskeyAuthentication(
            state: request.State,
            credentials: passkey,
            request: request.Request
        );

        if (!res.Success)
            throw new ApplicationException(res.Reason);
        
        var token = _authenticationService.Token(user, loa: 2);
        return new LoginUserResponse
        {
            Token = token,
            User = user
        };
    }
}