namespace Homely.AuthService.Domain;

public sealed class AuthUser
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Email { get; set; }
    public required string NormalizedEmail { get; set; }
    public string? DisplayName { get; set; }
    public string? PasswordHash { get; set; }
    public List<ExternalLogin> ExternalLogins { get; set; } = [];
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}
