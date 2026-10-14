using Claims.Api.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Claims.Api.Controllers;

public record LoginRequest(string? ApiKey);

public record LoginResponse(string Token, DateTime ExpiresAt);

[ApiController]
[Route("api/auth")]
public class AuthController(IOptions<AdminOptions> admin, JwtTokenService tokens) : ControllerBase
{
    [HttpPost("login")]
    [ProducesResponseType<LoginResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public ActionResult<LoginResponse> Login(LoginRequest request)
    {
        if (!ApiKeyValidator.IsValid(request.ApiKey ?? "", admin.Value.ApiKey))
            return Unauthorized();

        var (token, expiresAt) = tokens.CreateToken();
        return new LoginResponse(token, expiresAt);
    }
}
