using Identity.Consumer.Models.Tokens;

namespace Identity.Consumer.Models.Oidc.Authentication;

public class IdentityCodeExchangeResponse
{
    public IdToken IdToken { get; set; } = null!;
}