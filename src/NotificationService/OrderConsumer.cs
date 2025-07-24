using Contracts;
using MassTransit;
using NotificationService.Data;
using NotificationService.Models;

namespace NotificationService;

public class OrderConsumer : IConsumer<Order>
{
    private readonly NotificationDbContext _context;
    private readonly ILogger<OrderConsumer> _logger;
    private readonly IHostEnvironment _env;

    public OrderConsumer(NotificationDbContext context, ILogger<OrderConsumer> logger, IHostEnvironment env)
    {
        _context = context;
        _logger = logger;
        _env = env;
    }

    public async Task Consume(ConsumeContext<Order> context)
    {
        var order = context.Message;

        _logger.LogInformation("📨 Pedido recebido no NotificationService: {OrderId}", order.Id);

        _context.Notifications.Add(new Notification
        {
            Id = order.Id,
            Message = $"Novo pedido recebido: {order.ProductName}",
            CreatedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();
        _logger.LogInformation("💾 Notificação salva no banco para o pedido {OrderId}", order.Id);
    }
}
