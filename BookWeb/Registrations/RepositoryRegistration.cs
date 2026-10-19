using BookWeb.Repository.Repository;
using BookWeb.Repository.Repository.IRepository;

namespace BookWeb.Registrations;

public static class RepositoryRegistration
{
    public static IServiceCollection AddRepositories(
        this IServiceCollection services)
    {
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<ICompanyRepository, CompanyRepository>();
        services.AddScoped<IShoppingCartRepository, ShoppingCartRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IOrderDetailRepository, OrderDetailRepository>();
        services.AddScoped<IUserRepository, UserRepository>();

        return services;
    }
}