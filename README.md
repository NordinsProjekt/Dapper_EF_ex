# Kiosk Management System

A demonstration of a multi-layered architecture with **dual database implementation** - supporting both **Entity Framework Core** and **Dapper** with runtime selection.

## Architecture

```
???????????????????????????????????????????????????
?         Presentation.KioskViewer (Console)      ?
?         - Menu System                           ?
?         - User Interface                        ?
???????????????????????????????????????????????????
                  ?
???????????????????????????????????????????????????
?         Application.Services                    ?
?         - Business Logic                        ?
?         - Service Layer                         ?
???????????????????????????????????????????????????
                  ?
        ?????????????????????
        ?                   ?
????????????????    ????????????????
?Infrastructure?    ?Infrastructure?
?   .EFCore    ?    ?   .Dapper    ?
?              ?    ?              ?
? - DbContext  ?    ? - SQL Repos  ?
? - EF Repos   ?    ? - Direct SQL ?
????????????????    ????????????????
       ?                   ?
       ?????????????????????
                 ?
        ???????????????????
        ? Domain.Entities ?
        ? - Customer      ?
        ? - Product       ?
        ? - Employee      ?
        ? - Receipt       ?
        ? - Paycheck      ?
        ? - TimeEntry     ?
        ???????????????????
```

## Features

### Entities
- **Customer**: Manage customer information with email validation
- **Product**: Product catalog with pricing and stock management
- **Employee**: Employee records with hourly rate tracking
- **Receipt**: Sales receipts with customer and payment tracking
- **ReceiptItem**: Individual line items on receipts
- **PaymentMethod**: Different payment types (Cash, Credit Card, Debit Card)
- **TimeEntry**: Employee clock in/out records
- **Paycheck**: Employee payroll records

### Data Access Patterns
- **Entity Framework Core**: Full ORM with change tracking, LINQ support
- **Dapper**: Micro-ORM with raw SQL queries for performance

## Prerequisites

- .NET 9.0 SDK
- SQL Server (LocalDB is used by default)
- Visual Studio 2022 or VS Code (optional)

## Database Setup

### Option 1: Entity Framework Core (Automatic)
The application will automatically create and migrate the database on first run.

**Manual Migration (if needed):**
```bash
cd Infrastructure.EFCore
dotnet ef database update
```

### Option 2: Dapper (Manual SQL Script)
Run the initialization script located at:
```
Infrastructure.Dapper\Scripts\InitializeDatabase.sql
```

You can run this via:
- SQL Server Management Studio
- Azure Data Studio
- sqlcmd command-line tool

```bash
sqlcmd -S "(localdb)\mssqllocaldb" -i "Infrastructure.Dapper\Scripts\InitializeDatabase.sql"
```

## Running the Application

```bash
cd Console
dotnet run
```

You'll be prompted to choose:
1. **Entity Framework Core** - Uses ORM with LINQ
2. **Dapper** - Uses direct SQL queries

## Menu System

### Main Menu
- Customer Management
- Product Management
- Employee Management

### Each Section Supports:
1. **List All** - View all records
2. **Add New** - Create new records
3. **Update** - Modify existing records
4. **Delete** - Remove records

## Project Structure

```
Dapper_EF_ex/
??? Domain.Entities/              # Domain models
?   ??? Customer.cs
?   ??? Product.cs
?   ??? Employee.cs
?   ??? Receipt.cs
?   ??? ReceiptItem.cs
?   ??? PaymentMethod.cs
?   ??? TimeEntry.cs
?   ??? Paycheck.cs
??? Application.Services/          # Business logic
?   ??? Interfaces/
?       ??? IRepository.cs
??? Infrastructure.EFCore/         # EF Core implementation
?   ??? ApplicationDbContext.cs
?   ??? ApplicationDbContextFactory.cs
?   ??? Repositories/
?   ?   ??? Repository.cs
?   ??? Migrations/
??? Infrastructure.Dapper/         # Dapper implementation
?   ??? Repositories/
?   ?   ??? DapperCustomerRepository.cs
?   ?   ??? DapperProductRepository.cs
?   ?   ??? DapperEmployeeRepository.cs
?   ??? Scripts/
?   ?   ??? InitializeDatabase.sql
?   ??? ServiceCollectionExtensions.cs
??? Console/                       # Presentation layer
    ??? Program.cs
    ??? DataProvider.cs
    ??? appsettings.json
    ??? Services/
    ?   ??? CustomerService.cs
    ?   ??? ProductService.cs
    ?   ??? EmployeeService.cs
    ??? UI/
        ??? MainMenu.cs
```

## Configuration

Edit `Console\appsettings.json` to change:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=KioskDb;Trusted_Connection=true;MultipleActiveResultSets=true"
  },
  "AppSettings": {
    "DataProvider": "EFCore"
  }
}
```

## Design Patterns Used

- ? **Repository Pattern**: Abstract data access
- ? **Dependency Injection**: Loose coupling
- ? **Strategy Pattern**: Swappable data providers
- ? **Unit of Work**: Transaction management (EF Core)
- ? **Factory Pattern**: DbContext creation

## Key Differences: EF Core vs Dapper

| Feature | EF Core | Dapper |
|---------|---------|--------|
| Type | Full ORM | Micro-ORM |
| Queries | LINQ | Raw SQL |
| Change Tracking | Yes | No |
| Performance | Good | Excellent |
| Learning Curve | Higher | Lower |
| Migrations | Built-in | Manual |

## Sample Operations

### Add a Customer
```
First Name: John
Last Name: Doe
Email: john.doe@example.com
Phone: 555-1234
```

### Add a Product
```
Name: Laptop
Description: Gaming laptop
Price: 1299.99
Stock Quantity: 10
SKU: LAP-001
```

### Add an Employee
```
First Name: Jane
Last Name: Smith
Email: jane.smith@company.com
Hourly Rate: 25.50
Phone: 555-5678
```

## Testing Both Implementations

1. Run the application with **EF Core** selected
2. Add some customers, products, and employees
3. Exit and restart the application
4. Select **Dapper** this time
5. Verify you can see and modify the same data!

This demonstrates that both implementations work with the same database schema and are truly interchangeable.

## Troubleshooting

### "Database does not exist" error
- For EF Core: The app should create it automatically
- For Dapper: Run the SQL initialization script

### Migration issues
```bash
cd Infrastructure.EFCore
dotnet ef migrations remove
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### Connection string issues
Check that SQL Server LocalDB is installed:
```bash
sqllocaldb info
sqllocaldb start mssqllocaldb
```

## Future Enhancements

- [ ] Add Receipt/Sales management in UI
- [ ] Add TimeEntry tracking in UI
- [ ] Add Paycheck generation in UI
- [ ] Add reporting features
- [ ] Add data validation
- [ ] Add unit tests
- [ ] Add API layer (REST API)
- [ ] Add Blazor UI

## License

This is a demonstration project for educational purposes.
