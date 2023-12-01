using App.Settings;
using Identity.Consumer.Models.Oidc.Authentication;
using Identity.Consumer.Services.Consumers.Oidc;
using MediatR;
using Microsoft.Extensions.Options;

namespace App.Requests.Users.Authentication.Microsoft;

public class MicrosoftLoginUserRequest : IRequest<Uri> { }

public class MicrosoftLoginUserRequestHandler : IRequestHandler<MicrosoftLoginUserRequest, Uri>
{
    private readonly Configuration _configuration;
    private readonly MicrosoftIdentityConsumer _identityConsumer;

    public MicrosoftLoginUserRequestHandler(IOptions<Configuration> configuration, MicrosoftIdentityConsumer identityConsumer)
    {
        _configuration = configuration.Value;
        _identityConsumer = identityConsumer;
    }

    public async Task<Uri> Handle(MicrosoftLoginUserRequest request, CancellationToken cancellationToken)
        => await _identityConsumer.BuildAuthenticationRedirectUri(
            IdentityAuthenticationRequest.Create(
                clientId: _configuration.Credentials.Microsoft.ClientId, 
                redirectUri: _configuration.Credentials.Microsoft.Callback
            )
                .WithScope("email profile")
                .WithState(_identityConsumer.GenerateAntiForgeryToken())
                .WithNonce(Guid.NewGuid().ToString("D"))
            .Build()
        );
}