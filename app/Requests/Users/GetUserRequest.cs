using System;
using System.Threading;
using System.Threading.Tasks;
using App.Database.Repositories;
using App.Database.Services;
using App.Models;
using MediatR;

namespace App.Requests.Users;

/// <summary>
/// A request to retrieve a user from the database.
/// </summary>
public class GetUserRequest : IRequest<User?>
{
    /// <summary>
    /// Gets the uuid of the user which should be retrieved.
    /// </summary>
    public Guid? Uuid { get; }
    
    /// <summary>
    /// Gets the uuid of the user which should be retrieved.
    /// </summary>
    public string? Email { get; }

    public GetUserRequest(Guid uuid)
        => Uuid = uuid;

    public GetUserRequest(string email)
        => Email = email;
}

/// <summary>
/// A handler used to handle <see cref="GetUserRequest"/> objects.
/// </summary>
public class GetUserRequestHandler : IRequestHandler<GetUserRequest, User?>
{
    private readonly IRepository<User> _repository;
    private readonly IUserService _service;
    
    public GetUserRequestHandler(IRepository<User> repository, IUserService service)
        => (_repository, _service) = (repository, service);

    public async Task<User?> Handle(GetUserRequest request, CancellationToken cancellationToken)
        => request.Uuid is null 
            ? await _service.GetByEmail(request.Email!) 
            : await _repository.Get(request.Uuid);
}