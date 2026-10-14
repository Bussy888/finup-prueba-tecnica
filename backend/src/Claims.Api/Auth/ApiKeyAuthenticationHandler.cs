using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace Claims.Api.Auth;

public class ApiKeyAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    IOptions<AdminOptions> adminOptions)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string SchemeName = "ApiKey";
    public const string HeaderName = "X-Api-Key";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(HeaderName, out var provided))
            return Task.FromResult(AuthenticateResult.NoResult());

        if (!ApiKeyValidator.IsValid(provided.ToString(), adminOptions.Value.ApiKey))
            return Task.FromResult(AuthenticateResult.Fail("Clave inválida."));

        var identity = new ClaimsIdentity([new Claim(ClaimTypes.Role, "Admin")], SchemeName);
        var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), SchemeName);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}

public static class ApiKeyValidator
{
    public static bool IsValid(string provided, string expected)
    {
        if (string.IsNullOrEmpty(expected) || string.IsNullOrEmpty(provided)) return false;

        var a = SHA256.HashData(Encoding.UTF8.GetBytes(provided));
        var b = SHA256.HashData(Encoding.UTF8.GetBytes(expected));
        return CryptographicOperations.FixedTimeEquals(a, b);
    }
}
