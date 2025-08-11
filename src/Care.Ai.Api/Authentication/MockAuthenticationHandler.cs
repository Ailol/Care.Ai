using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using System.Globalization;

namespace Care.AI.Api.Authentication;

public class MockAuthenticationSchemeOptions : AuthenticationSchemeOptions
{
}

public class MockAuthenticationHandler : AuthenticationHandler<MockAuthenticationSchemeOptions>
{
    public MockAuthenticationHandler(
        IOptionsMonitor<MockAuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Get user type from header for testing
        var userType = Request.Headers["X-User-Type"].FirstOrDefault() ?? "guest";
        var userId = Request.Headers["X-User-Id"].FirstOrDefault() ?? "anonymous";

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Name, $"{userType}-{userId}")
        };

        // Add role claims based on user type
        switch (userType.ToLower(CultureInfo.InvariantCulture))
        {
            case "admin":
                claims.Add(new Claim(ClaimTypes.Role, "Administrator"));
                claims.Add(new Claim(ClaimTypes.Role, "User"));
                break;
            case "manager":
                claims.Add(new Claim(ClaimTypes.Role, "Manager"));
                claims.Add(new Claim(ClaimTypes.Role, "User"));
                break;
            case "user":
                claims.Add(new Claim(ClaimTypes.Role, "User"));
                break;
            default:
                claims.Add(new Claim(ClaimTypes.Role, "Guest"));
                break;
        }

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}