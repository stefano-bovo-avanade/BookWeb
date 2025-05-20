using BookWeb.Models.Models;
using Mapster;

namespace BookWeb.Common.Mapster
{
    public static class MapsterConfig
    {
        public static void RegisterMappings()
        {
            TypeAdapterConfig<InputUser, User>
                .NewConfig()
                .Ignore(dest => dest.Id)
                .Ignore(dest => dest.PasswordHash); // hash lo gestirai separatamente
        }
    }
}
