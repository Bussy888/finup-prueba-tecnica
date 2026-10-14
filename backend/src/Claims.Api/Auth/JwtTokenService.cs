using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Claims.Api.Auth;

public class JwtTokenService(IOptions<JwtOptions> options, TimeProvider clock)
{
    private readonly JwtOptions _options = options.Value;

    public (string Token, DateTime ExpiresAt) CreateToken()
    {
        var expires = clock.GetUtcNow().UtcDateTime.AddMinutes(_options.ExpiresMinutes);
        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            _options.Issuer,
            _options.Audience,
            [new Claim(ClaimTypes.Role, "Admin")],
            expires: expires,
            signingCredentials: credentials);

        return (new JwtSecurityTokenHandler().WriteToken(token), expires);
    }
}
