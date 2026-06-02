using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace InternshipPortal.API.Helpers
{
    public static class ClaimsPrincipalHelper
    {
        public static Guid GetCurrentUserId(this ClaimsPrincipal user)
        {
            var userId =
                user.FindFirstValue(ClaimTypes.NameIdentifier) ??
                user.FindFirstValue(JwtRegisteredClaimNames.Sub) ??
                user.FindFirstValue("sub") ??
                user.FindFirstValue("nameid") ??
                user.FindFirstValue("uid");

            if (!Guid.TryParse(userId, out var parsedUserId))
            {
                throw new UnauthorizedAccessException("Invalid or missing user id claim. Please log in again.");
            }

            return parsedUserId;
        }
    }
}
