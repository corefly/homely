using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Homely.AuthService.Contracts;
using Homely.AuthService.Domain;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Homely.AuthService.Security;

public sealed class JwtTokenService(IOptions<JwtOptions> options)
{
    private readonly JwtOptions _options = options.Value;

    public AuthResponse CreateToken(AuthUser user)
    {
        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(_options.ExpiresMinutes);
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SigningKey));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email)
        };

        if (!string.IsNullOrWhiteSpace(user.DisplayName))
        {
            claims.Add(new Claim(ClaimTypes.Name, user.DisplayName));
        }

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: expiresAt.UtcDateTime,
            signingCredentials: credentials);

        return new AuthResponse(
            new JwtSecurityTokenHandler().WriteToken(token),
            expiresAt,
            ToResponse(user));
    }

    public static UserResponse ToResponse(AuthUser user)
    {
        var providers = new List<string>();

        if (!string.IsNullOrWhiteSpace(user.PasswordHash))
        {
            providers.Add("password");
        }

        providers.AddRange(user.ExternalLogins.Select(login => login.Provider).Distinct(StringComparer.OrdinalIgnoreCase));

        return new UserResponse(user.Id, user.Email, user.DisplayName, providers);
    }
}
