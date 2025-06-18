using Contracts;

public interface IOrderServiceClient
{
    Task ProcessOrderAsync(Order order);
}
