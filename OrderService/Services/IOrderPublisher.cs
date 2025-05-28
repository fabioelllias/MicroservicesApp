using Contracts;

namespace OrderService.Services
{
    public interface IOrderPublisher
    {
        Task PublishAsync(Order order);
    }
}
