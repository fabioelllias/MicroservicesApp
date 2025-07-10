using MongoDB.Driver;
using OrderService.Configurations;
using OrderService.Models;

namespace OrderService.Extensions
{
    public static class MongoExtensions
    {
        public static IServiceCollection AddMongo(this IServiceCollection services, IConfiguration configuration)
        {
            // Carrega da configuração
            var mongoSettings = configuration.GetSection("MongoSettings").Get<MongoSettings>() ?? new MongoSettings();

            // Fallback para variáveis de ambiente (caso use Container App)
            if (string.IsNullOrWhiteSpace(mongoSettings.ConnectionString))
            {
                mongoSettings.ConnectionString = Environment.GetEnvironmentVariable("MONGOSETTINGS__CONNECTIONSTRING") ?? string.Empty;
                Console.WriteLine("⚠️ Fallback: lendo MongoDB ConnectionString do Environment => " +
                                  (string.IsNullOrEmpty(mongoSettings.ConnectionString) ? "(vazio)" : "✅ encontrado"));
            }

            if (string.IsNullOrWhiteSpace(mongoSettings.DatabaseName))
            {
                mongoSettings.DatabaseName = Environment.GetEnvironmentVariable("MONGOSETTINGS__DATABASENAME") ?? string.Empty;
            }

            if (string.IsNullOrWhiteSpace(mongoSettings.OutboxCollection))
            {
                mongoSettings.OutboxCollection = Environment.GetEnvironmentVariable("MONGOSETTINGS__OUTBOXCOLLECTION") ?? string.Empty;
            }

            if (string.IsNullOrWhiteSpace(mongoSettings.ConnectionString) ||
                string.IsNullOrWhiteSpace(mongoSettings.DatabaseName) ||
                string.IsNullOrWhiteSpace(mongoSettings.OutboxCollection))
            {
                Console.WriteLine("❌ Configuração do MongoDB incompleta. Verifique as variáveis de ambiente ou appsettings.");
                return services;
            }

            // Injeta as configurações e os serviços Mongo
            services.AddSingleton(mongoSettings);

            services.AddSingleton<IMongoClient>(sp =>
                new MongoClient(mongoSettings.ConnectionString));

            services.AddScoped(sp =>
            {
                var client = sp.GetRequiredService<IMongoClient>();
                return client
                    .GetDatabase(mongoSettings.DatabaseName)
                    .GetCollection<OutboxMessage>(mongoSettings.OutboxCollection);
            });

            Console.WriteLine("✅ MongoDB configurado com sucesso.");
            return services;
        }

    }
}