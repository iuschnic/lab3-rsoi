using System.Text.Json.Serialization;
using Gateway.Api.Controllers;
using Gateway.Application.InPorts;
using Gateway.Application.Services;
using Gateway.Infrastructure;
using Gateway.Api.BackgroundServices;
using Gateway.Application.OutPorts;
using Gateway.Infrastructure.DeferredOperations;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(o =>
        o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();
builder.Services.AddSingleton(Log.Logger);

builder.Services.AddServiceClients(builder.Configuration);

builder.Services.AddSingleton<InMemoryDeferredOperationQueue>();
builder.Services.AddSingleton<IDeferredOperationScheduler>(sp =>
    sp.GetRequiredService<InMemoryDeferredOperationQueue>());
builder.Services.AddSingleton<IDeferredOperationSource>(sp =>
    sp.GetRequiredService<InMemoryDeferredOperationQueue>());
builder.Services.AddScoped<CarRentalService>();
builder.Services.AddScoped<ICarRentalService>(sp => sp.GetRequiredService<CarRentalService>());
builder.Services.AddScoped<IDeferredOperationService>(sp => sp.GetRequiredService<CarRentalService>());
builder.Services.AddHostedService<DeferredOperationWorker>();

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseSerilogRequestLogging();
app.UseSwagger(options =>
{
    options.RouteTemplate = "api/swagger/{documentName}/swagger.json";
});
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/api/swagger/v1/swagger.json", "API V1");
    options.RoutePrefix = "api/swagger";
});
app.MapControllers();
app.MapGet("manage/health", () => Results.Ok("Healthy"));
app.Run();
