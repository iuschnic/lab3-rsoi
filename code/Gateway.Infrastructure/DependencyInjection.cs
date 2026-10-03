using Gateway.Application.OutPorts;
using Gateway.Infrastructure.Cars;
using Gateway.Infrastructure.Payments;
using Gateway.Infrastructure.Rentals;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Gateway.Infrastructure;

public static class DependencyInjection
{
    public const string ServicesSection = "Services";

    public static IServiceCollection AddServiceClients(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var section = configuration.GetSection(ServicesSection);

        services.AddHttpClient<ICarsService, CarsHttpClient>(client =>
            client.BaseAddress = GetBaseAddress(section, "Cars"));
        services.AddHttpClient<IPaymentsService, PaymentsHttpClient>(client =>
            client.BaseAddress = GetBaseAddress(section, "Payments"));
        services.AddHttpClient<IRentalsService, RentalsHttpClient>(client =>
            client.BaseAddress = GetBaseAddress(section, "Rentals"));

        return services;
    }

    private static Uri GetBaseAddress(IConfigurationSection section, string serviceName)
    {
        var url = section[serviceName]
            ?? throw new InvalidOperationException($"Base url for {serviceName} service is not configured ({ServicesSection}:{serviceName})");

        return new Uri(url.EndsWith('/') ? url : url + "/");
    }
}
