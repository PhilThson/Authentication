using System.Security.Claims;
using System.Text.Encodings.Web;
using Authentication.Core.Constants;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace Authentication.Api.Helpers;

public class ApiKeyAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly IConfiguration _configuration;
    
    public ApiKeyAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options, 
        ILoggerFactory logger, 
        UrlEncoder encoder, 
        ISystemClock clock,
        IConfiguration configuration) 
        : base(options, logger, encoder, clock)
    {
        _configuration = configuration;
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var authHeaders = Context.Request.Headers.Authorization;
        if (!authHeaders.Any())
        {
            return Task.FromResult(AuthenticateResult.Fail("Unauthorized"));
        }
        
        var authHeaderValue = authHeaders.First();
        if (string.IsNullOrEmpty(authHeaderValue))
        {
            return Task.FromResult(AuthenticateResult.Fail("Unauthorized"));
        }
        var registrationKey = _configuration.GetSection(nameof(AuthConstants.RegistrationKey)).Value;
        if (authHeaderValue != registrationKey)
        {
            return Task.FromResult(AuthenticateResult.Fail("Unauthorized"));
        }

        var user = new ClaimsPrincipal(new ClaimsIdentity(Scheme.Name));
        var ticket = new AuthenticationTicket(user, Scheme.Name);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}