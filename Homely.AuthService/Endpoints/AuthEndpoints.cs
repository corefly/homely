using System.Security.Claims;
using Homely.AuthService.Contracts;
using Homely.AuthService.Domain;
using Homely.AuthService.Security;
using Marten;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;

namespace Homely.AuthService.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(
        this IEndpointRouteBuilder endpoints,
        AuthEndpointOptions options)
    {
        var auth = endpoints.MapGroup("/auth");

        auth.MapPost("/sign-in", SignInAsync)
            .WithName("SignIn");

        auth.MapPost("/sign-up", SignUpAsync)
            .WithName("SignUp");

        auth.MapGet("/google", () => BeginGoogleSignIn(options))
            .WithName("GoogleSignIn");

        auth.MapGet("/google/callback", (
                    HttpContext httpContext,
                    IDocumentSession session,
                    JwtTokenService tokenService,
                    CancellationToken cancellationToken) =>
                CompleteGoogleSignInAsync(httpContext, session, tokenService, options, cancellationToken))
            .WithName("GoogleCallback");

        return endpoints;
    }

    private static async Task<IResult> SignInAsync(
        SignInRequest request,
        IDocumentSession session,
        IPasswordHasher<AuthUser> passwordHasher,
        JwtTokenService tokenService,
        CancellationToken cancellationToken)
    {
        if (!AuthInput.TryNormalizeEmail(request.Email, out var normalizedEmail) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                ["credentials"] = ["Email and password are required."]
            });
        }

        var user = await session.Query<AuthUser>()
            .FirstOrDefaultAsync(candidate => candidate.NormalizedEmail == normalizedEmail, cancellationToken);

        if (user is null || string.IsNullOrWhiteSpace(user.PasswordHash))
        {
            return Results.Unauthorized();
        }

        var verification = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);

        if (verification == PasswordVerificationResult.Failed)
        {
            return Results.Unauthorized();
        }

        if (verification == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.PasswordHash = passwordHasher.HashPassword(user, request.Password);
            user.UpdatedAt = DateTimeOffset.UtcNow;
            session.Store(user);
            await session.SaveChangesAsync(cancellationToken);
        }

        return Results.Ok(tokenService.CreateToken(user));
    }

    private static async Task<IResult> SignUpAsync(
        SignUpRequest request,
        IDocumentSession session,
        IPasswordHasher<AuthUser> passwordHasher,
        JwtTokenService tokenService,
        CancellationToken cancellationToken)
    {
        if (!AuthInput.TryNormalizeEmail(request.Email, out var normalizedEmail))
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                [nameof(request.Email)] = ["A valid email is required."]
            });
        }

        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 8)
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                [nameof(request.Password)] = ["Password must be at least 8 characters."]
            });
        }

        var existingUser = await session.Query<AuthUser>()
            .AnyAsync(user => user.NormalizedEmail == normalizedEmail, cancellationToken);

        if (existingUser)
        {
            return Results.Conflict(new { message = "A user with this email already exists." });
        }

        var user = new AuthUser
        {
            Email = AuthInput.NormalizeEmailForStorage(request.Email),
            NormalizedEmail = normalizedEmail,
            DisplayName = AuthInput.NormalizeDisplayName(request.DisplayName)
        };

        user.PasswordHash = passwordHasher.HashPassword(user, request.Password);

        session.Store(user);
        await session.SaveChangesAsync(cancellationToken);

        return Results.Created($"/auth/users/{user.Id}", tokenService.CreateToken(user));
    }

    private static IResult BeginGoogleSignIn(AuthEndpointOptions options)
    {
        if (!options.IsGoogleConfigured)
        {
            return Results.Problem("Google sign-in is not configured.");
        }

        var properties = new AuthenticationProperties
        {
            RedirectUri = "/auth/google/callback"
        };

        return Results.Challenge(properties, [options.GoogleAuthScheme]);
    }

    private static async Task<IResult> CompleteGoogleSignInAsync(
        HttpContext httpContext,
        IDocumentSession session,
        JwtTokenService tokenService,
        AuthEndpointOptions options,
        CancellationToken cancellationToken)
    {
        var externalAuth = await httpContext.AuthenticateAsync(options.ExternalAuthScheme);

        if (!externalAuth.Succeeded || externalAuth.Principal is null)
        {
            return Results.Unauthorized();
        }

        var email = externalAuth.Principal.FindFirstValue(ClaimTypes.Email);
        var providerUserId = externalAuth.Principal.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(providerUserId))
        {
            return Results.Problem("Google did not return the required user identity claims.");
        }

        if (!AuthInput.TryNormalizeEmail(email, out var normalizedEmail))
        {
            return Results.Problem("Google did not return a valid email address.");
        }

        var user = await session.Query<AuthUser>()
            .FirstOrDefaultAsync(candidate => candidate.NormalizedEmail == normalizedEmail, cancellationToken);

        if (user is null)
        {
            user = new AuthUser
            {
                Email = AuthInput.NormalizeEmailForStorage(email),
                NormalizedEmail = normalizedEmail,
                DisplayName = AuthInput.NormalizeDisplayName(externalAuth.Principal.FindFirstValue(ClaimTypes.Name)),
                ExternalLogins =
                [
                    new ExternalLogin
                    {
                        Provider = options.GoogleAuthScheme,
                        ProviderUserId = providerUserId
                    }
                ]
            };

            session.Store(user);
        }
        else if (!user.ExternalLogins.Any(login =>
                     string.Equals(login.Provider, options.GoogleAuthScheme, StringComparison.OrdinalIgnoreCase)
                     && login.ProviderUserId == providerUserId))
        {
            user.ExternalLogins.Add(new ExternalLogin
            {
                Provider = options.GoogleAuthScheme,
                ProviderUserId = providerUserId
            });
            user.UpdatedAt = DateTimeOffset.UtcNow;
            session.Store(user);
        }

        await session.SaveChangesAsync(cancellationToken);
        await httpContext.SignOutAsync(options.ExternalAuthScheme);

        return Results.Ok(tokenService.CreateToken(user));
    }
}
