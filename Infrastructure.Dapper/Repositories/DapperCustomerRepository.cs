using System.Data;
using System.Linq.Expressions;
using Application.Services.Interfaces;
using Dapper;
using Domain.Entities;
using Microsoft.Data.SqlClient;

namespace Infrastructure.Dapper.Repositories;

public class DapperCustomerRepository(string connectionString) : IRepository<Customer>
{
    private IDbConnection CreateConnection() => new SqlConnection(connectionString);

    public async Task<Customer?> GetByIdAsync(object id, CancellationToken cancellationToken = default)
    {
        using var connection = CreateConnection();
        const string sql = @"SELECT * FROM Customers WHERE Id = @Id";
        return await connection.QueryFirstOrDefaultAsync<Customer>(sql, new { Id = id });
    }

    public async Task<IEnumerable<Customer>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        using var connection = CreateConnection();
        const string sql = @"SELECT * FROM Customers ORDER BY LastName, FirstName";
        return await connection.QueryAsync<Customer>(sql);
    }

    public async Task<IEnumerable<Customer>> FindAsync(Expression<Func<Customer, bool>> predicate, CancellationToken cancellationToken = default)
    {
        // Note: Dapper doesn't support Expression trees directly
        // For a production system, consider using a library like DapperExtensions or implement specific search methods
        // For now, get all and filter in memory (not ideal for large datasets)
        var all = await GetAllAsync(cancellationToken);
        return all.Where(predicate.Compile());
    }

    public async Task<Customer?> FirstOrDefaultAsync(Expression<Func<Customer, bool>> predicate, CancellationToken cancellationToken = default)
    {
        var results = await FindAsync(predicate, cancellationToken);
        return results.FirstOrDefault();
    }

    public async Task<Customer> AddAsync(Customer entity, CancellationToken cancellationToken = default)
    {
        using var connection = CreateConnection();
        entity.Id = Guid.NewGuid();
        entity.CreatedAt = DateTime.UtcNow;
        
        const string sql = @"INSERT INTO Customers (Id, FirstName, LastName, Email, Phone, CreatedAt, UpdatedAt)
                    VALUES (@Id, @FirstName, @LastName, @Email, @Phone, @CreatedAt, @UpdatedAt)";
        
        await connection.ExecuteAsync(sql, entity);
        return entity;
    }

    public async Task AddRangeAsync(IEnumerable<Customer> entities, CancellationToken cancellationToken = default)
    {
        using var connection = CreateConnection();
        const string sql = @"INSERT INTO Customers (Id, FirstName, LastName, Email, Phone, CreatedAt, UpdatedAt)
                    VALUES (@Id, @FirstName, @LastName, @Email, @Phone, @CreatedAt, @UpdatedAt)";
        
        foreach (var entity in entities)
        {
            entity.Id = Guid.NewGuid();
            entity.CreatedAt = DateTime.UtcNow;
        }
        
        await connection.ExecuteAsync(sql, entities);
    }

    public async Task UpdateAsync(Customer entity, CancellationToken cancellationToken = default)
    {
        using var connection = CreateConnection();
        entity.UpdatedAt = DateTime.UtcNow;
        
        const string sql = @"UPDATE Customers 
                    SET FirstName = @FirstName, LastName = @LastName, Email = @Email, 
                        Phone = @Phone, UpdatedAt = @UpdatedAt
                    WHERE Id = @Id";
        
        await connection.ExecuteAsync(sql, entity);
    }

    public async Task DeleteAsync(Customer entity, CancellationToken cancellationToken = default)
    {
        await DeleteByIdAsync(entity.Id, cancellationToken);
    }

    public async Task DeleteByIdAsync(object id, CancellationToken cancellationToken = default)
    {
        using var connection = CreateConnection();
        const string sql = @"DELETE FROM Customers WHERE Id = @Id";
        await connection.ExecuteAsync(sql, new { Id = id });
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Dapper doesn't use Unit of Work pattern by default
        return Task.FromResult(0);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var connection = CreateConnection();
        const string sql = @"SELECT COUNT(1) FROM Customers WHERE Id = @Id";
        var count = await connection.ExecuteScalarAsync<int>(sql, new { Id = id });
        return count > 0;
    }

    public async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        using var connection = CreateConnection();
        const string sql = @"SELECT COUNT(*) FROM Customers";
        return await connection.ExecuteScalarAsync<int>(sql);
    }

    public async Task<int> CountAsync(Expression<Func<Customer, bool>> predicate, CancellationToken cancellationToken = default)
    {
        var results = await FindAsync(predicate, cancellationToken);
        return results.Count();
    }
}
