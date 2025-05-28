using MassTransit;
using Contracts;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using System.Diagnostics;
using System.Text;

public class OrderConsumer : IConsumer<Order>
{
    private readonly PaymentDbContext _dbContext;
    private static readonly ActivitySource ActivitySource = new("PaymentService");

    public OrderConsumer(PaymentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Consume(ConsumeContext<Order> context)
    {
        // Extrai o contexto de rastreamento dos headers MassTransit
        var parentContext = Propagators.DefaultTextMapPropagator.Extract(
            default,
            context.Headers,
            (headers, key) =>
            {
                if (headers.TryGetHeader(key, out var value))
                {
                    return value switch
                    {
                        string s => new[] { s },
                        byte[] b => new[] { Encoding.UTF8.GetString(b) },
                        _ => Array.Empty<string>()
                    };
                }
                return Array.Empty<string>();
            });

        Baggage.Current = parentContext.Baggage;

        using var activity = ActivitySource.StartActivity(
            "Consume Order",
            ActivityKind.Consumer,
            parentContext.ActivityContext);

        activity?.SetTag("message.id", context.MessageId);
        activity?.SetTag("order.id", context.Message.Id);
        activity?.SetTag("order.productName", context.Message.ProductName);
        activity?.SetTag("messaging.system", "rabbitmq");
        activity?.SetTag("messaging.destination", "payment-service-queue");

        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            OrderId = context.Message.Id
        };

        _dbContext.Payments.Add(payment);
        await _dbContext.SaveChangesAsync();

        Console.WriteLine($"💰 Pagamento registrado para pedido: {context.Message.ProductName}");
    }
}
