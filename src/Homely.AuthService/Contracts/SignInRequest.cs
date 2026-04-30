using System.ComponentModel.DataAnnotations;

namespace Homely.AuthService.Contracts;

public sealed record SignInRequest(
    [property: Required, EmailAddress] string Email,
    [property: Required] string Password);
