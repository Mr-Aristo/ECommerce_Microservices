using System.Security.Claims;

namespace BuildingBlock.Auth;

// Helpers to read the authenticated user's identity from the validated Keycloak token.
public static class ClaimsPrincipalExtensions
{
    // Keycloak 'sub' (mapped to NameIdentifier by the JWT handler) — the system-wide user id.
    public static string GetUserId(this ClaimsPrincipal user) =>
        user.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? user.FindFirstValue("sub")
        ?? throw new UnauthorizedAccessException("Token is missing the subject (sub) claim.");

    public static string GetEmail(this ClaimsPrincipal user) =>
        user.FindFirstValue(ClaimTypes.Email) ?? user.FindFirstValue("email") ?? string.Empty;

    public static string GetUserName(this ClaimsPrincipal user) =>
        user.Identity?.Name ?? user.FindFirstValue("preferred_username") ?? string.Empty;
}
