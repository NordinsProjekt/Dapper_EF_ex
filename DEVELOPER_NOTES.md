# Developer Notes

## Project Structure Explained

### Why This Architecture?

This project uses **Onion Architecture** (also called Clean Architecture) principles:

```
???????????????????????????????????????????
?         Presentation Layer              ? ? Console UI, Menu System
?         (Presentation.KioskViewer)      ?
???????????????????????????????????????????
                 ?
???????????????????????????????????????????
?         Application Layer               ? ? Services, Business Logic
?         (Application.Services)          ?
???????????????????????????????????????????
                 ?
        ???????????????????
        ?                 ?
?????????????????  ?????????????????
?Infrastructure ?  ?Infrastructure ?      ? Data Access
?   .EFCore     ?  ?   .Dapper     ?
?????????????????  ?????????????????
        ?                  ?
        ????????????????????
                 ?
        ???????????????????
        ? Domain.Entities ?               ? Core Domain
        ???????????????????
```

### Dependencies Flow Inward
- Presentation depends on Application, Infrastructure
- Application depends on Domain
- Infrastructure depends on Domain
- **Domain depends on nothing!**

## Implementation Decisions

### 1. Why Both EF Core AND Dapper?

**Educational Purpose**: To demonstrate:
- Different approaches to data access
- How to design for interchangeable implementations
- Repository pattern in practice
- Strategy pattern for provider selection

**Real-World Scenario**: 
- Some apps need EF Core's convenience (complex queries, migrations)
- Others need Dapper's performance (high-volume, specific queries)
- This shows how to support both!

### 2. Why Use IRepository<T>?

The generic repository pattern:
- ? Standardizes data access operations
- ? Reduces code duplication
- ? Makes testing easier (can mock the interface)
- ? Allows swapping implementations

**Note**: In production, you might want specific repositories:
```csharp
public interface ICustomerRepository : IRepository<Customer>
{
    Task<Customer?> GetByEmailAsync(string email);
    Task<IEnumerable<Customer>> GetActiveCustomersAsync();
}
```

### 3. Entity Relationships

The domain model includes several relationship types:

**One-to-Many**:
- Customer ? Receipts
- Employee ? TimeEntries
- Employee ? Paychecks
- Receipt ? ReceiptItems

**Many-to-One**:
- Receipt ? Customer
- Receipt ? PaymentMethod
- ReceiptItem ? Product

**Design Note**: We used `Guid` as primary keys for:
- Better distribution in distributed systems
- Easier merging of data
- No need for identity management

### 4. Why Service Layer?

The Services (CustomerService, ProductService, EmployeeService) provide:
- **Business logic encapsulation**: Rules live here, not in UI or repositories
- **Transaction coordination**: Can orchestrate multiple repository calls
- **DTO mapping** (future): Can convert entities to/from DTOs
- **Validation** (future): Can add validation logic

Example expansion:
```csharp
public async Task<Receipt> CreateReceiptAsync(Guid customerId, List<ReceiptItem> items)
{
    // Validate customer exists
    var customer = await _customerRepo.GetByIdAsync(customerId);
    if (customer == null) throw new InvalidOperationException("Customer not found");
    
    // Validate products and calculate total
    decimal total = 0;
    foreach (var item in items)
    {
        var product = await _productRepo.GetByIdAsync(item.ProductId);
        if (product == null) throw new InvalidOperationException($"Product {item.ProductId} not found");
        if (product.StockQuantity < item.Quantity) throw new InvalidOperationException("Insufficient stock");
        
        item.UnitPrice = product.Price;
        item.TotalPrice = item.Quantity * product.Price;
        total += item.TotalPrice;
        
        // Reduce stock
        product.StockQuantity -= item.Quantity;
        await _productRepo.UpdateAsync(product);
    }
    
    // Create receipt
    var receipt = new Receipt { ... };
    return await _receiptRepo.AddAsync(receipt);
}
```

## Technical Details

### EF Core Implementation

**Pros**:
- Change tracking (automatic updates)
- LINQ queries (type-safe)
- Migrations (version control for schema)
- Navigation properties (lazy/eager loading)

**Cons**:
- Heavier memory footprint
- Can be slower for simple queries
- Learning curve for complex scenarios

**Key Files**:
- `ApplicationDbContext.cs`: DbContext with all entities
- `ApplicationDbContextFactory.cs`: For design-time operations (migrations)
- `Repository.cs`: Generic repository using EF Core
- `Migrations/`: Auto-generated migration files

### Dapper Implementation

**Pros**:
- Blazing fast (micro-ORM)
- Direct SQL control
- Minimal overhead
- Easy to optimize

**Cons**:
- No change tracking
- Manual SQL writing
- No automatic migrations
- More boilerplate code

**Key Files**:
- `DapperCustomerRepository.cs`: Customer operations with SQL
- `DapperProductRepository.cs`: Product operations with SQL
- `DapperEmployeeRepository.cs`: Employee operations with SQL
- `Scripts/InitializeDatabase.sql`: Manual schema script

### Why Expression Trees Don't Work in Dapper

```csharp
public async Task<IEnumerable<Customer>> FindAsync(
    Expression<Func<Customer, bool>> predicate, 
    CancellationToken cancellationToken = default)
{
    // EF Core: Can translate expression to SQL
    return await _dbSet.Where(predicate).ToListAsync();
    
    // Dapper: Cannot translate expression to SQL
    // Options:
    // 1. Return all and filter in memory (not ideal)
    // 2. Create specific query methods instead
    // 3. Use a library like SqlKata
}
```

**Our approach**: Return all records for Dapper. In production, add specific methods:
```csharp
Task<Customer?> GetByEmailAsync(string email);
Task<IEnumerable<Customer>> GetByLastNameAsync(string lastName);
```

## Extending the Project

### Adding a New Entity

1. **Create Entity** in `Domain.Entities`:
```csharp
public class Category : IEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
```

2. **Add to EF Core**:
```csharp
// ApplicationDbContext.cs
public DbSet<Category> Categories { get; set; }

// OnModelCreating
modelBuilder.Entity<Category>(entity => {
    entity.HasKey(e => e.Id);
    entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
});
```

3. **Create Migration**:
```bash
cd Infrastructure.EFCore
dotnet ef migrations add AddCategory
```

4. **Add to Dapper**:
```csharp
// DapperCategoryRepository.cs
public class DapperCategoryRepository : IRepository<Category> { ... }

// Add to InitializeDatabase.sql
CREATE TABLE Categories (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL
);
```

5. **Add Service**:
```csharp
public class CategoryService
{
    private readonly IRepository<Category> _repository;
    // ... CRUD operations
}
```

6. **Add to UI**:
```csharp
// MainMenu.cs
Console.WriteLine("4. Category Management");
// ... menu implementation
```

### Adding Business Logic

Example: Email customer after purchase
```csharp
public class CustomerService
{
    private readonly IRepository<Customer> _repository;
    private readonly IEmailService _emailService;
    
    public async Task<Receipt> CompletePurchaseAsync(Receipt receipt)
    {
        var customer = await _repository.GetByIdAsync(receipt.CustomerId);
        
        // Send email
        await _emailService.SendReceiptAsync(customer.Email, receipt);
        
        return receipt;
    }
}
```

## Testing Strategies

### Unit Testing Repositories
```csharp
[Fact]
public async Task AddCustomer_ShouldCreateNewCustomer()
{
    // Arrange
    var options = new DbContextOptionsBuilder<ApplicationDbContext>()
        .UseInMemoryDatabase("TestDb")
        .Options;
    var context = new ApplicationDbContext(options);
    var repository = new Repository<Customer>(context);
    
    var customer = new Customer { 
        FirstName = "Test", 
        LastName = "User" 
    };
    
    // Act
    var result = await repository.AddAsync(customer);
    
    // Assert
    Assert.NotEqual(Guid.Empty, result.Id);
}
```

### Integration Testing
```csharp
[Fact]
public async Task EFCore_And_Dapper_ShouldAccessSameData()
{
    // Arrange: Add with EF Core
    var efRepo = new Repository<Customer>(efContext);
    var customer = await efRepo.AddAsync(new Customer { ... });
    
    // Act: Retrieve with Dapper
    var dapperRepo = new DapperCustomerRepository(connectionString);
    var retrieved = await dapperRepo.GetByIdAsync(customer.Id);
    
    // Assert
    Assert.NotNull(retrieved);
    Assert.Equal(customer.Email, retrieved.Email);
}
```

## Performance Considerations

### EF Core Optimization
```csharp
// Use AsNoTracking for read-only queries
var customers = await _context.Customers
    .AsNoTracking()
    .ToListAsync();

// Use eager loading to avoid N+1
var receipts = await _context.Receipts
    .Include(r => r.Customer)
    .Include(r => r.Items)
        .ThenInclude(i => i.Product)
    .ToListAsync();
```

### Dapper Optimization
```csharp
// Use multi-mapping for joins
var receipts = await connection.QueryAsync<Receipt, Customer, Receipt>(
    @"SELECT r.*, c.* 
      FROM Receipts r 
      INNER JOIN Customers c ON r.CustomerId = c.Id",
    (receipt, customer) => {
        receipt.Customer = customer;
        return receipt;
    },
    splitOn: "Id"
);
```

## Common Pitfalls

### 1. Forgetting SaveChanges in EF Core
```csharp
// ? Wrong
public async Task UpdateAsync(Customer entity)
{
    _dbSet.Update(entity);
    // Missing SaveChanges!
}

// ? Correct
public async Task UpdateAsync(Customer entity)
{
    _dbSet.Update(entity);
    await _context.SaveChangesAsync();
}
```

### 2. SQL Injection in Dapper
```csharp
// ? Wrong
var sql = $"SELECT * FROM Customers WHERE Email = '{email}'";

// ? Correct
var sql = "SELECT * FROM Customers WHERE Email = @Email";
await connection.QueryAsync<Customer>(sql, new { Email = email });
```

### 3. Not Disposing Connections
```csharp
// ? Wrong
var connection = new SqlConnection(connectionString);
await connection.QueryAsync<Customer>("SELECT * FROM Customers");
// Connection not disposed!

// ? Correct
using var connection = new SqlConnection(connectionString);
await connection.QueryAsync<Customer>("SELECT * FROM Customers");
```

## Additional Resources

- [EF Core Documentation](https://docs.microsoft.com/en-us/ef/core/)
- [Dapper Documentation](https://github.com/DapperLib/Dapper)
- [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [Repository Pattern](https://docs.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/infrastructure-persistence-layer-design)

## Questions?

This is a learning project. Experiment, break things, and learn how it all works together!
