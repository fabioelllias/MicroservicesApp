namespace OrderService.Configurations
{
    public class EventDeliverySettings
    {
        public string Strategy { get; set; } = "direct"; // "outbox", "direct"
        public string PublishMechanism { get; set; } = "publish"; // "send" or "publish"
    }
}
