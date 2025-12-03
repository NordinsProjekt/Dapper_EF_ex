using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;

namespace Benchmarks;

/// <summary>
/// Benchmarks simulating real-world Dapper repository scenarios
/// </summary>
[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
public class DapperRepositoryBenchmarks
{
    // Simulating typical Dapper repository methods

    // Const approach (like your updated code)
    [Benchmark(Baseline = true, Description = "Repository with Const SQL")]
    public string Repository_ConstSql()
    {
        const string sql = @"SELECT * FROM Customers WHERE Id = @Id";
        return SimulateDapperCall(sql);
    }

    // Field approach
    [Benchmark(Description = "Repository with Field SQL")]
    public string Repository_FieldSql()
    {
        var sql = "SELECT * FROM Customers WHERE Id = @Id";
        return SimulateDapperCall(sql);
    }

    // Local variable approach
    [Benchmark(Description = "Repository with Local SQL")]
    public string Repository_LocalSql()
    {
        string sql = "SELECT * FROM Customers WHERE Id = @Id";
        return SimulateDapperCall(sql);
    }

    // Method with multiple queries (const)
    [Benchmark(Description = "Multiple Queries - Const")]
    public int MultipleQueries_Const()
    {
        const string selectSql = "SELECT * FROM Customers WHERE Id = @Id";
        const string insertSql = "INSERT INTO Customers (Id, FirstName, LastName, Email) VALUES (@Id, @FirstName, @LastName, @Email)";
        const string updateSql = "UPDATE Customers SET FirstName = @FirstName WHERE Id = @Id";
        const string deleteSql = "DELETE FROM Customers WHERE Id = @Id";
        
        return selectSql.Length + insertSql.Length + updateSql.Length + deleteSql.Length;
    }

    // Method with multiple queries (local)
    [Benchmark(Description = "Multiple Queries - Local")]
    public int MultipleQueries_Local()
    {
        string selectSql = "SELECT * FROM Customers WHERE Id = @Id";
        string insertSql = "INSERT INTO Customers (Id, FirstName, LastName, Email) VALUES (@Id, @FirstName, @LastName, @Email)";
        string updateSql = "UPDATE Customers SET FirstName = @FirstName WHERE Id = @Id";
        string deleteSql = "DELETE FROM Customers WHERE Id = @Id";
        
        return selectSql.Length + insertSql.Length + updateSql.Length + deleteSql.Length;
    }

    // Complex repository operation (const)
    [Benchmark(Description = "Complex Operation - Const")]
    public string ComplexOperation_Const()
    {
        const string getCustomerSql = "SELECT * FROM Customers WHERE Id = @Id";
        const string getReceiptsSql = @"
            SELECT r.*, ri.*, p.*
            FROM Receipts r
            INNER JOIN ReceiptItems ri ON r.Id = ri.ReceiptId
            INNER JOIN Products p ON ri.ProductId = p.Id
            WHERE r.CustomerId = @CustomerId";
        const string updateCustomerSql = "UPDATE Customers SET LastName = @LastName WHERE Id = @Id";
        
        return SimulateDapperCall(getCustomerSql) + 
               SimulateDapperCall(getReceiptsSql) + 
               SimulateDapperCall(updateCustomerSql);
    }

    // Complex repository operation (local)
    [Benchmark(Description = "Complex Operation - Local")]
    public string ComplexOperation_Local()
    {
        var getCustomerSql = "SELECT * FROM Customers WHERE Id = @Id";
        var getReceiptsSql = @"
            SELECT r.*, ri.*, p.*
            FROM Receipts r
            INNER JOIN ReceiptItems ri ON r.Id = ri.ReceiptId
            INNER JOIN Products p ON ri.ProductId = p.Id
            WHERE r.CustomerId = @CustomerId";
        var updateCustomerSql = "UPDATE Customers SET LastName = @LastName WHERE Id = @Id";
        
        return SimulateDapperCall(getCustomerSql) + 
               SimulateDapperCall(getReceiptsSql) + 
               SimulateDapperCall(updateCustomerSql);
    }

    // Simulate a Dapper call (just string operations, no actual DB call)
    private string SimulateDapperCall(string sql)
    {
        // Simulates what Dapper does internally with the SQL string
        // In reality, Dapper parses, caches, and uses the string
        return sql.GetHashCode().ToString();
    }

    // Test SQL string building patterns
    [Benchmark(Description = "String Builder Pattern")]
    public string StringBuilderPattern()
    {
        var sb = new System.Text.StringBuilder();
        sb.Append("SELECT * FROM Customers ");
        sb.Append("WHERE Id = @Id ");
        sb.Append("AND Active = 1");
        return sb.ToString();
    }

    [Benchmark(Description = "String Concatenation Pattern")]
    public string StringConcatPattern()
    {
        const string baseSql = "SELECT * FROM Customers ";
        return baseSql + "WHERE Id = @Id " + "AND Active = 1";
    }

    [Benchmark(Description = "String Join Pattern")]
    public string StringJoinPattern()
    {
        return string.Join(" ", 
            "SELECT * FROM Customers",
            "WHERE Id = @Id",
            "AND Active = 1");
    }
}
