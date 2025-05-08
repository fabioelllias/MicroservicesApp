using System.ComponentModel.DataAnnotations;

public class Payment
{
    [Key]
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public string Status { get; set; } = "Confirmed";
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}