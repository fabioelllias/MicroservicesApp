using Contracts;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderServiceClient _orderServiceClient;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(IOrderServiceClient orderServiceClient, ILogger<OrdersController> logger)
    {
        _orderServiceClient = orderServiceClient;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] Order order)
    {
         try
        {
            await _orderServiceClient.ProcessOrderAsync(order);
            return Ok("Pedido salvo e evento gerado com sucesso");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Erro ao processar pedido com ID {OrderId}", order?.Id);
            return StatusCode(500, "Erro ao processar o pedido");
        }
    }
}