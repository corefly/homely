using System.Text;
using Homely.AuthService.Domain;
using Homely.AuthService.Endpoints;
using Homely.AuthService.Security;
using JasperFx;
using Marten;
using Marten.Schema;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

const string ExternalAuthScheme = "External";
const string GoogleAuthScheme = GoogleDefaults.AuthenticationScheme;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));
var jwtOptions = builder.Configuration.GetSection("Jwt").Get<JwtOptions>() ?? new JwtOptions();
var jwtSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey));

builder.Services.AddMarten(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("authdb")
        ?? throw new InvalidOperationException("Missing connection string 'authdb'.");

    options.Connection(connectionString);
    options.AutoCreateSchemaObjects = AutoCreate.CreateOrUpdate;
    options.Schema.For<AuthUser>().UniqueIndex(UniqueIndexType.Computed, user => user.NormalizedEmail);
}).UseLightweightSessions();

builder.Services.AddSingleton<IPasswordHasher<AuthUser>, PasswordHasher<AuthUser>>();
builder.Services.AddSingleton<JwtTokenService>();

var authentication = builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = jwtSigningKey
        };
    })
    .AddCookie(ExternalAuthScheme, options =>
    {
        options.Cookie.Name = "Homely.ExternalAuth";
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.ExpireTimeSpan = TimeSpan.FromMinutes(10);
    });

var googleClientId = builder.Configuration["Authentication:Google:ClientId"];
var googleClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
var isGoogleConfigured = !string.IsNullOrWhiteSpace(googleClientId)
    && !string.IsNullOrWhiteSpace(googleClientSecret);

if (isGoogleConfigured)
{
    authentication.AddGoogle(GoogleAuthScheme, options =>
    {
        options.SignInScheme = ExternalAuthScheme;
        options.ClientId = googleClientId!;
        options.ClientSecret = googleClientSecret!;
        options.CallbackPath = "/auth/google/oauth-callback";
        options.SaveTokens = false;
    });
}

builder.Services.AddAuthorization();

var app = builder.Build();

app.MapDefaultEndpoints();

app.UseAuthentication();
app.UseAuthorization();

app.MapAuthEndpoints(new AuthEndpointOptions(
    isGoogleConfigured,
    ExternalAuthScheme,
    GoogleAuthScheme));

app.Run();
