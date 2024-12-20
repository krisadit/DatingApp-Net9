using API.Data;
using API.Entities;
using API.Errors;
using API.Extensions;
using API.SignalR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DatingApp
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddApplicationServices(builder.Configuration);
            builder.Services.AddIdentityServices(builder.Configuration);

            var app = builder.Build();
            app.UseMiddleware<ExceptionMiddleware>();
            app.UseCors(policy =>
            {
                policy.AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials()
                    .WithOrigins("http://localhost:4200", "https://localhost:4200");
            });

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseDefaultFiles();
            app.UseStaticFiles();

            app.MapControllers();
            app.MapHub<PresenceHub>("hubs/presence");
            app.MapHub<MessageHub>("hubs/message");

            // We need to access a service locator to get ahold of a service outside of
            // dependency injection so that it can be disposed of immediately.
            using IServiceScope scope = app.Services.CreateScope();
            var services = scope.ServiceProvider;
            try
            {
                var context = services.GetRequiredService<DataContext>();
                var userManager = services.GetRequiredService<UserManager<AppUser>>();
                var roleManager = services.GetRequiredService<RoleManager<AppRole>>();
                await context.Database.MigrateAsync();
                await context.Database.ExecuteSqlRawAsync("DELETE FROM [Connections]"); // SQLite/SQLServer specific command
                // await context.Database.ExecuteSqlRawAsync("DELETE FROM \"Groups\""); // PostgreSQL example
                await Seed.SeedUsers(userManager, roleManager);
            }
            catch (Exception ex)
            {
                var logger = services.GetRequiredService<ILogger<Program>>();
                logger.LogError(ex, "An error occurred during migration");
            }
            app.Run();
        }
    }
}
