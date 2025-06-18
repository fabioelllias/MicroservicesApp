using Contracts;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using OrderService.Models;
using System.Text.Json;

namespace OrderService.Services
{
    public class OutboxWorker : BackgroundService
    {
        private readonly ILogger<OutboxWorker> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly TimeSpan _interval = TimeSpan.FromSeconds(10);

        public OutboxWorker(ILogger<OutboxWorker> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("📦 OutboxWorker iniciado.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();

                    var collection = scope.ServiceProvider
                        .GetRequiredService<IMongoCollection<OutboxMessage>>();

                    var publisher = scope.ServiceProvider
                        .GetRequiredService<IOrderPublisher>();

                    var pendingMessages = await collection.Find(x => x.Status == "pending")
                                                          .SortBy(x => x.CreatedAt)
                                                          .Limit(10)
                                                          .ToListAsync(stoppingToken);

                    foreach (var message in pendingMessages)
                    {
                        try
                        {
                            var order = JsonSerializer.Deserialize<Order>(message.Payload);
                            if (order == null)
                                throw new Exception("Falha na desserialização da mensagem");

                            await publisher.PublishAsync(order);

                            var update = Builders<OutboxMessage>.Update
                                .Set(x => x.Status, "processed")
                                .Set(x => x.RetryCount, message.RetryCount + 1);

                            await collection.UpdateOneAsync(
                                Builders<OutboxMessage>.Filter.Eq(x => x.Id, message.Id),
                                update,
                                cancellationToken: stoppingToken
                            );

                            _logger.LogInformation("✅ Evento {OutboxId} publicado e marcado como processado", message.Id);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "❌ Falha ao publicar evento {OutboxId}", message.Id);

                            var update = Builders<OutboxMessage>.Update
                                .Set(x => x.RetryCount, message.RetryCount + 1);

                            await collection.UpdateOneAsync(
                                Builders<OutboxMessage>.Filter.Eq(x => x.Id, message.Id),
                                update,
                                cancellationToken: stoppingToken
                            );
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Erro geral no OutboxWorker");
                }

                await Task.Delay(_interval, stoppingToken);
            }
        }
    }
}
