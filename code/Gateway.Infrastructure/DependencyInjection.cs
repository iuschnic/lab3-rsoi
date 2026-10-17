using Gateway.Application.OutPorts;
using Gateway.Infrastructure.Cars;
using Gateway.Infrastructure.Options;
using Gateway.Infrastructure.Payments;
using Gateway.Infrastructure.Rentals;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Polly;
using Polly.Timeout;
using System.Net;

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



        // Cars Read: Circuit Breaker + Timeout
        services.AddHttpClient<ICarsReadService, CarsReadHttpClient>(client =>
        {
            client.BaseAddress = ToBaseAddress(carsOptions);
        })
        .AddResilienceHandler("CarsRead", (builder, _) =>
        {
            ConfigureResilience(builder, carsOptions);
        });

        // Cars Write: Timeout
        services.AddHttpClient<ICarsWriteService, CarsWriteHttpClient>(client =>
        {
            client.BaseAddress = ToBaseAddress(carsOptions);
        })
        .AddResilienceHandler("CarsWrite", (builder, _) =>
        {
            builder.AddTimeout(
                TimeSpan.FromSeconds(carsOptions.TimeoutSeconds));
        });



        // Payments Read: Circuit Breaker + Timeout
        services.AddHttpClient<IPaymentsReadService, PaymentsReadHttpClient>(client =>
        {
            client.BaseAddress = ToBaseAddress(paymentsOptions);
        })
        .AddResilienceHandler("PaymentsRead", (builder, _) =>
        {
            ConfigureResilience(builder, paymentsOptions);
        });

        // Payments Write: Timeout
        services.AddHttpClient<IPaymentsWriteService, PaymentsWriteHttpClient>(client =>
        {
            client.BaseAddress = ToBaseAddress(paymentsOptions);
        })
        .AddResilienceHandler("PaymentsWrite", (builder, _) =>
        {
            builder.AddTimeout(
                TimeSpan.FromSeconds(paymentsOptions.TimeoutSeconds));
        });



        // Rentals Read: Circuit Breaker + Timeout
        services.AddHttpClient<IRentalsReadService, RentalsReadHttpClient>(client =>
        {
            client.BaseAddress = ToBaseAddress(rentalsOptions);
        })
        .AddResilienceHandler("RentalsRead", (builder, _) =>
        {
            ConfigureResilience(builder, rentalsOptions);
        });

        // Rentals Write: Timeout
        services.AddHttpClient<IRentalsWriteService, RentalsWriteHttpClient>(client =>
        {
            client.BaseAddress = ToBaseAddress(rentalsOptions);
        })
        .AddResilienceHandler("RentalsWrite", (builder, _) =>
        {
            builder.AddTimeout(
                TimeSpan.FromSeconds(rentalsOptions.TimeoutSeconds));
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
            SamplingDuration =
                TimeSpan.FromSeconds(options.SamplingSeconds),
            BreakDuration =
                TimeSpan.FromSeconds(options.BreakSeconds),
            ShouldHandle = static args =>
            {
                // Сетевые ошибки
                if (args.Outcome.Exception is HttpRequestException)
                    return ValueTask.FromResult(true);

                // Таймаут запроса
                if (args.Outcome.Exception is TimeoutRejectedException)
                    return ValueTask.FromResult(true);

                // Ошибки сервера и Request Timeout
                if (args.Outcome.Result is { } response &&
                    ((int)response.StatusCode >= 500 ||
                     response.StatusCode == HttpStatusCode.RequestTimeout))
                {
                    return ValueTask.FromResult(true);
                }

                return ValueTask.FromResult(false);
            }
        });

        builder.AddTimeout(
            TimeSpan.FromSeconds(options.TimeoutSeconds));
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

    private static Uri ToBaseAddress(ServiceClientOptions options)
    {
        return new Uri(options.BaseUrl.EndsWith('/') ? options.BaseUrl : options.BaseUrl + "/");
    }
}
