using Prometheus;

namespace OrderService.Extensions;

public static class MetricsExtensions
{
    public static IApplicationBuilder UseCustomMetrics(this IApplicationBuilder app)
    {
        app.UseHttpMetrics();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapMetrics("/metrics");
        });

        return app;
    }
}
