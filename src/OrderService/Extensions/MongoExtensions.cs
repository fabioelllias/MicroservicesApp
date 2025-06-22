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
            var mongoSettings = configuration.GetSection("MongoSettings").Get<MongoSettings>();
            services.AddSingleton(mongoSettings);

            services.AddSingleton<IMongoClient>(sp =>
                new MongoClient(mongoSettings?.ConnectionString));

            services.AddScoped(sp =>
            {
                var client = sp.GetRequiredService<IMongoClient>();
                return client.GetDatabase(mongoSettings?.DatabaseName)
                            .GetCollection<OutboxMessage>(mongoSettings?.OutboxCollection);
            });

            return services;
        }
    }
}