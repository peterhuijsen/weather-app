using Identity.Consumer.Configuration;
using Microsoft.Extensions.Options;

namespace Identity.Consumer.Services.Consumers.Oidc;

/// <summary>
/// An <see cref="IdentityConsumer"/> for the Google OIDC platform.
/// </summary>
public class GoogleIdentityConsumer : IdentityConsumer
{
    public GoogleIdentityConsumer(IOptions<IdentityConsumerConfiguration> configuration) :
        base(configuration.Value.Oidc.Google) { }
}