namespace Claims.Api.Auth;

public class AdminOptions
{
    public const string Section = "Admin";
    public string ApiKey { get; set; } = "";
}

public class JwtOptions
{
    public const string Section = "Jwt";
    public string Key { get; set; } = "";
    public string Issuer { get; set; } = "claims-api";
    public string Audience { get; set; } = "claims-web";
    public int ExpiresMinutes { get; set; } = 60;
}
