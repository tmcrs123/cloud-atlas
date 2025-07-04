using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

public class FakeAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public FakeAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder) : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "local-dev-user-id"),
            new Claim(ClaimTypes.Name, "Local Dev User"),
            new Claim("sub", "7c9e6679-7425-40de-944b-e07fc1f90ae7")
        };

        var identity = new ClaimsIdentity(claims, "FakeAuth");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "FakeAuth");

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}