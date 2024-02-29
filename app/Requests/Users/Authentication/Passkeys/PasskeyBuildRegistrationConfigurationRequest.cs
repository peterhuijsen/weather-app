using System;
using System.Threading;
using System.Threading.Tasks;
using App.Database.Repositories;
using App.Models;
using Identity.Consumer.Models.Passkeys.Registration;
using Identity.Consumer.Services;
using MediatR;

namespace App.Requests.Users.Authentication.Passkeys;

public class PasskeyBuildRegistrationConfigurationRequest : IRequest<RegisterPasskeyConfiguration>
{
    public Guid Uuid { get; set; }

    public PasskeyBuildRegistrationConfigurationRequest(Guid uuid)
        => Uuid = uuid;
}

public class PasskeyBuildRegistrationConfigurationRequestHandler : IRequestHandler<PasskeyBuildRegistrationConfigurationRequest, RegisterPasskeyConfiguration>
{
    private readonly IRepository<User> _repository;
    private readonly IWebAuthnConsumer _consumer;

    public PasskeyBuildRegistrationConfigurationRequestHandler(IRepository<User> repository, IWebAuthnConsumer consumer)
    {
        _repository = repository;
        _consumer = consumer;
    }

    public async Task<RegisterPasskeyConfiguration> Handle(PasskeyBuildRegistrationConfigurationRequest request, CancellationToken cancellationToken)
    {
        var user = await _repository.Get(request.Uuid);
        if (user is null)
            throw new NullReferenceException();
        
        return _consumer.GeneratePasskeyRegistrationConfiguration(
            GeneratePasskeySettings.Create(
                uuid: user.Uuid,
                name: user.Email,
                displayName: user.Username
            ).Build()
        );
    }
}