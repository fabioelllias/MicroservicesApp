using System.Text.Json;
using Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using OrderService.Configurations;
using OrderService.Models;
using OrderService.Services;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
     private readonly IOrderPublisher _orderPublisher;
    private readonly ILogger<OrdersController> _logger;
    private readonly OrderDbContext _context;
    private readonly IExternalServiceClient _externalClient;
    private readonly IMongoCollection<OutboxMessage> _outboxCollection;
    private readonly IOptions<EventDeliverySettings> _deliverySettings;
    public OrdersController(
        IOrderPublisher orderPublisher,
        ILogger<OrdersController> logger,
        OrderDbContext context,
        IExternalServiceClient externalClient,
        IMongoCollection<OutboxMessage> outboxCollection,
        IOptions<EventDeliverySettings> deliverySettings)
    {
        _orderPublisher = orderPublisher;
        _logger = logger;
        _context = context;
        _externalClient = externalClient;
        _outboxCollection = outboxCollection;
        _deliverySettings = deliverySettings;
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] Order order)
    {
        try
        {

            // var result = await _externalClient.GetDataAsync();
            // _logger.LogInformation("Dados recebidos do serviço externo: {Data}", result);

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Novo pedido salvo no banco com ID {OrderId}", order.Id);

             var strategy = _deliverySettings.Value.Strategy.ToLowerInvariant();

            if (strategy == "outbox")
            {
                var outboxMessage = new OutboxMessage
                {
                    EventType = "OrderCreated",
                    Payload = JsonSerializer.Serialize(order)
                };

                await _outboxCollection.InsertOneAsync(outboxMessage);
                _logger.LogInformation("📤 Evento gravado no outbox com ID {OutboxId}", outboxMessage.Id);
            }
            else
            {
                await _orderPublisher.PublishAsync(order);
                _logger.LogInformation("📨 Evento publicado diretamente no RabbitMQ");
            }

            return Ok("Pedido salvo e enviado");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar pedido com ID {OrderId}", order?.Id);
            return StatusCode(500, "Erro ao processar o pedido");
        }
    }
}
