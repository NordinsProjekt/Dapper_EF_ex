using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.EFCore;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the ApplicationDbContext to the service collection with SQL Server
    /// </summary>
    public static IServiceCollection AddApplicationDbContext(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        return services;
    }

    /// <summary>
    /// Adds the ApplicationDbContext to the service collection with a custom connection string
    /// </summary>
    public static IServiceCollection AddApplicationDbContext(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString));

        return services;
    }

    /// <summary>
    /// Adds the ApplicationDbContext to the service collection with custom configuration
    /// </summary>
    public static IServiceCollection AddApplicationDbContext(this IServiceCollection services, Action<DbContextOptionsBuilder> configureOptions)
    {
        services.AddDbContext<ApplicationDbContext>(configureOptions);
        return services;
    }

    /// <summary>
    /// Adds the ApplicationDbContext to the service collection with SQL Server and custom options
    /// </summary>
    public static IServiceCollection AddApplicationDbContext(
        this IServiceCollection services, 
        string connectionString, 
        Action<Microsoft.EntityFrameworkCore.Infrastructure.SqlServerDbContextOptionsBuilder>? sqlServerOptions = null)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseSqlServer(connectionString, sqlServerOptions);
        });

        return services;
    }

    /// <summary>
    /// Adds the ApplicationDbContext with in-memory database (for testing)
    /// </summary>
    public static IServiceCollection AddInMemoryApplicationDbContext(this IServiceCollection services, string databaseName = "TestDb")
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseInMemoryDatabase(databaseName));

        return services;
    }
}