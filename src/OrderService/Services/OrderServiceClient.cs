using System.Text.Json;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using OrderService.Configurations;
using OrderService.Models;
using OrderService.Services;
using Contracts;

public class OrderServiceClient : IOrderServiceClient
{
    private readonly IOrderPublisher _orderPublisher;
    private readonly ILogger<OrderServiceClient> _logger;
    private readonly OrderDbContext _context;
    private readonly IMongoCollection<OutboxMessage> _outboxCollection;
    private readonly EventDeliverySettings _deliverySettings;

    public OrderServiceClient(
        IOrderPublisher orderPublisher,
        ILogger<OrderServiceClient> logger,
        OrderDbContext context,
        IMongoCollection<OutboxMessage> outboxCollection,
        IOptions<EventDeliverySettings> deliverySettings)
    {
        _orderPublisher = orderPublisher;
        _logger = logger;
        _context = context;
        _outboxCollection = outboxCollection;
        _deliverySettings = deliverySettings.Value;
    }

    public async Task ProcessOrderAsync(Order order)
    {
        try
        {

            // usado para testar a resiliencia com polly a serviço externo
            // var result = await _externalClient.GetDataAsync();
            // _logger.LogInformation("Dados recebidos do serviço externo: {Data}", result);

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            _logger.LogInformation("✅ Pedido salvo no banco com ID {OrderId}", order.Id);

            var strategy = _deliverySettings.Strategy.ToLowerInvariant();

            if (strategy == "outbox")
            {
                var outboxMessage = new OutboxMessage
                {
                    EventType = "OrderCreated",
                    Payload = JsonSerializer.Serialize(order)
                };

                await _outboxCollection.InsertOneAsync(outboxMessage);
                _logger.LogInformation("📤 Evento gravado no Outbox com ID {OutboxId}", outboxMessage.Id);
            }
            else
            {
                await _orderPublisher.PublishAsync(order);
                _logger.LogInformation("📨 Evento publicado diretamente no RabbitMQ");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Erro ao processar pedido com ID {OrderId}", order?.Id);
            throw;
        }
    }
}
