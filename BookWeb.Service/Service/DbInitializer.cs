using BookWeb.Models.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using BookWeb.Service.Context;

namespace BookWeb.Service.Service
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(
            ApplicationDbContext context,
            IPasswordHasher<User> passwordHasher)
        {
            // Assicuriamoci che il database sia aggiornato
            await context.Database.MigrateAsync();

            // Cerchiamo il ruolo Admin
            var adminRole = await context.Roles
                .SingleOrDefaultAsync(r => r.Name == "Admin");

            if (adminRole == null)
            {
                throw new Exception("Il ruolo Admin non esiste.");
            }

            // Controlliamo se esiste già un Admin
            var adminExists = await context.Users
                .AnyAsync(u => u.RoleId == adminRole.Id);

            if (adminExists)
            {
                return;
            }

            // Creiamo l'utente Admin
            var admin = new User
            {
                Id = Guid.NewGuid().ToString(),
                UserName = "admin",
                Name = "Administrator",
                Email = "admin@bookweb.local",
                PhoneNumber = "0000000000",

                RoleId = adminRole.Id,

                CompanyId = null
            };

            // Hash della password
            admin.PasswordHash = passwordHasher.HashPassword(
                admin,
                "Admin123!"
            );

            context.Users.Add(admin);

            await context.SaveChangesAsync();
        }
    }
}