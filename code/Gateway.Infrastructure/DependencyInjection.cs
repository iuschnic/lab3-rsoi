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

        var carsOptions = GetServiceClientOptions(section, "Cars");
        var rentalsOptions = GetServiceClientOptions(section, "Rentals");
        var paymentsOptions = GetServiceClientOptions(section, "Payments");

        services.AddHttpClient<ICarsReadService, CarsReadHttpClient>(client =>
        {
            client.BaseAddress = ToBaseAddress(carsOptions, "Cars");
        })
        .AddResilienceHandler("CarsRead", (builder, context) =>
        {
            ConfigureResilience(builder, carsOptions);
        });

        services.AddHttpClient<ICarsWriteService, CarsWriteHttpClient>(client =>
        {
            client.BaseAddress = ToBaseAddress(carsOptions, "Cars");
        })
        .AddResilienceHandler("CarsWrite", (builder, _) =>
        {
            builder.AddTimeout(TimeSpan.FromSeconds(carsOptions.TimeoutSeconds));
        });



        services.AddHttpClient<IPaymentsReadService, PaymentsReadHttpClient>(client =>
        {
            client.BaseAddress = ToBaseAddress(paymentsOptions, "Payments");
        })
        .AddResilienceHandler("PaymentsRead", (builder, context) =>
        {
            ConfigureResilience(builder, paymentsOptions);
        });

        services.AddHttpClient<IPaymentsWriteService, PaymentsWriteHttpClient>(client =>
        {
            client.BaseAddress = ToBaseAddress(paymentsOptions, "Payments");
        })
        .AddResilienceHandler("PaymentsWrite", (builder, _) =>
        {
            builder.AddTimeout(TimeSpan.FromSeconds(paymentsOptions.TimeoutSeconds));
        });
        


        services.AddHttpClient<IRentalsReadService, RentalsReadHttpClient>(client =>
        {
            client.BaseAddress = ToBaseAddress(rentalsOptions, "Rentals");
        })
        .AddResilienceHandler("RentalsRead", (builder, context) =>
        {
            ConfigureResilience(builder, rentalsOptions);
        });

        services.AddHttpClient<IRentalsWriteService, RentalsWriteHttpClient>(client =>
        {
            client.BaseAddress = ToBaseAddress(rentalsOptions, "Rentals");
        })
        .AddResilienceHandler("RentalsWrite", (builder, _) =>
        {
            builder.AddTimeout(TimeSpan.FromSeconds(rentalsOptions.TimeoutSeconds));
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
        if (string.IsNullOrWhiteSpace(options.BaseUrl))
            throw new InvalidOperationException(
                $"Base url for {serviceName} is not configured");
        return options;
    }

    private static Uri ToBaseAddress(ServiceClientOptions options, string serviceName)
    {
        return new Uri(options.BaseUrl.EndsWith('/') ? options.BaseUrl : options.BaseUrl + "/");
    }
}
