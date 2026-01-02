using KeremProject1backend.Models.DBs;
using Microsoft.EntityFrameworkCore;

namespace KeremProject1backend.Services
{
    public static class ServiceRegistration
    {
        public static void AddAppServices(this IServiceCollection services, IConfiguration config)
        {
            // Veritabanı
            services.AddDbContext<ApplicationContext>(options =>
                options.UseSqlServer(config.GetConnectionString("DefaultConnection")));

            // CORS
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAngularApp",
                    policy =>
                    {
                        policy.WithOrigins("http://localhost:4200")
                              .AllowAnyHeader()
                              .AllowAnyMethod();
                    });
            });

            // TokenService'i appsettings ile başlat
            TokenServices.Initialize(config);
        }
    }
}
