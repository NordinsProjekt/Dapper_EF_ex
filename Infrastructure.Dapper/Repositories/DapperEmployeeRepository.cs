using System.Data;
using System.Linq.Expressions;
using Application.Services.Interfaces;
using Dapper;
using Domain.Entities;
using Microsoft.Data.SqlClient;

namespace Infrastructure.Dapper.Repositories;

public class DapperEmployeeRepository : IRepository<Employee>
{
    private readonly string _connectionString;

    public DapperEmployeeRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    private IDbConnection CreateConnection() => new SqlConnection(_connectionString);

    public async Task<Employee?> GetByIdAsync(object id, CancellationToken cancellationToken = default)
    {
        using var connection = CreateConnection();
        var sql = @"SELECT * FROM Employees WHERE Id = @Id";
        return await connection.QueryFirstOrDefaultAsync<Employee>(sql, new { Id = id });
    }

    public async Task<IEnumerable<Employee>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        using var connection = CreateConnection();
        var sql = @"SELECT * FROM Employees ORDER BY LastName, FirstName";
        return await connection.QueryAsync<Employee>(sql);
    }

    public async Task<IEnumerable<Employee>> FindAsync(Expression<Func<Employee, bool>> predicate, CancellationToken cancellationToken = default)
    {
        var all = await GetAllAsync(cancellationToken);
        return all.Where(predicate.Compile());
    }

    public async Task<Employee?> FirstOrDefaultAsync(Expression<Func<Employee, bool>> predicate, CancellationToken cancellationToken = default)
    {
        var results = await FindAsync(predicate, cancellationToken);
        return results.FirstOrDefault();
    }

    public async Task<Employee> AddAsync(Employee entity, CancellationToken cancellationToken = default)
    {
        using var connection = CreateConnection();
        entity.Id = Guid.NewGuid();
        
        var sql = @"INSERT INTO Employees (Id, FirstName, LastName, Email, Phone, HireDate, HourlyRate, IsActive)
                    VALUES (@Id, @FirstName, @LastName, @Email, @Phone, @HireDate, @HourlyRate, @IsActive)";
        
        await connection.ExecuteAsync(sql, entity);
        return entity;
    }

    public async Task AddRangeAsync(IEnumerable<Employee> entities, CancellationToken cancellationToken = default)
    {
        using var connection = CreateConnection();
        var sql = @"INSERT INTO Employees (Id, FirstName, LastName, Email, Phone, HireDate, HourlyRate, IsActive)
                    VALUES (@Id, @FirstName, @LastName, @Email, @Phone, @HireDate, @HourlyRate, @IsActive)";
        
        foreach (var entity in entities)
        {
            entity.Id = Guid.NewGuid();
        }
        
        await connection.ExecuteAsync(sql, entities);
    }

    public async Task UpdateAsync(Employee entity, CancellationToken cancellationToken = default)
    {
        using var connection = CreateConnection();
        
        var sql = @"UPDATE Employees 
                    SET FirstName = @FirstName, LastName = @LastName, Email = @Email, 
                        Phone = @Phone, HireDate = @HireDate, HourlyRate = @HourlyRate, IsActive = @IsActive
                    WHERE Id = @Id";
        
        await connection.ExecuteAsync(sql, entity);
    }

    public async Task DeleteAsync(Employee entity, CancellationToken cancellationToken = default)
    {
        await DeleteByIdAsync(entity.Id, cancellationToken);
    }

    public async Task DeleteByIdAsync(object id, CancellationToken cancellationToken = default)
    {
        using var connection = CreateConnection();
        var sql = @"DELETE FROM Employees WHERE Id = @Id";
        await connection.ExecuteAsync(sql, new { Id = id });
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(0);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var connection = CreateConnection();
        const string sql = @"SELECT COUNT(1) FROM Employees WHERE Id = @Id";
        var count = await connection.ExecuteScalarAsync<int>(sql, new { Id = id });
        return count > 0;
    }

    public async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        using var connection = CreateConnection();
        const string sql = @"SELECT COUNT(*) FROM Employees";
        return await connection.ExecuteScalarAsync<int>(sql);
    }

    public async Task<int> CountAsync(Expression<Func<Employee, bool>> predicate, CancellationToken cancellationToken = default)
    {
        var results = await FindAsync(predicate, cancellationToken);
        return results.Count();
    }
}
