using Application.Services.Interfaces;
using Domain.Entities;
using Infrastructure.Dapper.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Dapper;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDapperInfrastructure(this IServiceCollection services, string connectionString)
    {
        // Register Dapper repositories
        services.AddScoped<IRepository<Customer>>(sp => new DapperCustomerRepository(connectionString));
        services.AddScoped<IRepository<Product>>(sp => new DapperProductRepository(connectionString));
        services.AddScoped<IRepository<Employee>>(sp => new DapperEmployeeRepository(connectionString));
        
        return services;
    }
}
