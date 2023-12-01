using App.Database.Repositories;
using App.Models;
using Identity.Consumer.Models;
using Identity.Consumer.Models.Passkeys.Registration;
using Identity.Consumer.Services;
using MediatR;

namespace App.Requests.Users.Authentication.Passkeys;

public class PasskeyConfirmRegistrationRequest : IRequest
{
    public Guid Uuid { get; set; }

    public ConfirmRegisterPasskeyRequest Request { get; set; }

    public PasskeyConfirmRegistrationRequest(Guid uuid, ConfirmRegisterPasskeyRequest request)
    {
        Uuid = uuid;
        Request = request;
    }
}

public class PasskeyConfirmRegistrationRequestHandler : IRequestHandler<PasskeyConfirmRegistrationRequest>
{
    private readonly IRepository<User> _repository;
    private readonly IWebAuthnConsumer _consumer;

    public PasskeyConfirmRegistrationRequestHandler(IRepository<User> repository, IWebAuthnConsumer consumer)
    {
        _repository = repository;
        _consumer = consumer;
    }

    public async Task Handle(PasskeyConfirmRegistrationRequest request, CancellationToken cancellationToken)
    {
        var confirmation = _consumer.ConfirmPasskeyRegistration(request.Uuid, request.Request);
        if (!confirmation.Success)
            throw new ApplicationException(confirmation.Reason);

        var user = await _repository.Get(request.Uuid);
        if (user is null)
            throw new NullReferenceException("The given user could not be found for passkey registration.");

        var passkey = new PasskeyCredentials(confirmation.CredentialId!, confirmation.PublicKey!);
        user.Credentials.Passkeys!.Add(passkey);
        
        await _repository.Update(user);
    }
}