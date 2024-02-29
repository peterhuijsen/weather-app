using System;
using System.Threading;
using System.Threading.Tasks;
using App.Settings;
using Identity.Consumer.Models.Oidc.Authentication;
using Identity.Consumer.Services.Consumers.Oidc;
using MediatR;
using Microsoft.Extensions.Options;

namespace App.Requests.Users.Authentication.Google;

public class GoogleLoginUserRequest : IRequest<Uri> { }

public class GoogleLoginUserRequestHandler : IRequestHandler<GoogleLoginUserRequest, Uri>
{
    private readonly Configuration _configuration;
    private readonly GoogleIdentityConsumer _identityConsumer;

    public GoogleLoginUserRequestHandler(IOptions<Configuration> configuration, GoogleIdentityConsumer identityConsumer)
    {
        _configuration = configuration.Value;
        _identityConsumer = identityConsumer;
    }

    public async Task<Uri> Handle(GoogleLoginUserRequest request, CancellationToken cancellationToken)
        => await _identityConsumer.BuildAuthenticationRedirectUri(
            IdentityAuthenticationRequest.Create(
                clientId: _configuration.Credentials.Google.ClientId,
                redirectUri: _configuration.Credentials.Google.Callback
            )
                .WithScope("email profile")
                .WithState(_identityConsumer.GenerateAntiForgeryToken())
                .WithNonce(Guid.NewGuid().ToString("D"))
            .Build()
        );
}