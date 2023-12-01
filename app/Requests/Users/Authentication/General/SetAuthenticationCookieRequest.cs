using App.Models.Controllers.Users;
using MediatR;

namespace App.Requests.Users.Authentication.General;

public class SetAuthenticationCookieRequest : IRequest
{
    public HttpResponse Response { get; }

    public LoginUserResponse Data { get; }

    public SetAuthenticationCookieRequest(HttpResponse response, LoginUserResponse data)
        => (Response, Data) = (response, data);
}

public class SetAuthenticationCookieRequestHandler : IRequestHandler<SetAuthenticationCookieRequest>
{
    public Task Handle(SetAuthenticationCookieRequest request, CancellationToken cancellationToken)
    {
        request.Response.Cookies.Append(
            key: "t",
            value: request.Data.Token, 
            options: new CookieOptions
            {
                // When using cookies for storing our access token, we need to make sure
                // that the token will not be sent via HTTP without SSL.
                Secure = false,
                // We also want the token to only be accessible by the server, so the client
                // can't mess with the token.
                HttpOnly = true,
                // Lastly we want to prevent the token from being sent when the used method
                // is dangerous/susceptbile for XSS, i.e POST, PUT, PATCH. We still allow GET 
                // and HEAD, etc. for a seamless integration with the frontend, but this is 
                // indeed a bigger security risk than it needs to be.
                SameSite = SameSiteMode.Lax,
            }
        );
        
        // The user id cookie needs no protections from XSS, it isn't important information. 
        request.Response.Cookies.Append("u", request.Data.User.Uuid.ToString("D"));
        
        return Task.CompletedTask;
    }
}