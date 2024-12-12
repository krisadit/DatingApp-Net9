using System.Security.Claims;

namespace API.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static string GetUsername(this ClaimsPrincipal user)
        {
            string? username = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new Exception("Username not detected");
            return username;
        }
    }
}
