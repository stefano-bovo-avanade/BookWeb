using BookWeb.Models.Models;
using BookWeb.Registrations;
using BookWeb.Service.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.OpenApi;
using Stripe;

namespace BookWeb;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Host.AddLoggingConfiguration();

        // Configurazione Stripe
        StripeConfiguration.ApiKey =
            Environment.GetEnvironmentVariable("STRIPE_SECRET_KEY");

        // Registrazione dei servizi
        builder.Services.AddControllersWithViews()
        .AddMvcOptions(options =>
        {
            options.RespectBrowserAcceptHeader = true;
        });

        builder.Services.AddRazorPages();
        builder.Services.AddHttpContextAccessor();

        builder.Services.AddSwaggerConfiguration();

        // Registrazioni personalizzate
        builder.Services.AddDatabase(builder.Configuration);
        builder.Services.AddRepositories();
        builder.Services.AddApplicationServices();
        builder.Services.AddJwtAuthentication(builder.Configuration);
        builder.Services.AddMapping();

        var app = builder.Build();

        // Configurazione Swagger
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();

            app.UseSwaggerUI();
        }

        // Configurazione della pipeline HTTP
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();

        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        // Routing MVC
        app.MapControllerRoute(
            name: "default",
            pattern: "{area=Customer}/{controller=Home}/{action=Index}/{id?}");

        app.MapRazorPages();

        AdminSeeder.SeedAdmin(app.Services);

        app.Run();
    }
}
