using API.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace API.Data
{
    public class Seed // Temporary database seeder for test data
    {
        public static async Task SeedUsers(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager)
        {
            if (await userManager.Users.AnyAsync())
            {
                return;
            }
            string? userData = await File.ReadAllTextAsync("Data/UserSeedData.json");

            JsonSerializerOptions options = new()
            {
                PropertyNameCaseInsensitive = true,
            };

            List<AppUser>? users = JsonSerializer.Deserialize<List<AppUser>>(userData);

            if (users == null)
            {
                return;
            }

            List<AppRole> roles = [
                new AppRole() { Name = "Member"},
                new AppRole() { Name = "Admin" },
                new AppRole() { Name = "Moderator" },
            ];

            foreach (AppRole role in roles)
            {
                await roleManager.CreateAsync(role);
            }

            foreach (var user in users)
            {
                user.UserName = user.UserName!.ToLower();
                user.Photos.First().IsApproved = true;
                await userManager.CreateAsync(user, "Pa$$w0rd"); // Probably needs to be randomized everytime?
                await userManager.AddToRoleAsync(user, "Member");
            }

            AppUser admin = new()
            {
                UserName = "admin",
                KnownAs = "Admin",
                Gender = "",
                City = "",
                Country = ""
            };

            await userManager.CreateAsync(admin, "Pa$$w0rd");
            await userManager.AddToRolesAsync(admin, ["Admin", "Moderator"]);
        }
    }
}
