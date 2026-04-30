namespace Homely.AuthService.Security;

public sealed class JwtOptions
{
    public string Issuer { get; set; } = "homely-auth";
    public string Audience { get; set; } = "homely";
    public string SigningKey { get; set; } = "homely-development-signing-key";
    public int ExpiresMinutes { get; set; } = 60;
}
