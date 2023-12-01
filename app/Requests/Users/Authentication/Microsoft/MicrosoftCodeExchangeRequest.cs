using App.Settings;
using Identity.Consumer.Models.Oidc.Authentication;
using Identity.Consumer.Services.Consumers.Oidc;
using MediatR;
using Microsoft.Extensions.Options;

namespace App.Requests.Users.Authentication.Microsoft;

public class MicrosoftCodeExchangeRequest : IRequest<IdentityCodeExchangeResponse>
{
    public string Code { get; }
    public string State { get; }

    public MicrosoftCodeExchangeRequest(string code, string state)
        => (Code, State) = (code, state);
}

public class MicrosoftCodeExchangeRequestHandler : IRequestHandler<MicrosoftCodeExchangeRequest, IdentityCodeExchangeResponse>
{
    private readonly Configuration _configuration;
    private readonly MicrosoftIdentityConsumer _identityConsumer;

    public MicrosoftCodeExchangeRequestHandler(IOptions<Configuration> congiration, MicrosoftIdentityConsumer identityConsumer)
    {
        _configuration = congiration.Value;
        _identityConsumer = identityConsumer;
    }

    public async Task<IdentityCodeExchangeResponse> Handle(MicrosoftCodeExchangeRequest request, CancellationToken cancellationToken)
    {
        var result = _identityConsumer.VerifyAntiForgeryToken(request.State);
        if (!result)
            throw new UnauthorizedAccessException("The given state token is invalid.");
        
        var response = await _identityConsumer.SendCodeExchangeRequest(
            IdentityCodeExchangeRequest.Create(
                code: request.Code,
                clientId: _configuration.Credentials.Microsoft.ClientId,
                clientSecret: _configuration.Credentials.Microsoft.ClientSecret,
                redirectUri: _configuration.Credentials.Microsoft.Callback
            )
            .Build()
        );

        var verification = await _identityConsumer.VerifyIdToken(
            token: response!.IdToken,
            settings: new IdentityIdTokenVerificationSettings(
                clientId: _configuration.Credentials.Microsoft.ClientId
            )
        );

        if (!verification)
            throw new UnauthorizedAccessException("The retrieved id token is invalid.");

        return response;
    }
}