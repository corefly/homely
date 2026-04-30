namespace Homely.AuthService.Contracts;

public sealed record UserResponse(
    Guid Id,
    string Email,
    string? DisplayName,
    IReadOnlyCollection<string> LoginProviders);
