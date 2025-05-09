namespace Contracts;

public class Payment
{
    public Guid Id { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
}