using System;
using System.Threading;
using System.Threading.Tasks;
using App.Database.Repositories;
using App.Database.Services;
using App.Models;
using App.Services;
using FluentValidation;
using Identity.Consumer.Models;
using Identity.Consumer.Models.Oidc;
using Identity.Consumer.Models.Tokens;
using MediatR;

namespace App.Requests.Users;

public class CreateUserRequest : IRequest<User>
{
    public string Username { get; }

    public string Email { get; }
    
    public string? Password { get; }

    public OidcProviderType? ProviderType { get; }
    public string? ProviderId { get; }

    public CreateUserRequest(string username, string email, string password)
        => (Username, Email, Password) = (username, email, password);
    
    public CreateUserRequest(IdToken idToken, OidcProviderType providerType)
        => (Username, Email, ProviderType, ProviderId) = (
            idToken.Payload.Name ?? idToken.Payload.PreferredName ?? idToken.Payload.Subject, 
            idToken.Payload.Email ?? throw new NullReferenceException("The given email address was null."),
            providerType,
            idToken.Payload.Subject);
}

public class CreateUserRequestHandler : IRequestHandler<CreateUserRequest, User>
{
    private readonly IUserService _service;
    private readonly IValidator<User> _validator;
    private readonly IRepository<User> _repository;
    private readonly IAuthenticationService _authenticationService;
    
    public CreateUserRequestHandler(IUserService service, IValidator<User> validator, IRepository<User> repository, IAuthenticationService authenticationService)
        => (_service, _validator, _repository, _authenticationService) = (service, validator, repository, authenticationService);
    
    public async Task<User> Handle(CreateUserRequest request, CancellationToken cancellationToken)
    {
        if (request.Password is not null)
        {
            var match = await _service.GetByEmail(request.Email);
            if (match is not null)
                throw new ValidationException("Unable to create account for user which already exists.");
        }
        
        var credentials = request.Password is not null
            ? new Credentials(_authenticationService.Hash(request.Password))
            : request.ProviderType is not null && request.ProviderId is not null
                ? new Credentials(id: request.ProviderId, providerType: (OidcProviderType)request.ProviderType)
                : throw new ApplicationException("No account could be created without a provider ID or a password.");
        
        var item = new User
        {
            Email = request.Email, 
            Username = request.Username, 
            Credentials = credentials
        };

        var result = await _validator.ValidateAsync(item, cancellationToken);
        if (!result.IsValid)
            throw new ValidationException(result.ToString());
        
        var uuid = await _repository.Create(item);

        item.Uuid = uuid;
        return item;
    }
}