using Contracts;
using MassTransit;
using System.Diagnostics;
using OpenTelemetry.Context.Propagation;
using OrderService.Observability;
using OpenTelemetry;
using OrderService.Configurations;

namespace OrderService.Services
{
    public class OrderPublisher : IOrderPublisher
    {
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly ISendEndpointProvider _sendEndpointProvider;
        private readonly EventDeliverySettings _settings;
        private readonly ILogger<OrderPublisher> _logger;


        public OrderPublisher(IPublishEndpoint publishEndpoint, ISendEndpointProvider sendEndpointProvider, EventDeliverySettings settings, ILogger<OrderPublisher> logger)
        {
            _publishEndpoint = publishEndpoint;
            _sendEndpointProvider = sendEndpointProvider;
            _settings = settings;
            _logger = logger;
        }

        public async Task PublishAsync(Order order)
        {
            _logger.LogInformation("📤 Iniciando envio do pedido {OrderId} via {Mechanism}", order.Id, _settings.PublishMechanism);

            if (_settings.PublishMechanism == "Send")
            {
                var endpoint = await _sendEndpointProvider.GetSendEndpoint(
                    new Uri("queue:notification-service-queue"));

                await endpoint.Send(order);
                _logger.LogInformation("📦 Pedido {OrderId} enviado com sucesso via Send", order.Id);
            }
            else
            {
                using var activity = Tracing.Source.StartActivity("PublishOrder");

                var publishTask = _publishEndpoint.Publish<Order>(order, context =>
                {
                    var activityContext = Activity.Current != null ? Activity.Current.Context : default;
                    var propagationContext = new PropagationContext(activityContext, Baggage.Current);

                    Propagators.DefaultTextMapPropagator.Inject(
                        propagationContext,
                        context.Headers,
                        (headers, key, value) => headers.Set(key, value));
                });

                // var completed = await Task.WhenAny(publishTask, Task.Delay(10000));

                // if (completed != publishTask)
                // {
                //     _logger.LogError("❌ Falha ao enviar pedido {OrderId} via Send", order.Id);
                //     throw new TimeoutException("❌ Timeout ao tentar publicar mensagem via MassTransit.");
                // }

                await publishTask;
                _logger.LogInformation("📦 Pedido {OrderId} enviado com sucesso via Publish", order.Id);
            }

            
        }
    }
}