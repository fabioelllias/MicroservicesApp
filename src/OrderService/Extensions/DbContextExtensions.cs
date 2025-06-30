using Microsoft.EntityFrameworkCore;

namespace OrderService.Extensions;

public static class DbContextExtensions
{
        public static IServiceCollection AddDbContext(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetSection("PostgresSettings")["ConnectionString"];

            // Fallback direto, caso o IConfiguration falhe por algum motivo
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                connectionString = Environment.GetEnvironmentVariable("POSTGRESSETTINGS__CONNECTIONSTRING");
                Console.WriteLine("⚠️ Fallback: lendo diretamente do Environment => " + connectionString);
            }

            if (string.IsNullOrWhiteSpace(connectionString))
                throw new InvalidOperationException("❌ A string de conexão do PostgreSQL não foi encontrada.");

            services.AddDbContext<OrderDbContext>(options =>
                options.UseNpgsql(connectionString));

            return services;
        }
}
