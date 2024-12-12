using API.Entities;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace API.Data
{
    public class Seed // Temporary database seeder for test data
    {
        public static async Task SeedUsers(DataContext context)
        {
            if (await context.Users.AnyAsync())
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
            foreach (var user in users)
            {
                using HMACSHA512 hmac = new();

                user.UserName = user.UserName.ToLower();
                user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes("Pa$$w0rd"));
                user.PassswordSalt = hmac.Key;
                context.Users.Add(user);
            }

            await context.SaveChangesAsync();
        }
    }
}
