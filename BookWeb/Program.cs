using BookWeb.Models.Models;
using BookWeb.Repository.Repository;
using BookWeb.Repository.Repository.IRepository;
using BookWeb.Service.Context;
using BookWeb.Service.Service;
using BookWeb.Service.Service.IService;
using BookWeb.Utility;
using BookWeb.Utility.Authentication;
using BookWeb.Utility.Authorization;
using BookWeb.Utility.ShoppingCartHelper;
using Mapster;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Stripe;
using System.Text;
using MapsterConfig = BookWeb.Areas.Mapster.MapsterConfig;

namespace BookWeb
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var stripeSecretKey = Environment.GetEnvironmentVariable("STRIPE_SECRET_KEY");
            StripeConfiguration.ApiKey = stripeSecretKey;
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllersWithViews()
            .AddMvcOptions(options =>
            {
                options.RespectBrowserAcceptHeader = true;
            });

            // sicurezza JWT
            builder.Services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,

                    ValidIssuer = builder.Configuration["Jwt:Issuer"],
                    ValidAudience = builder.Configuration["Jwt:Audience"],

                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(
                            builder.Configuration["Jwt:SecretKey"]!
                        )
                    ),

                    RoleClaimType = System.Security.Claims.ClaimTypes.Role,
                    NameClaimType = System.Security.Claims.ClaimTypes.NameIdentifier
                };

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        context.Token = context.Request.Cookies["jwt"];
                        return Task.CompletedTask;
                    }
                };
            });


            // end sicurezza JWT

            builder.Services.AddAuthorization();
            builder.Services.AddRazorPages();

            // 1. Configurazione del DbContext
            builder.Services.AddSingleton<DapperContext>();
            builder.Services.AddDbContext<ApplicationDbContext>(options => 
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
            builder.Services.Configure<StripeSettings>(builder.Configuration.GetSection("Stripe"));


            // 2. Registrazione dei repository
            builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
            builder.Services.AddScoped<IProductRepository, ProductRepository>();
            builder.Services.AddScoped<ICompanyRepository, CompanyRepository>();
            builder.Services.AddScoped<IShoppingCartRepository, ShoppingCartRepository>();
            builder.Services.AddScoped<IOrderRepository, OrderRepository>();
            builder.Services.AddScoped<IOrderDetailRepository, OrderDetailRepository>();
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IEmailSender, EmailSender>();
            builder.Services.AddMapster();
            MapsterConfig.RegisterMappings();

            // 3. Registrazione dei servizi
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IOrderService, OrderService>();
            builder.Services.AddScoped<IOrderDetailService, OrderDetailService>();
            builder.Services.AddScoped<IShoppingCartService, ShoppingCartService>();
            builder.Services.AddScoped<CartHelper>();
            builder.Services.AddScoped<UserAuthorizationService>();
            builder.Services.AddScoped<JwtService>();
            builder.Services.AddHttpContextAccessor();

            builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseAuthentication(); // se vuoi abilitare auth anche fuori dalle API
            app.UseAuthorization();

            app.MapControllerRoute(
            name: "default",
            pattern: "{area=Customer}/{controller=Home}/{action=Index}/{id?}");

            app.MapRazorPages();


            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;

                var context = services.GetRequiredService<ApplicationDbContext>();

                var passwordHasher = new PasswordHasher<User>();

                const string username = "admin";

                var adminEsistente = context.Users
                    .FirstOrDefault(u => u.UserName == username);

                if (adminEsistente == null)
                {
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
                        "Admin123!"
                    );

                    context.Users.Add(admin);

                    context.UserRoles.Add(new UserRole
                    {
                        UserId = admin.Id,
                        RoleId = adminRole.Id
                    });

                    context.SaveChanges();
                }
            }


            app.Run();
        }

    }
}
