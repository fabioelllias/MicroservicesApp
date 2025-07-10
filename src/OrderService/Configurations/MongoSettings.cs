namespace OrderService.Configurations
{
    public class MongoSettings
    {
        public string ConnectionString { get; set; } = default!;
        public string DatabaseName { get; set; } = default!;
        public string OutboxCollection { get; set; } = "OutboxMessages";
    }
}
