using App.Settings;
using Identity.Consumer.Models.Oidc.Authentication;
using Identity.Consumer.Services.Consumers.Oidc;
using MediatR;
using Microsoft.Extensions.Options;

namespace App.Requests.Users.Authentication.Google;

public class GoogleCodeExchangeRequest : IRequest<IdentityCodeExchangeResponse>
{
    public string Code { get; }
    public string State { get; }

    public GoogleCodeExchangeRequest(string code, string state)
        => (Code, State) = (code, state);
}

public class GoogleCodeExchangeRequestHandler : IRequestHandler<GoogleCodeExchangeRequest, IdentityCodeExchangeResponse>
{
    private readonly Configuration _configuration;
    private readonly GoogleIdentityConsumer _identityConsumer;

    public GoogleCodeExchangeRequestHandler(IOptions<Configuration> configuration, GoogleIdentityConsumer identityConsumer)
    {
        _configuration = configuration.Value;
        _identityConsumer = identityConsumer;
    }

    public async Task<IdentityCodeExchangeResponse> Handle(GoogleCodeExchangeRequest request, CancellationToken cancellationToken)
    {
        var result = _identityConsumer.VerifyAntiForgeryToken(request.State);
        if (!result)
            throw new UnauthorizedAccessException();
        
        var response = await _identityConsumer.SendCodeExchangeRequest(
            IdentityCodeExchangeRequest.Create(
                code: request.Code,
                clientId: _configuration.Credentials.Google.ClientId,
                clientSecret: _configuration.Credentials.Google.ClientSecret,
                redirectUri: _configuration.Credentials.Google.Callback
            )
            .Build()
        );
        
        var verification = await _identityConsumer.VerifyIdToken(
            token: response!.IdToken,
            settings: new IdentityIdTokenVerificationSettings(
                clientId: _configuration.Credentials.Google.ClientId
            )
        );

        if (!verification)
            throw new UnauthorizedAccessException("The retrieved id token is invalid.");

        return response;
    }
}