using API.Data;
using Microsoft.EntityFrameworkCore;

namespace DatingApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            builder.Services.AddCors();

            builder.Services.AddDbContext<DataContext>(options =>
            {
                options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"));
            });

            var app = builder.Build();

            app.UseCors(policy =>
            {
                policy.AllowAnyHeader();
                policy.AllowAnyMethod();
                policy.WithOrigins("http://localhost:4200", "https://localhost:4200");
            });
            app.MapControllers();

            app.Run();
        }
    }
}
