using System.ComponentModel.DataAnnotations;

namespace Homely.AuthService.Contracts;

public sealed record SignUpRequest(
    [property: Required, EmailAddress] string Email,
    [property: Required, MinLength(8)] string Password,
    string? DisplayName);
