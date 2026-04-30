namespace Homely.AuthService.Contracts;

public sealed record AuthResponse(string AccessToken, DateTimeOffset ExpiresAt, UserResponse User);
