using BookWeb.Models.Models;
using Mapster;

namespace BookWeb.Areas.Mapster
{
    public static class MapsterConfig
    {
        public static void RegisterMappings()
        {
            TypeAdapterConfig<User, OrderHeader>
                .NewConfig()
                .Map(dest => dest.UserId, src => src.Id)
                .Map(dest => dest.PhoneNumber, src => src.PhoneNumber)
                .Map(dest => dest.Name, src => src.Name)
                .Map(dest => dest.StreetAddress, src => src.StreetAddress)
                .Map(dest => dest.City, src => src.City)
                .Map(dest => dest.State, src => src.State)
                .Map(dest => dest.PostalCode, src => src.PostalCode)
                .IgnoreNonMapped(true); // Ignora tutte le altre proprietà
        }
    }
}
