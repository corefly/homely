namespace Homely.AuthService.Domain;

public sealed class ExternalLogin
{
    public required string Provider { get; set; }

    public required string ProviderUserId { get; set; }
}
