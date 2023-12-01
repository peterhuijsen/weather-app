using App.Database.Repositories;
using App.Models;
using MediatR;

namespace App.Requests.Users;

public class DeleteUserRequest : IRequest<bool>
{
    public Guid Uuid { get; set; }

    public DeleteUserRequest(Guid uuid)
        => Uuid = uuid;
}

public class DeleteUserRequestHandler : IRequestHandler<DeleteUserRequest, bool>
{
    private readonly IRepository<User> _repository;

    public DeleteUserRequestHandler(IRepository<User> repository)
        => _repository = repository;

    public async Task<bool> Handle(DeleteUserRequest request, CancellationToken cancellationToken)
    {
        try
        {
            await _repository.Delete(request.Uuid);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}