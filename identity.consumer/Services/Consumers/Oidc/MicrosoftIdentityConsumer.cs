using Identity.Consumer.Configuration;
using Microsoft.Extensions.Options;

namespace Identity.Consumer.Services.Consumers.Oidc;

/// <summary>
/// An <see cref="IdentityConsumer"/> for the Microsoft OIDC platform.
/// </summary>
public class MicrosoftIdentityConsumer : IdentityConsumer
{
    public MicrosoftIdentityConsumer(IOptions<IdentityConsumerConfiguration> configuration) :
        base(configuration.Value.Oidc.Microsoft) { }
}