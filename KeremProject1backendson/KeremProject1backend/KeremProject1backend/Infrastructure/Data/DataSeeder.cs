using KeremProject1backend.Core.Helpers;
using KeremProject1backend.Infrastructure;
using KeremProject1backend.Models.DBs;
using KeremProject1backend.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace KeremProject1backend.Infrastructure.Data;

public static class DataSeeder
{
    public static async Task SeedAsync(WebApplication app)
    {
        using (var scope = app.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationContext>();

            // Ensure Database is Created & Migrated
            await context.Database.MigrateAsync();

            // Check if any admin exists
            if (!await context.Users.AnyAsync(u => u.GlobalRole == UserRole.AdminAdmin))
            {
                // Create Default Admin
                PasswordHelper.CreateHash("Admin123!", out byte[] hash, out byte[] salt);

                var admin = new User
                {
                    FullName = "System Admin",
                    Username = "admin",
                    Email = "admin@karneproject.com",
                    GlobalRole = UserRole.AdminAdmin,
                    Status = UserStatus.Active,
                    PasswordHash = hash,
                    PasswordSalt = salt,
                    CreatedAt = DateTime.UtcNow,
                    ProfileVisibility = ProfileVisibility.Private
                };

                context.Users.Add(admin);
                await context.SaveChangesAsync();

                Console.WriteLine("--> Default Admin User Seeded: admin / Admin123!");
            }
        }
    }
}
