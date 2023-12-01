using App.Database.Repositories;
using App.Database.Services;
using App.Models;
using App.Models.Controllers.Users;
using App.Services;
using Identity.Consumer.Models.Tokens;
using MediatR;

namespace App.Requests.Users.Authentication.Google;

public class GoogleAuthenticateUserRequest : IRequest<LoginUserResponse?>
{
    public IdToken IdToken { get; set; }

    public GoogleAuthenticateUserRequest(IdToken idToken)
        => IdToken = idToken;
}

public class GoogleAuthenticateUserRequestHandler : IRequestHandler<GoogleAuthenticateUserRequest, LoginUserResponse?>
{
    private readonly IRepository<User> _repository;
    private readonly IUserService _userService;
    private readonly IAuthenticationService _authenticationService;

    public GoogleAuthenticateUserRequestHandler(IRepository<User> repository, IUserService userService, IAuthenticationService authenticationService)
    {
        _repository = repository;
        _userService = userService;
        _authenticationService = authenticationService;
    }

    public async Task<LoginUserResponse?> Handle(GoogleAuthenticateUserRequest request, CancellationToken cancellationToken)
    {
        if (request.IdToken.Payload.Email is null)
            throw new ApplicationException("The given id token does not include an email adress, which is required.");

        var user = await _userService.GetByEmail(request.IdToken.Payload.Email);
        if (user is null)
            throw new NullReferenceException();

        if (user.Credentials.Google is null)
        {
            user.Credentials.Google = request.IdToken.Payload.Subject;
            await _repository.Update(user);
        } 
        else if (user.Credentials.Google != request.IdToken.Payload.Subject)
            throw new UnauthorizedAccessException("The registered google account to this user does not equal the given google account.");

        var token = _authenticationService.Token(user, loa: 1);
        return new LoginUserResponse
        {
            Token = token,
            User = user
        };
    }
}