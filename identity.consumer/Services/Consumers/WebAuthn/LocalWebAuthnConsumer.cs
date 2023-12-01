using Identity.Consumer.Configuration;
using Microsoft.Extensions.Options;

namespace Identity.Consumer.Services.Consumers.WebAuthn;

public class LocalWebAuthnConsumer : WebAuthnConsumer
{
    public LocalWebAuthnConsumer(IOptions<IdentityConsumerConfiguration> configuration) :
        base(id: configuration.Value.WebAuthn.Id, origin: configuration.Value.WebAuthn.Origin) { }
}