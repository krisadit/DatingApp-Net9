using System.Security.Claims;

namespace API.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static string GetUsername(this ClaimsPrincipal user)
        {
            string? username = user.FindFirstValue(ClaimTypes.Name) ?? throw new Exception("Username not detected");
            return username;
        }

        public static int GetUserId(this ClaimsPrincipal user)
        {
            int userId =  int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new Exception("UserId not detected"));
            return userId;
        }
    }
}
