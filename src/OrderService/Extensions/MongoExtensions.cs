using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
            var mongoSettings = configuration.GetSection("MongoSettings").Get<MongoSettings>();

            // Fallback para variáveis de ambiente (caso use Container App)
            if (string.IsNullOrWhiteSpace(mongoSettings?.ConnectionString))
            {
                mongoSettings ??= new MongoSettings(); // garante instância não nula
                mongoSettings.ConnectionString = Environment.GetEnvironmentVariable("MONGODBSETTINGS__CONNECTIONSTRING");
                Console.WriteLine("⚠️ Fallback: lendo MongoDB ConnectionString do Environment => " +
                                  (string.IsNullOrEmpty(mongoSettings.ConnectionString) ? "(vazio)" : "✅ encontrado"));
            }

            if (string.IsNullOrWhiteSpace(mongoSettings?.DatabaseName))
            {
                mongoSettings.DatabaseName = Environment.GetEnvironmentVariable("MONGODBSETTINGS__DATABASENAME");
            }

            if (string.IsNullOrWhiteSpace(mongoSettings?.OutboxCollection))
            {
                mongoSettings.OutboxCollection = Environment.GetEnvironmentVariable("MONGODBSETTINGS__OUTBOXCOLLECTION");
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