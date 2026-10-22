using Microsoft.OpenApi;

namespace BookWeb.Registrations;

public static class SwaggerRegistration
{
    public static IServiceCollection AddSwaggerConfiguration(
        this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();

        services.AddSwaggerGen(options =>
        {
            // Informazioni API
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "BookWeb API",
                Version = "v1",
                Description = "API REST per la gestione della libreria BookWeb"
            });

            // Configurazione autenticazione JWT Bearer
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Inserisci il token JWT per autenticarti."
            });

            // Applica il requisito di sicurezza alle operazioni documentate
            options.AddSecurityRequirement(document =>
                new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecuritySchemeReference(
                            "Bearer",
                            document),
                        new List<string>()
                    }
                });
        });

        return services;
    }

    public static WebApplication UseSwaggerConfiguration(
        this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();

            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint(
                    "/swagger/v1/swagger.json",
                    "BookWeb API v1");

                options.RoutePrefix = "swagger";
            });
        }

        return app;
    }
}
