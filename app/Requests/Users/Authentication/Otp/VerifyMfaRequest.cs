using System;
using System.Threading;
using System.Threading.Tasks;
using App.Models;
using App.Models.Codes;
using App.Models.Controllers.Users;
using App.Services;
using MediatR;

namespace App.Requests.Users.Authentication.Otp;

public class VerifyMfaRequest : IRequest<LoginUserResponse>
{
    public User User { get; set; }
    public VerifyOneTimeTokenResponse TotpResponse { get; set; }
    public VerifyOneTimeTokenResponse HotpResponse { get; set; }

    public VerifyMfaRequest(User user, VerifyOneTimeTokenResponse totpResponse, VerifyOneTimeTokenResponse hotpResponse)
    {
        User = user;
        TotpResponse = totpResponse;
        HotpResponse = hotpResponse;
    }
}

public class VerifyMfaRequestHandler : IRequestHandler<VerifyMfaRequest, LoginUserResponse>
{
    private readonly IAuthenticationService _authenticationService;

    public VerifyMfaRequestHandler(IAuthenticationService authenticationService)
    {
        _authenticationService = authenticationService;
    }

    public Task<LoginUserResponse> Handle(VerifyMfaRequest request, CancellationToken cancellationToken)
    {
        if (!request.HotpResponse.Verified && !request.TotpResponse.Verified)
            throw new ApplicationException("The given OTP is invalid.");
        
        var token = _authenticationService.Token(request.User, loa: 2);
        return Task.FromResult(
            new LoginUserResponse
            {
                Token = token,
                User = request.User
            }
        );
    }
}