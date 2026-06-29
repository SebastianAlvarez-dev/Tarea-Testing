using Api.Infrastructure.Data;
using Api.Infrastructure.Data.Seed;
using Microsoft.EntityFrameworkCore;

namespace Api.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        var connectionString = configuration.GetConnectionString("bd");

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString)
        );

        services.AddScoped<DatabaseSeeder>();

        return services;
    }
}
