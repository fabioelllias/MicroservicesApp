using Contracts;
using MassTransit;
using NotificationService.Data;
using NotificationService.Models;

namespace NotificationService
{
    public class OrderConsumer : IConsumer<Order>
    {
        private readonly NotificationDbContext _db;

        public OrderConsumer(NotificationDbContext db)
        {
            _db = db;
        }

        public async Task Consume(ConsumeContext<Order> context)
        {
            var order = context.Message;
            Console.WriteLine($"[Notification] Enviando notificação para pedido: {order.ProductName}");

            var notification = new Notification
            {
                Message = $"Novo pedido recebido: {order.ProductName} (Qtd: {order.Quantity})"
            };

            _db.Notifications.Add(notification);
            await _db.SaveChangesAsync();
        }
    }
}
