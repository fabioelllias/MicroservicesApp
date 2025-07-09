using Contracts;
using MassTransit;
using System.Diagnostics;
using OpenTelemetry.Context.Propagation;
using OrderService.Observability;
using OpenTelemetry;

namespace OrderService.Services
{
    public class OrderPublisher : IOrderPublisher
    {
        private readonly IPublishEndpoint _publishEndpoint;

        public OrderPublisher(IPublishEndpoint publishEndpoint)
        {
            _publishEndpoint = publishEndpoint;
        }

       public async Task PublishAsync(Order order)
     {
            using var activity = Tracing.Source.StartActivity("PublishOrder");

            var publishTask = _publishEndpoint.Publish(order, context =>
            {
                var propagationContext = new PropagationContext(Activity.Current.Context, Baggage.Current);

                Propagators.DefaultTextMapPropagator.Inject(
                    propagationContext,
                    context.Headers,
                    (headers, key, value) => headers.Set(key, value));
            });

            var completed = await Task.WhenAny(publishTask, Task.Delay(3000)); // timeout de 3s

            if (completed != publishTask)
            {
                throw new TimeoutException("❌ Timeout ao tentar publicar mensagem via MassTransit.");
            }

            await publishTask; // rethrow se falhou
        }
    }
}