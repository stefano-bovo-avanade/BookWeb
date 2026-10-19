using BookWeb.Service.Service;
using BookWeb.Service.Service.IService;
using BookWeb.Utility.Authentication;
using BookWeb.Utility.Authorization;
using BookWeb.Utility.ShoppingCartHelper;

namespace BookWeb.Registrations;

public static class ServiceRegistration
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services)
    {
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IOrderDetailService, OrderDetailService>();
        services.AddScoped<IShoppingCartService, ShoppingCartService>();

        services.AddScoped<CartHelper>();
        services.AddScoped<UserAuthorizationService>();
        services.AddScoped<JwtService>();

        services.AddScoped<Microsoft.AspNetCore.Identity.UI.Services.IEmailSender, BookWeb.Utility.EmailSender>();

        //services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

        return services;
    }
}