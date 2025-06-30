// Arquivo: Configurations/OpenTelemetrySettings.cs
namespace OrderService.Configurations;

public class OpenTelemetrySettings
{
    public string JaegerEndpoint { get; set; } = string.Empty;
}
