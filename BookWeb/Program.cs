using BookWeb.Models.Models;
using BookWeb.Registrations;
using BookWeb.Service.Context;
using Microsoft.AspNetCore.Identity;
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


        // Registrazioni personalizzate
        builder.Services.AddDatabase(builder.Configuration);
        builder.Services.AddRepositories();
        builder.Services.AddApplicationServices();
        builder.Services.AddJwtAuthentication(builder.Configuration);
        builder.Services.AddMapping();

        var app = builder.Build();

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

        // Inizializzazione dell'amministratore
        SeedAdmin(app);

        app.Run();
    }

    private static void SeedAdmin(WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var services = scope.ServiceProvider;

        var context = services.GetRequiredService<ApplicationDbContext>();

        var passwordHasher = new PasswordHasher<User>();

        const string username = "admin";

        var adminEsistente = context.Users
            .FirstOrDefault(u => u.UserName == username);

        if (adminEsistente != null)
        {
            return;
        }

        var adminRole = context.Roles
            .FirstOrDefault(r => r.Name == "Admin");

        if (adminRole == null)
        {
            throw new InvalidOperationException(
                "Il ruolo Admin non esiste nel database.");
        }

        var admin = new User
        {
            Id = Guid.NewGuid().ToString(),
            UserName = username,
            Email = "admin@esempio.it",
            Name = "Administrator",
            PhoneNumber = "0000000000",
            RoleId = adminRole.Id,
            Role = adminRole
        };

        admin.PasswordHash = passwordHasher.HashPassword(
            admin,
            "Admin123!");

        context.Users.Add(admin);

        context.UserRoles.Add(new UserRole
        {
            UserId = admin.Id,
            RoleId = adminRole.Id
        });

        context.SaveChanges();
    }

}
