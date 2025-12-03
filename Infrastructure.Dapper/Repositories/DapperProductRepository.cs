using System.Data;
using System.Linq.Expressions;
using Application.Services.Interfaces;
using Dapper;
using Domain.Entities;
using Microsoft.Data.SqlClient;

namespace Infrastructure.Dapper.Repositories;

public class DapperProductRepository : IRepository<Product>
{
    private readonly string _connectionString;

    public DapperProductRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    private IDbConnection CreateConnection() => new SqlConnection(_connectionString);

    public async Task<Product?> GetByIdAsync(object id, CancellationToken cancellationToken = default)
    {
        using var connection = CreateConnection();
        var sql = @"SELECT * FROM Products WHERE Id = @Id";
        return await connection.QueryFirstOrDefaultAsync<Product>(sql, new { Id = id });
    }

    public async Task<IEnumerable<Product>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        using var connection = CreateConnection();
        var sql = @"SELECT * FROM Products ORDER BY Name";
        return await connection.QueryAsync<Product>(sql);
    }

    public async Task<IEnumerable<Product>> FindAsync(Expression<Func<Product, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await GetAllAsync(cancellationToken);
    }

    public async Task<Product> AddAsync(Product entity, CancellationToken cancellationToken = default)
    {
        using var connection = CreateConnection();
        entity.Id = Guid.NewGuid();
        entity.CreatedAt = DateTime.UtcNow;
        
        var sql = @"INSERT INTO Products (Id, Name, Description, Price, StockQuantity, SKU, CreatedAt, UpdatedAt)
                    VALUES (@Id, @Name, @Description, @Price, @StockQuantity, @SKU, @CreatedAt, @UpdatedAt)";
        
        await connection.ExecuteAsync(sql, entity);
        return entity;
    }

    public async Task AddRangeAsync(IEnumerable<Product> entities, CancellationToken cancellationToken = default)
    {
        using var connection = CreateConnection();
        var sql = @"INSERT INTO Products (Id, Name, Description, Price, StockQuantity, SKU, CreatedAt, UpdatedAt)
                    VALUES (@Id, @Name, @Description, @Price, @StockQuantity, @SKU, @CreatedAt, @UpdatedAt)";
        
        foreach (var entity in entities)
        {
            entity.Id = Guid.NewGuid();
            entity.CreatedAt = DateTime.UtcNow;
        }
        
        await connection.ExecuteAsync(sql, entities);
    }

    public async Task UpdateAsync(Product entity, CancellationToken cancellationToken = default)
    {
        using var connection = CreateConnection();
        entity.UpdatedAt = DateTime.UtcNow;
        
        var sql = @"UPDATE Products 
                    SET Name = @Name, Description = @Description, Price = @Price, 
                        StockQuantity = @StockQuantity, SKU = @SKU, UpdatedAt = @UpdatedAt
                    WHERE Id = @Id";
        
        await connection.ExecuteAsync(sql, entity);
    }

    public async Task DeleteAsync(Product entity, CancellationToken cancellationToken = default)
    {
        await DeleteByIdAsync(entity.Id, cancellationToken);
    }

    public async Task DeleteByIdAsync(object id, CancellationToken cancellationToken = default)
    {
        using var connection = CreateConnection();
        var sql = @"DELETE FROM Products WHERE Id = @Id";
        await connection.ExecuteAsync(sql, new { Id = id });
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(0);
    }
}
