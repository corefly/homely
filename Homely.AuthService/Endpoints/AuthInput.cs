namespace Homely.AuthService.Endpoints;

internal static class AuthInput
{
    public static bool TryNormalizeEmail(string? email, out string normalizedEmail)
    {
        normalizedEmail = string.Empty;

        if (string.IsNullOrWhiteSpace(email))
        {
            return false;
        }

        var trimmedEmail = email.Trim();
        var atIndex = trimmedEmail.IndexOf('@');

        if (atIndex <= 0 || atIndex == trimmedEmail.Length - 1)
        {
            return false;
        }

        normalizedEmail = trimmedEmail.ToUpperInvariant();
        return true;
    }

    public static string NormalizeEmailForStorage(string email)
    {
        return email.Trim();
    }

    public static string? NormalizeDisplayName(string? displayName)
    {
        return string.IsNullOrWhiteSpace(displayName) ? null : displayName.Trim();
    }
}
