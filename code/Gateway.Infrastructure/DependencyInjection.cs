using Gateway.Application.OutPorts;
using Gateway.Infrastructure.Cars;
using Gateway.Infrastructure.Payments;
using Gateway.Infrastructure.Rentals;
using Gateway.Infrastructure.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Polly;
using Polly.Timeout;

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
        {
            var options = GetServiceClientOptions(section, "Cars");
            client.BaseAddress = GetBaseAddress(section, "Cars");
        })
        .AddResilienceHandler("CarsResilience", (builder, context) =>
        {
            var options = GetServiceClientOptions(section, "Cars");
            ConfigureResilience(builder, options);
        });

        services.AddHttpClient<IPaymentsService, PaymentsHttpClient>(client =>
        {
            var options = GetServiceClientOptions(section, "Payments");
            client.BaseAddress = GetBaseAddress(section, "Payments");
        })
        .AddResilienceHandler("PaymentsResilience", (builder, context) =>
        {
            var options = GetServiceClientOptions(section, "Payments");
            ConfigureResilience(builder, options);
        });

        services.AddHttpClient<IRentalsService, RentalsHttpClient>(client =>
        {
            var options = GetServiceClientOptions(section, "Rentals");
            client.BaseAddress = GetBaseAddress(section, "Rentals");
        })
        .AddResilienceHandler("RentalsResilience", (builder, context) =>
        {
            var options = GetServiceClientOptions(section, "Rentals");
            ConfigureResilience(builder, options);
        });

        return services;
    }

    private static void ConfigureResilience(
        ResiliencePipelineBuilder<HttpResponseMessage> builder,
        ServiceClientOptions options)
    {
        builder.AddCircuitBreaker(new HttpCircuitBreakerStrategyOptions
        {
            MinimumThroughput = options.FailureThreshold,
            SamplingDuration = TimeSpan.FromSeconds(options.SamplingSeconds),
            BreakDuration = TimeSpan.FromSeconds(options.BreakSeconds),
            ShouldHandle = static args =>
            {
                // Сетевые ошибки и таймауты
                if (args.Outcome.Exception is HttpRequestException or TimeoutRejectedException)
                    return ValueTask.FromResult(true);

                // 5xx и 408 — считаем сбоем
                if (args.Outcome.Result is { } response &&
                    ((int)response.StatusCode >= 500 || response.StatusCode == System.Net.HttpStatusCode.RequestTimeout))
                    return ValueTask.FromResult(true);
                return ValueTask.FromResult(false);
            }
        });
        builder.AddTimeout(TimeSpan.FromSeconds(options.TimeoutSeconds));
    }

    private static ServiceClientOptions GetServiceClientOptions(IConfigurationSection section, string serviceName)
    {
        var options = section.GetSection(serviceName).Get<ServiceClientOptions>()
            ?? throw new InvalidOperationException($"Service client options for {serviceName} " +
            $"service are not configured ({ServicesSection}:{serviceName})");
        return options;
    }

    private static Uri GetBaseAddress(IConfigurationSection section, string serviceName)
    {
        var url = section[serviceName]
            ?? throw new InvalidOperationException($"Base url for {serviceName} service is not configured ({ServicesSection}:{serviceName})");

        return new Uri(url.EndsWith('/') ? url : url + "/");
    }
}
