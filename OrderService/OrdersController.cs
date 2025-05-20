using Contracts;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<OrdersController> _logger;
    private readonly OrderDbContext _context;

    public OrdersController(
        IPublishEndpoint publishEndpoint,
        ILogger<OrdersController> logger,
        OrderDbContext context)
    {
        _publishEndpoint = publishEndpoint;
        _logger = logger;
        _context = context;

        _logger.LogInformation("🎯 Teste de log no OrdersController");
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] Order order)
    {
        try
        {
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Novo pedido salvo no banco com ID {OrderId}", order.Id);

            await _publishEndpoint.Publish(order);
            _logger.LogInformation("Pedido com ID {OrderId} publicado no RabbitMQ", order.Id);

            return Ok("Pedido salvo e enviado");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar pedido com ID {OrderId}", order?.Id);
            return StatusCode(500, "Erro ao processar o pedido");
        }
    }
}
