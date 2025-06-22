using Microsoft.EntityFrameworkCore;

namespace OrderService.Extensions;

public static class DbContextExtensions
{
    public static IServiceCollection AddDbContext(this IServiceCollection services, IConfiguration configuration)
    {
        var postgresConnection = configuration.GetSection("PostgresSettings")["ConnectionString"];

        if (string.IsNullOrEmpty(postgresConnection))
        {
            throw new InvalidOperationException("❌ A string de conexão do PostgreSQL não foi encontrada. Verifique 'PostgresSettings:ConnectionString' no appsettings.json ou nas variáveis de ambiente.");
        }

        services.AddDbContext<OrderDbContext>(options =>
            options.UseNpgsql(postgresConnection));

        return services;
    }
}
