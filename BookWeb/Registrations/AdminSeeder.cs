using BookWeb.Models.Models;
using BookWeb.Service.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BookWeb.Registrations;

public static class AdminSeeder
{
    public static void SeedAdmin(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();

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