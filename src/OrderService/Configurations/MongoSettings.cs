namespace OrderService.Configurations
{
    public class MongoSettings
    {
        public string ConnectionString { get; set; } = null!;
        public string DatabaseName { get; set; } = null!;
        public string OutboxCollection { get; set; } = "OutboxMessages";
    }
}
