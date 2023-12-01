using App.Database.Repositories;
using App.Models;
using FluentValidation;
using MediatR;

namespace App.Requests.Users;

public class UpdateUserRequest : IRequest<User>
{
    public Guid Uuid { get; set; }

    public string? Username { get; set; }
    
    public bool? MFA { get; set; }

    public UpdateUserRequest(Guid uuid, string? username = null, bool? mfa = null)
        => (Uuid, Username, MFA) = (uuid, username, mfa);
}

public class UpdateUserRequestHandler : IRequestHandler<UpdateUserRequest, User>
{
    private readonly IValidator<User> _validator;
    private readonly IRepository<User> _repository;

    public UpdateUserRequestHandler(IValidator<User> validator, IRepository<User> repository)
        => (_validator, _repository) = (validator, repository);

    public async Task<User> Handle(UpdateUserRequest request, CancellationToken cancellationToken)
    {
        var user = await _repository.Get(request.Uuid);
        if (user is null)
            throw new NullReferenceException("The given item could not be updated because it does not exist.");
        
        if (request.Username is not null)
            user.Username = request.Username;
        
        if (request.MFA is not null)
            user.Credentials.MFA = request.MFA ?? user.Credentials.MFA;
        
        var result = await _validator.ValidateAsync(user, cancellationToken);
        if (!result.IsValid)
            throw new ValidationException(result.ToString());

        await _repository.Update(user);

        return user;
    }
}