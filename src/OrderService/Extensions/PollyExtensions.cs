using OrderService.Policies;
using Polly;

namespace OrderService.Extensions;

public static class PollyExtensions
{
    public static IServiceCollection AddCustomPolly(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpClient("ExternalServiceClient", client =>
        {
            client.BaseAddress = new Uri("http://orderservice");
        })
        .AddPolicyHandler(PollyPolicies.GetRetryPolicy())
        .AddPolicyHandler(PollyPolicies.GetCircuitBreakerPolicy())
        .AddPolicyHandler(PollyPolicies.GetTimeoutPolicy());

        return services;
    }
}
