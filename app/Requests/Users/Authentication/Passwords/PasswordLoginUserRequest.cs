using System;
using System.Threading;
using System.Threading.Tasks;
using App.Database.Services;
using App.Models.Controllers.Users;
using App.Services;
using MediatR;

namespace App.Requests.Users.Authentication.Passwords;

/// <summary>
/// A request to login a given user by their password.
/// </summary>
public class PasswordLoginUserRequest : IRequest<LoginUserResponse>
{
    /// <summary>
    /// Gets the email adress of the user which should be logged in.
    /// </summary>
    public string Email { get; }

    /// <summary>
    /// Gets the password of the user which should be logged in.
    /// </summary>
    public string Password { get; }

    public PasswordLoginUserRequest(string email, string password)
        => (Email, Password) = (email, password);
}

/// <summary>
/// A handler for the <see cref="PasswordLoginUserRequest"/> to execute the login process of the given user.
/// </summary>
public class PasswordLoginUserRequestHandler : IRequestHandler<PasswordLoginUserRequest, LoginUserResponse>
{
    /// <summary>
    /// The current <see cref="IAuthenticationService"/>.
    /// </summary>
    private readonly IAuthenticationService _authentication;
    
    /// <summary>
    /// The current <see cref="IUserService"/>.
    /// </summary>
    private readonly IUserService _users;

    public PasswordLoginUserRequestHandler(IAuthenticationService authentication, IUserService users)
        => (_authentication, _users) = (authentication, users);

    /// <inheritdoc cref="IRequestHandler{TRequest,TResponse}.Handle"/>
    public async Task<LoginUserResponse> Handle(PasswordLoginUserRequest request, CancellationToken cancellationToken)
    {
        // Get the user
        var user = await _users.GetByEmail(request.Email);
        if (user is null)
            throw new NullReferenceException("The given item could not be updated because it does not exist.");

        if (user.Credentials.Hash is null)
            throw new NullReferenceException("The user has not registered their account via a password.");
        
        // Check whether the given password matches the stored hash
        var result = _authentication.Challenge(user.Credentials.Hash, request.Password);
        if (!result)
            throw new ApplicationException("The given password does not match the stored password for the given user.");

        // Generate an access token for the user
        var token = _authentication.Token(user, loa: 1);
        return new LoginUserResponse
        {
            Token = token,
            User = user
        };
    }
}