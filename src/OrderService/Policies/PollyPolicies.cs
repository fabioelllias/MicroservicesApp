using Polly;
using Polly.Extensions.Http;
using Polly.Timeout;
using System.Net.Http;

namespace OrderService.Policies;

public static class PollyPolicies
{
    public static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        var jitterer = new Random();
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt =>
                    TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)) +
                    TimeSpan.FromMilliseconds(jitterer.Next(0, 1000)),
                onRetry: (outcome, timespan, retryAttempt, context) =>
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"🔁 [Polly|Retry {retryAttempt}] Esperando {timespan.TotalSeconds:n1}s - Motivo: {outcome.Exception?.Message ?? outcome.Result?.StatusCode.ToString()}");
                    Console.ResetColor();
                });
    }

    public static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: 3,
                durationOfBreak: TimeSpan.FromSeconds(30),
                onBreak: (outcome, timespan) =>
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"⛔ [Polly|Breaker] Circuito aberto por {timespan.TotalSeconds}s - Motivo: {outcome.Exception?.Message ?? outcome.Result?.StatusCode.ToString()}");
                    Console.ResetColor();
                },
                onReset: () =>
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("✅ [Polly|Breaker] Circuito fechado - serviço estável novamente.");
                    Console.ResetColor();
                });
    }

    public static IAsyncPolicy<HttpResponseMessage> GetTimeoutPolicy()
    {
        return Policy.TimeoutAsync<HttpResponseMessage>(
            timeout: TimeSpan.FromSeconds(5),
            timeoutStrategy: TimeoutStrategy.Pessimistic,
            onTimeoutAsync: (context, timespan, task, exception) =>
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"⌛ [Polly|Timeout] Excedido {timespan.TotalSeconds}s de tempo limite.");
                Console.ResetColor();
                return Task.CompletedTask;
            });
    }
}
