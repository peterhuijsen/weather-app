using System;
using System.Threading.Tasks;
using App.Models;
using App.Models.Codes;
using App.Models.Controllers.Users;
using App.Requests.Users;
using App.Requests.Users.Authentication.General;
using App.Requests.Users.Authentication.Google;
using App.Requests.Users.Authentication.Microsoft;
using App.Requests.Users.Authentication.Otp;
using App.Requests.Users.Authentication.Otp.Hotp;
using App.Requests.Users.Authentication.Otp.Totp;
using App.Requests.Users.Authentication.Passkeys;
using App.Requests.Users.Authentication.Passwords;
using App.Services;
using App.Settings;
using App.Validation;
using Identity.Consumer.Models.Oidc;
using Identity.Consumer.Models.Passkeys.Authentication;
using Identity.Consumer.Models.Passkeys.Registration;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace App.Controllers;

[ApiController]
[Route("api/users")]
public class UserController : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromServices] IMediator mediator, [FromBody] RegisterUserParameters parameters)
    {
        await mediator.Send(new CreateUserRequest(parameters.Username, parameters.Email, parameters.Password));
        var response = await mediator.Send(new PasswordLoginUserRequest(parameters.Email, parameters.Password));
        await mediator.Send(new SetAuthenticationCookieRequest(Response, response));
        
        return Ok();
    }

    
    [Authorize]
    [HttpPut("{uuid:guid}/otp/register/hotp")]
    public async Task<RegisterOneTimeTokenResponse> RegisterHotp([FromServices] IMediator mediator, Guid uuid)
    {
        var user = await mediator.Send(new GetUserRequest(uuid));
        var response = await mediator.Send(new RegisterHotpRequest(user));

        return response;
    }
    
    [Authorize]
    [HttpPut("{uuid:guid}/otp/verify/hotp")]
    public async Task<VerifyOneTimeTokenResponse> VerifyHotp([FromServices] IMediator mediator, [FromQuery] string hotp, Guid uuid)
    {
        var user = await mediator.Send(new GetUserRequest(uuid));
        var response = await mediator.Send(new VerifyHotpRequest(user, hotp));

        return response;
    }
    
    [Authorize]
    [HttpPut("{uuid:guid}/otp/register/totp")]
    public async Task<RegisterOneTimeTokenResponse> RegisterTotp([FromServices] IMediator mediator, Guid uuid)
    {
        var user = await mediator.Send(new GetUserRequest(uuid));
        var response = await mediator.Send(new RegisterTotpRequest(user));

        return response;
    }
    
    [Authorize]
    [HttpPut("{uuid:guid}/otp/verify/totp")]
    public async Task<VerifyOneTimeTokenResponse> VerifyTotp([FromServices] IMediator mediator, [FromQuery] string totp, Guid uuid)
    {
        var user = await mediator.Send(new GetUserRequest(uuid));
        var response = await mediator.Send(new VerifyTotpRequest(user, totp));

        return response;
    }

    [Authorize(Policy = Policies.MFA_MISSING_POLICY)]
    [HttpPost("{uuid:guid}/login/mfa")]
    public async Task<IActionResult?> VerifyMFA(
        [FromServices] IMediator mediator,
        [FromQuery] string otp,
        Guid uuid)
    {
        var user = await mediator.Send(new GetUserRequest(uuid));
        if (user is null)
            return BadRequest("The given user does not exist.");

        var totpResult = await mediator.Send(new VerifyTotpRequest(user, otp)); 
        var hotpResult = await mediator.Send(new VerifyHotpRequest(user, otp)); 
        
        var response = await mediator.Send(new VerifyMfaRequest(user, totpResult, hotpResult)); 
        await mediator.Send(new SetAuthenticationCookieRequest(Response, response));
        
        return Ok();
    }
    
    [HttpPost("login/password")]
    public async Task<IActionResult> PasswordLogin([FromServices] IMediator mediator, [FromBody] PasswordLoginUserParameters parameters)
    {
        var response = await mediator.Send(new PasswordLoginUserRequest(parameters.Email, parameters.Password));
        await mediator.Send(new SetAuthenticationCookieRequest(Response, response));
        return Ok();
    }


    [Mfa]
    [Authorize]
    [HttpPut("{uuid:guid}/webauthn/attest/generate")]
    public async Task<RegisterPasskeyConfiguration> PasskeyBuildAttestation([FromServices] IMediator mediator, Guid uuid)
        => await mediator.Send(new PasskeyBuildRegistrationConfigurationRequest(uuid));

    [Mfa]
    [Authorize]
    [HttpPut("{uuid:guid}/webauthn/attest")]
    public async Task PasskeyConfirmAttestation([FromServices] IMediator mediator, [FromBody] ConfirmRegisterPasskeyRequest request, Guid uuid)
        => await mediator.Send(new PasskeyConfirmRegistrationRequest(uuid, request));

    [HttpPost("login/webauthn/assert/generate")]
    public async Task<AuthenticatePasskeyConfiguration> PasskeyBuildAssertion([FromServices] IMediator mediator, [FromQuery] string state)
        => await mediator.Send(new PasskeyBuildAuthenticationConfigurationRequest(state));

    [HttpPost("login/webauthn/assert")]
    public async Task<IActionResult> PasskeyConfirmAssertion(
        [FromServices] IMediator mediator, 
        [FromQuery] string state, 
        [FromBody] ConfirmAuthenticatePasskeyRequest request)
    {
        var response = await mediator.Send(new PasskeyAuthenticateUserRequest(state, request));
        await mediator.Send(new SetAuthenticationCookieRequest(Response, response!));
        return Ok();
    }


    [HttpGet("login/oidc/google")]
    public async Task<IActionResult> GoogleLogin([FromServices] IMediator mediator)
        => Redirect((await mediator.Send(new GoogleLoginUserRequest())).ToString());
    
    [HttpGet("login/oidc/google/callback")]
    public async Task<IActionResult> GoogleCallback(
        [FromServices] IOptions<Configuration> configuration,
        [FromServices] IMediator mediator, 
        [FromQuery] string code,
        [FromQuery] string state)
    {
        var exchange = await mediator.Send(new GoogleCodeExchangeRequest(code, state));
        if (exchange.IdToken.Payload.Email is not null &&
            await mediator.Send(new GetUserRequest(exchange.IdToken.Payload.Email)) is null)
            await mediator.Send(new CreateUserRequest(exchange.IdToken, OidcProviderType.Google));

        var response = await mediator.Send(new GoogleAuthenticateUserRequest(exchange.IdToken));
        await mediator.Send(new SetAuthenticationCookieRequest(Response, response!));
        
        return Redirect(configuration.Value.Client.Url);
    }


    [HttpGet("login/oidc/microsoft")]
    public async Task<IActionResult> MicrosoftLogin([FromServices] IMediator mediator)
        => Redirect((await mediator.Send(new MicrosoftLoginUserRequest())).ToString());
    
    [HttpGet("login/oidc/microsoft/callback")]
    public async Task<IActionResult> MicrosoftCallback(
        [FromServices] IOptions<Configuration> configuration,
        [FromServices] IMediator mediator, 
        [FromQuery] string code,
        [FromQuery] string state)
    {
        var exchange = await mediator.Send(new MicrosoftCodeExchangeRequest(code, state));
        if (exchange.IdToken.Payload.Email is not null &&
            await mediator.Send(new GetUserRequest(exchange.IdToken.Payload.Email)) is null)
            await mediator.Send(new CreateUserRequest(exchange.IdToken, OidcProviderType.Microsoft));
        
        var response = await mediator.Send(new MicrosoftAuthenticateUserRequest(exchange.IdToken));
        await mediator.Send(new SetAuthenticationCookieRequest(Response, response!));

        return Redirect(configuration.Value.Client.Url);
    }
    
    [Mfa]
    [Authorize]
    [HttpGet("{uuid:guid}")]
    public async Task<User> Get([FromServices] IMediator mediator, Guid uuid)
        => await mediator.Send(new GetUserRequest(uuid)) ?? throw new NullReferenceException("Unable to retrieve user.");

    [Mfa]
    [Authorize]
    [HttpPut("{uuid:guid}")]
    public async Task<User> Edit([FromServices] IMediator mediator, [FromBody] EditUserParameters parameters, Guid uuid)
        => await mediator.Send(new UpdateUserRequest(uuid, parameters.Username, parameters.MFA));
    
    [Mfa]
    [Authorize]
    [HttpDelete("{uuid:guid}")]
    public async Task<bool> Remove([FromServices] IMediator mediator, Guid uuid)
        => await mediator.Send(new DeleteUserRequest(uuid));
}