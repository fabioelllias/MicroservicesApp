using MassTransit;
using Contracts;

public class OrderConsumer : IConsumer<Order>
{
    private readonly PaymentDbContext _dbContext;

    public OrderConsumer(PaymentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Consume(ConsumeContext<Order> context)
    {
        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            OrderId = context.Message.Id
        };

        _dbContext.Payments.Add(payment);
        await _dbContext.SaveChangesAsync();

        Console.WriteLine($"Pagamento registrado para pedido: {context.Message.ProductName}");
    }
}