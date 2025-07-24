using OrderService.Configurations;

namespace OrderService.Extensions;

public static class EventDeliveryExtensions
{
    public static IServiceCollection AddEventDelivery(this IServiceCollection services, IConfiguration configuration)
    {
        var eventDeliverySettings = configuration.GetSection("EventDeliverySettings").Get<EventDeliverySettings>() ?? new EventDeliverySettings();

        var strategy = Environment.GetEnvironmentVariable("EVENTDELIVERY__STRATEGY") ?? string.Empty;
        if (strategy != null)
            eventDeliverySettings.Strategy = strategy;

        var publishMechanism = Environment.GetEnvironmentVariable("EVENTDELIVERY__PUBLISHMECHANISM") ?? string.Empty;
        if (publishMechanism != null)
            eventDeliverySettings.PublishMechanism = publishMechanism;

        Console.WriteLine($"✅ EventDelivery.Strategy => {eventDeliverySettings.Strategy}");
        Console.WriteLine($"✅ EventDelivery.PublishMechanism => {eventDeliverySettings.PublishMechanism}");

        services.AddSingleton(eventDeliverySettings);

        return services;
    }
}
