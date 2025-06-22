using Microsoft.EntityFrameworkCore;
using Polly;
using Serilog;

namespace OrderService.Extensions;

public static class MigrationExtensions
{
    public static void UseDatabaseMigration<T>(this IApplicationBuilder app) where T : DbContext
    {
        var retryPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetry(5, attempt => TimeSpan.FromSeconds(5), (exception, time, retryCount, context) =>
            {
                Log.Warning("[Polly] Tentativa {RetryCount}: aguardando {Seconds}s - erro: {Message}", retryCount, time.TotalSeconds, exception.Message);
            });

        retryPolicy.Execute(() =>
        {
            using var scope = app.ApplicationServices.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<T>();
            db.Database.Migrate();
        });
    }
}
