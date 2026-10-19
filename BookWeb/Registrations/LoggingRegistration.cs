using Serilog;

namespace BookWeb.Registrations;

public static class LoggingRegistration
{
    public static IHostBuilder AddLoggingConfiguration(
        this IHostBuilder hostBuilder)
    {
        return hostBuilder.UseSerilog((context, configuration) =>
            configuration.ReadFrom.Configuration(context.Configuration));
    }
}