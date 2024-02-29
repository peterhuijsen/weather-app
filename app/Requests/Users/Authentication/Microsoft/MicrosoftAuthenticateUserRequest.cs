using System;
using System.Threading;
using System.Threading.Tasks;
using App.Database.Repositories;
using App.Database.Services;
using App.Models;
using App.Models.Controllers.Users;
using App.Services;
using Identity.Consumer.Models.Tokens;
using MediatR;

namespace App.Requests.Users.Authentication.Microsoft;

public class MicrosoftAuthenticateUserRequest : IRequest<LoginUserResponse?>
{
    public IdToken IdToken { get; set; }

    public MicrosoftAuthenticateUserRequest(IdToken idToken)
        => IdToken = idToken;
}

public class MicrosoftAuthenticateUserRequestHandler : IRequestHandler<MicrosoftAuthenticateUserRequest, LoginUserResponse?>
{
    private readonly IRepository<User> _repository;
    private readonly IUserService _userService;
    private readonly IAuthenticationService _authenticationService;

    public MicrosoftAuthenticateUserRequestHandler(IRepository<User> repository, IUserService userService, IAuthenticationService authenticationService)
    {
        _repository = repository;
        _userService = userService;
        _authenticationService = authenticationService;
    }

    public async Task<LoginUserResponse?> Handle(MicrosoftAuthenticateUserRequest request, CancellationToken cancellationToken)
    {
        if (request.IdToken.Payload.Email is null)
            throw new ApplicationException("The given id token does not include an email adress, which is required.");

        var user = await _userService.GetByEmail(request.IdToken.Payload.Email);
        if (user is null)
            throw new NullReferenceException();

        if (user.Credentials.Microsoft is null)
        {
            user.Credentials.Microsoft = request.IdToken.Payload.Subject;
            await _repository.Update(user);
        } 
        else if (user.Credentials.Microsoft != request.IdToken.Payload.Subject)
            throw new UnauthorizedAccessException("The registered microsoft account to this user does not equal the given microsoft account.");

        var token = _authenticationService.Token(user, loa: 1);
        return new LoginUserResponse
        {
            Token = token,
            User = user
        };
    }
}