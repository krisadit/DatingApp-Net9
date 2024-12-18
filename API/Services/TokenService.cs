using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace API.Services
{
    public class TokenService(IConfiguration config, UserManager<AppUser> userManager) : ITokenService
    {
        public async Task<string> CreateToken(AppUser user)
        {
            string tokenKey = config["TokenKey"] ?? throw new Exception("TokenKey not found in App settings");
            if (tokenKey.Length < 64)
            {
                throw new Exception("TokenKey needs to be at least 64 characters long");
            }

            SymmetricSecurityKey key = new(Encoding.UTF8.GetBytes(tokenKey));

            if (user.UserName == null)
            {
                throw new Exception("No username for user");
            }

            List<Claim> claims = [
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, user.UserName),
            ];

            var roles = await userManager.GetRolesAsync(user);

            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            SigningCredentials creds = new(key, SecurityAlgorithms.HmacSha512Signature);

            SecurityTokenDescriptor descriptor = new()
            {
                Subject = new(claims),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = creds
            };

            JwtSecurityTokenHandler handler = new();
            SecurityToken token = handler.CreateToken(descriptor);
            return handler.WriteToken(token);
        }
    }
}
