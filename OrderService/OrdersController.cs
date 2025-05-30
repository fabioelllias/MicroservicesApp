using Contracts;
using Microsoft.AspNetCore.Mvc;
using OrderService.Services;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
     private readonly IOrderPublisher _orderPublisher;
    private readonly ILogger<OrdersController> _logger;
    private readonly OrderDbContext _context;

    public OrdersController(
        IOrderPublisher orderPublisher,
        ILogger<OrdersController> logger,
        OrderDbContext context)
    {
        _orderPublisher = orderPublisher;
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

            await _orderPublisher.PublishAsync(order);

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
