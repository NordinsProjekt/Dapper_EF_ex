using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;

namespace Benchmarks;

/// <summary>
/// Benchmarks comparing string allocation and memory usage patterns
/// </summary>
[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
public class StringMemoryBenchmarks
{
    private const string ConstShortString = "SELECT * FROM Customers WHERE Id = @Id";
    private readonly string _fieldShortString = "SELECT * FROM Customers WHERE Id = @Id";
    
    private const string ConstLongString = @"
        SELECT c.Id, c.FirstName, c.LastName, c.Email, c.Phone, c.CreatedAt, c.UpdatedAt,
               r.Id as ReceiptId, r.PurchaseDate, r.TotalAmount, r.TaxAmount,
               p.Id as ProductId, p.Name, p.Price, p.StockQuantity
        FROM Customers c
        LEFT JOIN Receipts r ON c.Id = r.CustomerId
        LEFT JOIN ReceiptItems ri ON r.Id = ri.ReceiptId
        LEFT JOIN Products p ON ri.ProductId = p.Id
        WHERE c.CreatedAt >= @StartDate AND c.CreatedAt <= @EndDate
        ORDER BY c.LastName, c.FirstName";
    
    private readonly string _fieldLongString = @"
        SELECT c.Id, c.FirstName, c.LastName, c.Email, c.Phone, c.CreatedAt, c.UpdatedAt,
               r.Id as ReceiptId, r.PurchaseDate, r.TotalAmount, r.TaxAmount,
               p.Id as ProductId, p.Name, p.Price, p.StockQuantity
        FROM Customers c
        LEFT JOIN Receipts r ON c.Id = r.CustomerId
        LEFT JOIN ReceiptItems ri ON r.Id = ri.ReceiptId
        LEFT JOIN Products p ON ri.ProductId = p.Id
        WHERE c.CreatedAt >= @StartDate AND c.CreatedAt <= @EndDate
        ORDER BY c.LastName, c.FirstName";

    [Benchmark(Description = "Const - String Concatenation (Short)")]
    public string Const_StringConcat_Short()
    {
        return ConstShortString + " AND Active = 1";
    }

    [Benchmark(Description = "Field - String Concatenation (Short)")]
    public string Field_StringConcat_Short()
    {
        return _fieldShortString + " AND Active = 1";
    }

    [Benchmark(Description = "Local - String Concatenation (Short)")]
    public string Local_StringConcat_Short()
    {
        var sql = "SELECT * FROM Customers WHERE Id = @Id";
        return sql + " AND Active = 1";
    }

    [Benchmark(Description = "Const - String Interpolation")]
    public string Const_StringInterpolation()
    {
        var tableName = "Customers";
        return $"SELECT * FROM {tableName} WHERE Id = @Id";
    }

    [Benchmark(Description = "Local - String Interpolation")]
    public string Local_StringInterpolation()
    {
        var sql = "SELECT * FROM {0} WHERE Id = @Id";
        var tableName = "Customers";
        return $"SELECT * FROM {tableName} WHERE Id = @Id";
    }

    [Benchmark(Description = "Const - Multiple References")]
    public int Const_MultipleReferences()
    {
        var sql1 = ConstShortString;
        var sql2 = ConstShortString;
        var sql3 = ConstShortString;
        return sql1.Length + sql2.Length + sql3.Length;
    }

    [Benchmark(Description = "Field - Multiple References")]
    public int Field_MultipleReferences()
    {
        var sql1 = _fieldShortString;
        var sql2 = _fieldShortString;
        var sql3 = _fieldShortString;
        return sql1.Length + sql2.Length + sql3.Length;
    }

    [Benchmark(Description = "Local - Multiple References")]
    public int Local_MultipleReferences()
    {
        var sql1 = "SELECT * FROM Customers WHERE Id = @Id";
        var sql2 = "SELECT * FROM Customers WHERE Id = @Id";
        var sql3 = "SELECT * FROM Customers WHERE Id = @Id";
        return sql1.Length + sql2.Length + sql3.Length;
    }

    // Test string interning behavior
    [Benchmark(Description = "String Interning - Const")]
    public bool StringInterning_Const()
    {
        var sql1 = ConstShortString;
        var sql2 = ConstShortString;
        return ReferenceEquals(sql1, sql2); // Should be true (same reference)
    }

    [Benchmark(Description = "String Interning - Field")]
    public bool StringInterning_Field()
    {
        var sql1 = _fieldShortString;
        var sql2 = _fieldShortString;
        return ReferenceEquals(sql1, sql2); // Should be true (same reference)
    }

    [Benchmark(Description = "String Interning - Local")]
    public bool StringInterning_Local()
    {
        var sql1 = "SELECT * FROM Customers WHERE Id = @Id";
        var sql2 = "SELECT * FROM Customers WHERE Id = @Id";
        return ReferenceEquals(sql1, sql2); // Might be true (compiler optimization)
    }
}
