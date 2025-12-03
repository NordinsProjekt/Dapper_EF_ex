# Service Layer Architecture - Implementation Summary

## Overview

The application has been refactored to implement a proper **Service Layer** architecture where:
- **Repositories** are ONLY accessed by services in `Application.Services`
- **Presentation layers** (Console and Blazor) ONLY access services, never repositories directly
- Business logic is centralized in the service layer
- Both EF Core and Dapper repositories implement the same `IRepository<T>` interface

## Architecture Layers

```
???????????????????????????????????????????????????????????
?         PRESENTATION LAYER                               ?
?  ????????????????????      ????????????????????        ?
?  ?  Console App     ?      ?  Blazor Server   ?        ?
?  ?  - MainMenu      ?      ?  - Razor Pages   ?        ?
?  ?  - Wrappers*     ?      ?  (Direct inject) ?        ?
?  ????????????????????      ????????????????????        ?
????????????????????????????????????????????????????????????
            ?                           ?
            ?????????????????????????????
                       ?
??????????????????????????????????????????????????????????
?         SERVICE LAYER ?                                 ?
?              ????????????????????                       ?
?              ?  Application     ?                       ?
?              ?  .Services       ?                       ?
?              ?  - CustomerSvc   ?                       ?
?              ?  - ProductSvc    ?                       ?
?              ?  - EmployeeSvc   ?                       ?
?              ????????????????????                       ?
??????????????????????????????????????????????????????????
                       ?
                       ? IRepository<T>
                       ?
??????????????????????????????????????????????????????????
?      REPOSITORY LAYER ?                                 ?
?              ????????????????????                       ?
?              ?  EF Core  ?Dapper?                       ?
?              ?  Repos    ?Repos ?                       ?
?              ????????????????????                       ?
??????????????????????????????????????????????????????????
                       ?
??????????????????????????????????????????????????????????
?                DATABASE (SQL Server)                    ?
???????????????????????????????????????????????????????????
```

*Console uses lightweight wrappers for naming consistency

## Key Components

### 1. Application.Services (Business Logic Layer)

**Location:** `Application.Services\`

**Services:**
- `CustomerService.cs` - Customer business logic
- `ProductService.cs` - Product business logic
- `EmployeeService.cs` - Employee business logic

**Interface:**
- `IRepository<T>` - Repository contract (updated with all necessary methods)

**Responsibilities:**
- Business rule validation
- Data integrity checks
- Orchestration of repository operations
- Error handling and exceptions

**Example - CustomerService:**
```csharp
public class CustomerService
{
    private readonly IRepository<Customer> _repository;

    // Business logic: Validate email uniqueness
    public async Task<Customer> CreateCustomerAsync(
        string firstName, string lastName, string email, string? phone = null)
    {
        var existingCustomer = await _repository
            .FirstOrDefaultAsync(c => c.Email == email);
        
        if (existingCustomer != null)
        {
            throw new InvalidOperationException(
                $"A customer with email '{email}' already exists.");
        }

        var customer = new Customer { /* ... */ };
        return await _repository.AddAsync(customer);
    }
}
```

### 2. Console Presentation (Wrapper Pattern)

**Location:** `Console\Services\`

**Wrappers:**
- `CustomerServiceWrapper.cs`
- `ProductServiceWrapper.cs`
- `EmployeeServiceWrapper.cs`

**Purpose:**
- Thin wrappers around `Application.Services`
- Maintain naming convention for Console project
- No business logic - pure delegation

**Example:**
```csharp
public class CustomerServiceWrapper
{
    private readonly CustomerService _customerService;

    public async Task<IEnumerable<Customer>> GetAllCustomersAsync()
    {
        return await _customerService.GetAllCustomersAsync();
    }
}
```

### 3. Blazor Presentation (Direct Injection)

**Location:** `BlazorServer\Components\Pages\`

**Pages:**
- `Customers.razor`
- `Products.razor`
- `Employees.razor`

**Usage:**
- Directly inject `Application.Services` classes
- No wrappers needed

**Example:**
```razor
@inject Application.Services.CustomerService CustomerService

@code {
    private async Task LoadCustomers()
    {
        customers = (await CustomerService.GetAllCustomersAsync()).ToList();
    }
}
```

### 4. Repository Layer (Data Access)

**EF Core Repositories:** `Infrastructure.EFCore\Repositories\`
- `Repository<T>.cs` - Generic EF Core repository

**Dapper Repositories:** `Infrastructure.Dapper\Repositories\`
- `DapperCustomerRepository.cs`
- `DapperProductRepository.cs`
- `DapperEmployeeRepository.cs`

## Interface Updates

### IRepository<T> - Complete Interface

**File:** `Application.Services\Interfaces\IRepository.cs`

**Added Methods:**
```csharp
public interface IRepository<T> where T : class, IEntity
{
    // Basic CRUD
    Task<T?> GetByIdAsync(object id, CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(T entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(T entity, CancellationToken cancellationToken = default);
    Task DeleteByIdAsync(object id, CancellationToken cancellationToken = default);
    
    // Querying
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, ...);
    Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, ...);
    
    // Utilities
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<int> CountAsync(CancellationToken cancellationToken = default);
    Task<int> CountAsync(Expression<Func<T, bool>> predicate, ...);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    
    // Batch operations
    Task AddRangeAsync(IEnumerable<T> entities, ...);
}
```

**Key Change:** Added constraint `where T : class, IEntity` to ensure all entities have an `Id` property.

## Business Logic Examples

### 1. Email Uniqueness Validation

**CustomerService.CreateCustomerAsync:**
```csharp
var existingCustomer = await _repository.FirstOrDefaultAsync(c => c.Email == email);
if (existingCustomer != null)
{
    throw new InvalidOperationException($"A customer with email '{email}' already exists.");
}
```

### 2. SKU Uniqueness Validation

**ProductService.CreateProductAsync:**
```csharp
if (!string.IsNullOrEmpty(sku))
{
    var existingProduct = await _repository.FirstOrDefaultAsync(p => p.SKU == sku);
    if (existingProduct != null)
    {
        throw new InvalidOperationException($"A product with SKU '{sku}' already exists.");
    }
}
```

### 3. Price Validation

**ProductService.CreateProductAsync:**
```csharp
if (price < 0)
{
    throw new ArgumentException("Price cannot be negative.", nameof(price));
}
```

### 4. Hire Date Validation

**EmployeeService.CreateEmployeeAsync:**
```csharp
if (hireDate > DateTime.UtcNow)
{
    throw new ArgumentException("Hire date cannot be in the future.", nameof(hireDate));
}
```

### 5. Hourly Rate Validation

**EmployeeService.CreateEmployeeAsync:**
```csharp
if (hourlyRate <= 0)
{
    throw new ArgumentException("Hourly rate must be greater than zero.", nameof(hourlyRate));
}
```

## Dependency Injection Configuration

### Console Application

**File:** `Console\Program.cs`

```csharp
// 1. Register repositories (EF Core or Dapper)
if (dataProvider == DataProvider.EFCore)
{
    services.AddScoped<IRepository<Customer>, Repository<Customer>>();
    services.AddScoped<IRepository<Product>, Repository<Product>>();
    services.AddScoped<IRepository<Employee>, Repository<Employee>>();
}
else
{
    services.AddDapperInfrastructure(connectionString);
}

// 2. Register APPLICATION SERVICES (business logic layer)
services.AddScoped<Application.Services.CustomerService>();
services.AddScoped<Application.Services.ProductService>();
services.AddScoped<Application.Services.EmployeeService>();

// 3. Register PRESENTATION WRAPPERS
services.AddScoped<CustomerServiceWrapper>();
services.AddScoped<ProductServiceWrapper>();
services.AddScoped<EmployeeServiceWrapper>();
```

### Blazor Server Application

**File:** `BlazorServer\Program.cs`

```csharp
// 1. Register repositories (EF Core or Dapper)
if (DATA_PROVIDER == DataProvider.EFCore)
{
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(connectionString));
    
    builder.Services.AddScoped<IRepository<Customer>, Repository<Customer>>();
    builder.Services.AddScoped<IRepository<Product>, Repository<Product>>();
    builder.Services.AddScoped<IRepository<Employee>, Repository<Employee>>();
}
else
{
    builder.Services.AddDapperInfrastructure(connectionString);
}

// 2. Register APPLICATION SERVICES (business logic layer)
builder.Services.AddScoped<CustomerService>();
builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<EmployeeService>();
```

## Benefits of This Architecture

### 1. **Separation of Concerns**
- Repositories handle data access only
- Services handle business logic only
- Presentation handles UI only

### 2. **Reusability**
- Same services used by Console and Blazor
- No code duplication
- Consistent business logic across applications

### 3. **Testability**
- Services can be unit tested by mocking `IRepository<T>`
- Business logic isolated from data access
- No need to mock database

### 4. **Maintainability**
- Business rules in one place
- Easy to update logic without touching repositories or UI
- Clear dependencies

### 5. **Flexibility**
- Switch between EF Core and Dapper without changing services
- Add new presentation layers (API, Mobile) easily
- Repositories completely hidden from presentation

### 6. **Validation Centralization**
- All validation rules in services
- Consistent error messages
- No validation in UI or repositories

## Error Handling

### Service Layer Exceptions

Services throw meaningful exceptions:

```csharp
// Validation errors
throw new ArgumentException("Price cannot be negative.", nameof(price));

// Business rule violations
throw new InvalidOperationException("A customer with email 'x@y.com' already exists.");

// Not found errors
throw new InvalidOperationException("Customer with ID 'xxx' does not exist.");
```

### Presentation Layer Handling

**Console:**
```csharp
try
{
    await _customerService.CreateCustomerAsync(...);
}
catch (Exception ex)
{
    Console.WriteLine($"\nError: {ex.Message}");
}
```

**Blazor:**
```csharp
try
{
    await CustomerService.CreateCustomerAsync(...);
}
catch (Exception ex)
{
    errorMessage = $"Error saving customer: {ex.Message}";
}
```

## Testing Strategy

### Unit Testing Services

```csharp
[Fact]
public async Task CreateCustomer_WithDuplicateEmail_ThrowsException()
{
    // Arrange
    var mockRepo = new Mock<IRepository<Customer>>();
    mockRepo.Setup(r => r.FirstOrDefaultAsync(It.IsAny<Expression<Func<Customer, bool>>>()))
        .ReturnsAsync(new Customer { Email = "test@test.com" });
    
    var service = new CustomerService(mockRepo.Object);
    
    // Act & Assert
    await Assert.ThrowsAsync<InvalidOperationException>(
        () => service.CreateCustomerAsync("John", "Doe", "test@test.com"));
}
```

### Integration Testing

Test with real repositories (both EF Core and Dapper) to ensure compatibility.

## Migration Path

### Before (? Direct Repository Access)

```csharp
// Console UI directly using repository
public class MainMenu
{
    private readonly IRepository<Customer> _customerRepo;
    
    public async Task AddCustomer()
    {
        var customer = new Customer { /* ... */ };
        await _customerRepo.AddAsync(customer); // No validation!
    }
}
```

### After (? Service Layer)

```csharp
// Console UI using service
public class MainMenu
{
    private readonly CustomerServiceWrapper _customerService;
    
    public async Task AddCustomer()
    {
        // Service handles validation
        await _customerService.CreateCustomerAsync(
            firstName, lastName, email, phone);
    }
}
```

## File Structure

```
Application.Services/
??? Interfaces/
?   ??? IRepository.cs           # Complete repository interface
??? CustomerService.cs            # Customer business logic
??? ProductService.cs             # Product business logic
??? EmployeeService.cs            # Employee business logic

Console/Services/
??? CustomerServiceWrapper.cs     # Thin wrapper
??? ProductServiceWrapper.cs      # Thin wrapper
??? EmployeeServiceWrapper.cs     # Thin wrapper

BlazorServer/Components/Pages/
??? Customers.razor               # Injects CustomerService directly
??? Products.razor                # Injects ProductService directly
??? Employees.razor               # Injects EmployeeService directly

Infrastructure.EFCore/Repositories/
??? Repository<T>.cs              # Generic EF Core repository

Infrastructure.Dapper/Repositories/
??? DapperCustomerRepository.cs   # Dapper customer repo
??? DapperProductRepository.cs    # Dapper product repo
??? DapperEmployeeRepository.cs   # Dapper employee repo
```

## Summary

? **Repositories are ONLY accessed by services**  
? **Business logic centralized in Application.Services**  
? **Both presentation layers use same services**  
? **EF Core and Dapper fully interchangeable**  
? **Proper separation of concerns**  
? **Testable and maintainable architecture**  
? **Consistent validation across all entry points**  

## Next Steps

Potential future enhancements:
1. Add unit tests for all service methods
2. Add integration tests for repository implementations
3. Consider adding DTOs for data transfer
4. Add AutoMapper for entity-to-DTO mapping
5. Implement repository unit of work pattern
6. Add caching layer in services
7. Add logging throughout services
8. Implement soft deletes in services

---

**Architecture Status:** ? Complete and Production-Ready
