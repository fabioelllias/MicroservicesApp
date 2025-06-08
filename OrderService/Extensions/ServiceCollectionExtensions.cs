// Extensions/ServiceCollectionExtensions.cs

using MassTransit;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using HealthChecks.UI.Client;
using OpenTelemetry.Trace;
using OpenTelemetry.Resources;
using Microsoft.Extensions.DependencyInjection;
using Prometheus;
using OrderService.Policies;

namespace OrderService.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMassTransitWithRabbitMq(this IServiceCollection services)
        {
            services.AddMassTransit(x =>
            {
                x.UsingRabbitMq((ctx, cfg) =>
                {
                    cfg.Host("rabbitmq", "/", h =>
                    {
                        h.Username("guest");
                        h.Password("guest");
                    });
                });
            });

            return services;
        }

        public static IServiceCollection AddOpenTelemetryWithJaeger(this IServiceCollection services)
        {
            services.AddOpenTelemetry()
                .WithTracing(tracerProviderBuilder =>
                {
                    tracerProviderBuilder
                        .AddAspNetCoreInstrumentation()
                        .AddHttpClientInstrumentation()
                        .SetResourceBuilder(
                            ResourceBuilder.CreateDefault()
                                .AddService("OrderService")) // nome do serviço que aparece no Jaeger
                        .AddOtlpExporter(opt =>
                        {
                            opt.Endpoint = new Uri("http://jaeger:4317");
                        });
                });

            return services;
        }

        public static IServiceCollection AddCustomHealthChecks(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddHealthChecks()
                .AddNpgSql(
                    connectionString: configuration.GetConnectionString("DefaultConnection") ??
                                     "Host=orderdb;Port=5432;Username=postgres;Password=postgres;Database=orderdb",
                    name: "PostgreSQL")
                .AddRabbitMQ(_ =>
                    {
                        var factory = new RabbitMQ.Client.ConnectionFactory()
                        {
                            Uri = new Uri("amqp://guest:guest@rabbitmq:5672")
                        };
                        return factory.CreateConnectionAsync();
                    }, name: "RabbitMQ");


            services.AddHealthChecksUI(setup =>
            {
                setup.SetEvaluationTimeInSeconds(15);
                setup.AddHealthCheckEndpoint("Self", "http://orderservice:80/health");
            }).AddInMemoryStorage();

            return services;
        }

        public static IServiceCollection AddCustomPolly(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddHttpClient("ExternalServiceClient", client =>{
                    client.BaseAddress = new Uri("http://orderservice"); // ou http://orderservice:5002 se estiver em container
                })      
                .AddPolicyHandler(PollyPolicies.GetRetryPolicy())
                .AddPolicyHandler(PollyPolicies.GetCircuitBreakerPolicy())
                .AddPolicyHandler(PollyPolicies.GetTimeoutPolicy());

            return services;
        }

        public static IApplicationBuilder UseCustomHealthChecks(this IApplicationBuilder app)
        {
            app.UseHealthChecks("/health", new HealthCheckOptions
            {
                Predicate = _ => true,
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });

            app.UseHealthChecksUI(options =>
            {
                options.UIPath = "/health-ui";
                options.ApiPath = "/health-ui-api";
            });

            return app;
        }

        public static IApplicationBuilder UseCustomMetrics(this IApplicationBuilder app)
        {
            app.UseHttpMetrics();

            app.UseEndpoints(endpoints =>
            {   
                endpoints.MapMetrics("/metrics"); // aqui define o endpoint
            });

            return app;
        }
    }
}
