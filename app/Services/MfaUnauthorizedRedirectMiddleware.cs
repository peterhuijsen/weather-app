using App.Settings;
using App.Validation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace App.Services;

public class MfaAttribute : ActionFilterAttribute
{
    // private readonly IAuthorizationService _authorizationService;
    // private readonly Configuration _configuration;
    //
    // public MfaRedirectFilter(
    //     IAuthorizationService authorizationService, 
    //     IOptions<Configuration> configuration)
    // {
    //     _authorizationService = authorizationService;
    //     _configuration = configuration.Value;
    // }
    
    public override async Task OnActionExecutionAsync(ActionExecutingContext actionContext, ActionExecutionDelegate next)
    {
        var context = actionContext.HttpContext;
        
        var authorizationService = context.RequestServices.GetService<IAuthorizationService>();
        var configuration = context.RequestServices.GetService<IOptions<Configuration>>();
        
        if (authorizationService is null || configuration is null)
        {
            await next();
            return;
        }
            
        var result = await authorizationService.AuthorizeAsync(
            user: context.User, 
            resource: context.GetRouteData(),
            policyName: Policies.MFA_REQUIRED_IF_ENABLED_POLICY
        );
            
        if (!result.Succeeded)
        {
            context.Response.Headers.Add("Content-Type", "application/json");
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                
            await context.Response.WriteAsync(
                text: JsonConvert.SerializeObject(
                    value: new
                    {
                        Message = "The given user has enabled MFA, please also verify via HOTP or TOTP.",
                        Url = configuration.Value.Client.LoginUrl,
                    }, 
                    settings: new JsonSerializerSettings
                    {
                        ContractResolver = new CamelCasePropertyNamesContractResolver()
                    }
                )
            );
                
            await context.Response.CompleteAsync();
                
            return;
        }
        
        await next();
    }
}

public class MfaUnauthorizedRedirectMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IAuthorizationService _authorizationService;
    
    private readonly Configuration _configuration;
    
    public MfaUnauthorizedRedirectMiddleware(
        RequestDelegate next, 
        IAuthorizationService authorizationService, 
        IOptions<Configuration> configuration)
    {
        _next = next;
        _authorizationService = authorizationService;
        _configuration = configuration.Value;
    }
    
    public async Task Invoke(HttpContext context)
    {
        var endpoint = context.Features.Get<IEndpointFeature>()?.Endpoint;
        var authorizeAttribute = endpoint?.Metadata.GetMetadata<AuthorizeAttribute>();
        
        if (authorizeAttribute is { Policy: Policies.MFA_REQUIRED_IF_ENABLED_POLICY })
        {
            var result = await _authorizationService.AuthorizeAsync(
                user: context.User, 
                resource: context.GetRouteData(),
                policyName: authorizeAttribute.Policy
            );
            
            if (!result.Succeeded)
            {
                context.Response.Headers.Add("Content-Type", "application/json");
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                
                await context.Response.WriteAsync(
                    text: JsonConvert.SerializeObject(
                        value: new
                        {
                            Message = "The given user has enabled MFA, please also verify via HOTP or TOTP.",
                            Url = _configuration.Client.LoginUrl,
                        }, 
                        settings: new JsonSerializerSettings
                        {
                            ContractResolver = new CamelCasePropertyNamesContractResolver()
                        }
                    )
                );
                
                await context.Response.CompleteAsync();
                
                return;
            }
        }
        
        await _next(context);
    }
}