using API.Data;
using API.Errors;
using API.Extensions;
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
                policy.AllowAnyHeader();
                policy.AllowAnyMethod();
                policy.WithOrigins("http://localhost:4200", "https://localhost:4200");
            });

            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();

            // We need to access a service locator to get ahold of a service outside of
            // dependency injection so that it can be disposed of immediately.
            using IServiceScope scope = app.Services.CreateScope();
            var services = scope.ServiceProvider;
            try
            {
                var context = services.GetRequiredService<DataContext>();
                await context.Database.MigrateAsync();
                await Seed.SeedUsers(context);
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
