using BookWeb.Repository.Repository;
using BookWeb.Repository.Repository.IRepository;
using BookWeb.Service.Context;
using BookWeb.Service.Service;
using BookWeb.Service.Service.IService;
using BookWeb.Utility;
using BookWeb.Utility.Authentication;
using BookWeb.Utility.ShoppingCartHelper;
using Mapster;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Stripe;
using System.Security.Claims;
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

            var jwtSettings = builder.Configuration.GetSection("Jwt");
            var jwtSecret = jwtSettings["Secret"];
            var jwtIssuer = jwtSettings["Issuer"];
            var jwtAudience = jwtSettings["Audience"];

            if (string.IsNullOrWhiteSpace(jwtIssuer))
            {
                throw new Exception("Jwt:Issuer è null");
            }

            if (string.IsNullOrWhiteSpace(jwtAudience))
            {
                throw new Exception("Jwt:Audience è null");
            }

            if (string.IsNullOrWhiteSpace(jwtSecret))
            {
                throw new Exception("JWT_SECRET è null");
            }

            var keyBytes = Encoding.UTF8.GetBytes(jwtSecret);

            var signingKey = new SymmetricSecurityKey(keyBytes);

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,

                ValidIssuer = jwtIssuer,
                ValidAudience = jwtAudience,
                IssuerSigningKey = signingKey
            };

            builder.Services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme =
                        JwtBearerDefaults.AuthenticationScheme;

                    options.DefaultChallengeScheme =
                        JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            context.Token = context.Request.Cookies["jwt"];
                            return Task.CompletedTask;
                        }
                    };

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,

                        ValidIssuer = jwtIssuer,
                        ValidAudience = jwtAudience,

                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(jwtSecret))
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
            builder.Services.AddScoped<AuthService>();
            builder.Services.AddScoped<IEmailSender, EmailSender>();
            builder.Services.AddMapster();
            MapsterConfig.RegisterMappings();

            // 3. Registrazione dei servizi
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IOrderService, OrderService>();
            builder.Services.AddScoped<IOrderDetailService, OrderDetailService>();
            builder.Services.AddScoped<IShoppingCartService, ShoppingCartService>();
            builder.Services.AddScoped<CartHelper>();
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

            app.Run();
        }

    }
}
