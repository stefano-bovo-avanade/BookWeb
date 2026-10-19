using BookWeb.Service.Context;
using BookWeb.Utility;
using Microsoft.EntityFrameworkCore;

namespace BookWeb.Registrations;

public static class DatabaseRegistration
{
    public static IServiceCollection AddDatabase(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSingleton<DapperContext>();

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection")));

        services.Configure<StripeSettings>(
            configuration.GetSection("Stripe"));

        return services;
    }
}