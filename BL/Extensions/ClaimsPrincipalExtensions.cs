using System.Security.Claims;

namespace NewsSite.BL.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static int? GetCurrentUserId(this ClaimsPrincipal principal, HttpRequest request, ClaimsPrincipal user)
        {
            // First try to get from claims
            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                return userId;
            }

            // Try alternative claim types
            userIdClaim = principal.FindFirst("user_id") ?? principal.FindFirst("sub");
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out userId))
            {
                return userId;
            }

            return null;
        }

        public static int? GetUserId(this ClaimsPrincipal principal)
        {
            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                return userId;
            }

            // Try alternative claim types
            userIdClaim = principal.FindFirst("user_id") ?? principal.FindFirst("sub");
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out userId))
            {
                return userId;
            }

            return null;
        }

        public static string? GetUserEmail(this ClaimsPrincipal principal)
        {
            return principal.FindFirst(ClaimTypes.Email)?.Value;
        }

        public static string? GetUserName(this ClaimsPrincipal principal)
        {
            return principal.FindFirst(ClaimTypes.Name)?.Value;
        }

        public static bool IsAdmin(this ClaimsPrincipal principal)
        {
            return principal.IsInRole("Admin") || 
                   principal.HasClaim("role", "Admin") ||
                   principal.HasClaim(ClaimTypes.Role, "Admin");
        }
    }
}
