using Mapster;
using MapsterConfig = BookWeb.Areas.Mapster.MapsterConfig;

namespace BookWeb.Registrations;

public static class MappingRegistration
{
    public static IServiceCollection AddMapping(
        this IServiceCollection services)
    {
        services.AddMapster();

        MapsterConfig.RegisterMappings();

        return services;
    }
}