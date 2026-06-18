using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace BuildingBlock.Auth;

public static class AuthenticationExtensions
{
    // Shared Keycloak (OIDC) JWT bearer validation for every service and the gateway.
    // Authority/Audience come from config (Jwt:Authority, Jwt:Audience). Keycloak carries roles
    // under the realm_access.roles claim; they are flattened into standard role claims so that
    // [Authorize(Roles = "...")] and role policies work.
    public static IServiceCollection AddStandardJwtAuth(this IServiceCollection services, IConfiguration configuration)
    {
        var authority = configuration["Jwt:Authority"];
        var audience = configuration["Jwt:Audience"];

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = authority;
                options.Audience = audience;
                options.RequireHttpsMetadata = false; // dev: Keycloak served over http within the compose network
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = !string.IsNullOrWhiteSpace(audience),
                    ValidateLifetime = true,
                    RoleClaimType = ClaimTypes.Role,
                    NameClaimType = "preferred_username"
                };
                options.Events = new JwtBearerEvents { OnTokenValidated = MapRealmRoles };
            });

        services.AddAuthorization();
        return services;
    }

    private static Task MapRealmRoles(TokenValidatedContext context)
    {
        if (context.Principal?.Identity is not ClaimsIdentity identity)
            return Task.CompletedTask;

        var realmAccess = context.Principal.FindFirst("realm_access")?.Value;
        if (string.IsNullOrWhiteSpace(realmAccess))
            return Task.CompletedTask;

        using var document = JsonDocument.Parse(realmAccess);
        if (document.RootElement.TryGetProperty("roles", out var roles) && roles.ValueKind == JsonValueKind.Array)
        {
            foreach (var role in roles.EnumerateArray())
            {
                var value = role.GetString();
                if (!string.IsNullOrWhiteSpace(value))
                    identity.AddClaim(new Claim(ClaimTypes.Role, value));
            }
        }

        return Task.CompletedTask;
    }
}
