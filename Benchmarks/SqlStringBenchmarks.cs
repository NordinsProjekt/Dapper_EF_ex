using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;

namespace Benchmarks;

/// <summary>
/// Benchmarks comparing const string vs regular string for SQL queries
/// Tests different scenarios: short queries, medium queries, long queries, and repeated usage
/// </summary>
[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
public class SqlStringBenchmarks
{
    // Short SQL query (typical simple query)
    private const string ShortSqlConst = "SELECT * FROM Customers WHERE Id = @Id";
    private readonly string _shortSqlField = "SELECT * FROM Customers WHERE Id = @Id";
    
    // Medium SQL query (typical join query)
    private const string MediumSqlConst = @"
        SELECT c.*, r.*, p.*
        FROM Customers c
        INNER JOIN Receipts r ON c.Id = r.CustomerId
        INNER JOIN ReceiptItems ri ON r.Id = ri.ReceiptId
        INNER JOIN Products p ON ri.ProductId = p.Id
        WHERE c.Id = @Id";
    
    private readonly string _mediumSqlField = @"
        SELECT c.*, r.*, p.*
        FROM Customers c
        INNER JOIN Receipts r ON c.Id = r.CustomerId
        INNER JOIN ReceiptItems ri ON r.Id = ri.ReceiptId
        INNER JOIN Products p ON ri.ProductId = p.Id
        WHERE c.Id = @Id";
    
    // Long SQL query (complex query with multiple joins and conditions)
    private const string LongSqlConst = @"
        SELECT 
            c.Id, c.FirstName, c.LastName, c.Email, c.Phone, c.CreatedAt, c.UpdatedAt,
            r.Id as ReceiptId, r.PurchaseDate, r.TotalAmount, r.TaxAmount, r.Notes as ReceiptNotes,
            ri.Id as ItemId, ri.Quantity, ri.UnitPrice, ri.TotalPrice,
            p.Id as ProductId, p.Name as ProductName, p.Description, p.Price, p.StockQuantity, p.SKU,
            pm.Id as PaymentMethodId, pm.Name as PaymentMethodName, pm.Description as PaymentMethodDescription
        FROM Customers c
        LEFT JOIN Receipts r ON c.Id = r.CustomerId
        LEFT JOIN ReceiptItems ri ON r.Id = ri.ReceiptId
        LEFT JOIN Products p ON ri.ProductId = p.Id
        LEFT JOIN PaymentMethods pm ON r.PaymentMethodId = pm.Id
        WHERE c.CreatedAt >= @StartDate 
            AND c.CreatedAt <= @EndDate
            AND (c.Email LIKE @EmailPattern OR c.LastName LIKE @NamePattern)
            AND r.TotalAmount > @MinAmount
        ORDER BY c.LastName, c.FirstName, r.PurchaseDate DESC";
    
    private readonly string _longSqlField = @"
        SELECT 
            c.Id, c.FirstName, c.LastName, c.Email, c.Phone, c.CreatedAt, c.UpdatedAt,
            r.Id as ReceiptId, r.PurchaseDate, r.TotalAmount, r.TaxAmount, r.Notes as ReceiptNotes,
            ri.Id as ItemId, ri.Quantity, ri.UnitPrice, ri.TotalPrice,
            p.Id as ProductId, p.Name as ProductName, p.Description, p.Price, p.StockQuantity, p.SKU,
            pm.Id as PaymentMethodId, pm.Name as PaymentMethodName, pm.Description as PaymentMethodDescription
        FROM Customers c
        LEFT JOIN Receipts r ON c.Id = r.CustomerId
        LEFT JOIN ReceiptItems ri ON r.Id = ri.ReceiptId
        LEFT JOIN Products p ON ri.ProductId = p.Id
        LEFT JOIN PaymentMethods pm ON r.PaymentMethodId = pm.Id
        WHERE c.CreatedAt >= @StartDate 
            AND c.CreatedAt <= @EndDate
            AND (c.Email LIKE @EmailPattern OR c.LastName LIKE @NamePattern)
            AND r.TotalAmount > @MinAmount
        ORDER BY c.LastName, c.FirstName, r.PurchaseDate DESC";

    [Benchmark(Description = "Const String - Short SQL")]
    public string ConstString_Short()
    {
        // Simulating what happens when SQL is used in a method
        var sql = ShortSqlConst;
        return sql.GetHashCode().ToString(); // Simulate using the string
    }

    [Benchmark(Description = "Field String - Short SQL")]
    public string FieldString_Short()
    {
        var sql = _shortSqlField;
        return sql.GetHashCode().ToString();
    }

    [Benchmark(Description = "Local String - Short SQL")]
    public string LocalString_Short()
    {
        var sql = "SELECT * FROM Customers WHERE Id = @Id";
        return sql.GetHashCode().ToString();
    }

    [Benchmark(Description = "Const String - Medium SQL")]
    public string ConstString_Medium()
    {
        var sql = MediumSqlConst;
        return sql.GetHashCode().ToString();
    }

    [Benchmark(Description = "Field String - Medium SQL")]
    public string FieldString_Medium()
    {
        var sql = _mediumSqlField;
        return sql.GetHashCode().ToString();
    }

    [Benchmark(Description = "Local String - Medium SQL")]
    public string LocalString_Medium()
    {
        var sql = @"
            SELECT c.*, r.*, p.*
            FROM Customers c
            INNER JOIN Receipts r ON c.Id = r.CustomerId
            INNER JOIN ReceiptItems ri ON r.Id = ri.ReceiptId
            INNER JOIN Products p ON ri.ProductId = p.Id
            WHERE c.Id = @Id";
        return sql.GetHashCode().ToString();
    }

    [Benchmark(Description = "Const String - Long SQL")]
    public string ConstString_Long()
    {
        var sql = LongSqlConst;
        return sql.GetHashCode().ToString();
    }

    [Benchmark(Description = "Field String - Long SQL")]
    public string FieldString_Long()
    {
        var sql = _longSqlField;
        return sql.GetHashCode().ToString();
    }

    [Benchmark(Description = "Local String - Long SQL")]
    public string LocalString_Long()
    {
        var sql = @"
            SELECT 
                c.Id, c.FirstName, c.LastName, c.Email, c.Phone, c.CreatedAt, c.UpdatedAt,
                r.Id as ReceiptId, r.PurchaseDate, r.TotalAmount, r.TaxAmount, r.Notes as ReceiptNotes,
                ri.Id as ItemId, ri.Quantity, ri.UnitPrice, ri.TotalPrice,
                p.Id as ProductId, p.Name as ProductName, p.Description, p.Price, p.StockQuantity, p.SKU,
                pm.Id as PaymentMethodId, pm.Name as PaymentMethodName, pm.Description as PaymentMethodDescription
            FROM Customers c
            LEFT JOIN Receipts r ON c.Id = r.CustomerId
            LEFT JOIN ReceiptItems ri ON r.Id = ri.ReceiptId
            LEFT JOIN Products p ON ri.ProductId = p.Id
            LEFT JOIN PaymentMethods pm ON r.PaymentMethodId = pm.Id
            WHERE c.CreatedAt >= @StartDate 
                AND c.CreatedAt <= @EndDate
                AND (c.Email LIKE @EmailPattern OR c.LastName LIKE @NamePattern)
                AND r.TotalAmount > @MinAmount
            ORDER BY c.LastName, c.FirstName, r.PurchaseDate DESC";
        return sql.GetHashCode().ToString();
    }

    // Test repeated usage in a loop (simulating multiple repository calls)
    [Benchmark(Description = "Const String - Repeated (1000x)")]
    public int ConstString_Repeated()
    {
        int result = 0;
        for (int i = 0; i < 1000; i++)
        {
            var sql = ShortSqlConst;
            result += sql.Length;
        }
        return result;
    }

    [Benchmark(Description = "Field String - Repeated (1000x)")]
    public int FieldString_Repeated()
    {
        int result = 0;
        for (int i = 0; i < 1000; i++)
        {
            var sql = _shortSqlField;
            result += sql.Length;
        }
        return result;
    }

    [Benchmark(Description = "Local String - Repeated (1000x)")]
    public int LocalString_Repeated()
    {
        int result = 0;
        for (int i = 0; i < 1000; i++)
        {
            var sql = "SELECT * FROM Customers WHERE Id = @Id";
            result += sql.Length;
        }
        return result;
    }
}
