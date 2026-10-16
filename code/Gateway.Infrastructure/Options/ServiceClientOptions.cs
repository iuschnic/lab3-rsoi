namespace Gateway.Infrastructure.Options;

public class ServiceClientOptions
{
    public string BaseUrl { get; init; } = string.Empty;
    public int FailureThreshold { get; init; } = 3;
    public int BreakSeconds { get; init; } = 5;
    public int TimeoutSeconds { get; init; } = 5;
    public int SamplingSeconds { get; init; } = 30;
}