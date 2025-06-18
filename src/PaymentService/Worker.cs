using MassTransit;

namespace PaymentService
{
    public class Worker : BackgroundService
    {
        private readonly IBusControl _bus;

        public Worker(IBusControl bus)
        {
            _bus = bus;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Console.WriteLine("[Worker] Inicializando MassTransit...");

            await _bus.StartAsync(stoppingToken);

            Console.WriteLine("[Worker] MassTransit conectado ao RabbitMQ.");

            try
            {
                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
            catch (TaskCanceledException)
            {
                // Ignorado no encerramento do serviço
            }

            Console.WriteLine("[Worker] Encerrando MassTransit...");
            await _bus.StopAsync(stoppingToken);
        }
    }
}
