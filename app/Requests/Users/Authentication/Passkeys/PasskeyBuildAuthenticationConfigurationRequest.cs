using Identity.Consumer.Models.Passkeys.Authentication;
using Identity.Consumer.Models.Passkeys.Registration;
using Identity.Consumer.Services;
using MediatR;

namespace App.Requests.Users.Authentication.Passkeys;

public class PasskeyBuildAuthenticationConfigurationRequest : IRequest<AuthenticatePasskeyConfiguration>
{
    public string State { get; set; }

    public PasskeyBuildAuthenticationConfigurationRequest(string state)
    {
        State = state;
    }
}

public class PasskeyBuildAuthenticationConfigurationRequestHandler : IRequestHandler<PasskeyBuildAuthenticationConfigurationRequest, AuthenticatePasskeyConfiguration>
{
    private readonly IWebAuthnConsumer _consumer;

    public PasskeyBuildAuthenticationConfigurationRequestHandler(IWebAuthnConsumer consumer)
    {
        _consumer = consumer;
    }

    public Task<AuthenticatePasskeyConfiguration> Handle(PasskeyBuildAuthenticationConfigurationRequest request, CancellationToken cancellationToken)
        => Task.FromResult(
            _consumer.GeneratePasskeyAuthenticationConfiguration(
                GeneratePasskeySettings.Create(state: request.State)
                    .Build()
            )
        );
}