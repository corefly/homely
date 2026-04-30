namespace Homely.AuthService.Endpoints;

public sealed record AuthEndpointOptions(
    bool IsGoogleConfigured,
    string ExternalAuthScheme,
    string GoogleAuthScheme);
