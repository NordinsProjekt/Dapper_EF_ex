# Quick Start Guide

## ?? Get Started in 3 Steps

### Step 1: Choose Your Data Provider

When you run the application, you'll be asked:
```
Select Data Provider:
1. Entity Framework Core
2. Dapper
```

- Choose **1** for EF Core (automatic database setup)
- Choose **2** for Dapper (requires manual SQL script)

### Step 2: Database Setup

#### If you chose EF Core (Option 1):
? **Nothing to do!** The database is created automatically.

**Note**: If the database already exists (e.g., created by Dapper previously), EF Core will detect and use the existing schema automatically.

#### If you chose Dapper (Option 2):
1. Open SQL Server Management Studio or Azure Data Studio
2. Connect to `(localdb)\mssqllocaldb`
3. Run the script: `Infrastructure.Dapper\Scripts\InitializeDatabase.sql`

**Note**: The SQL script is **idempotent** - safe to run multiple times. It won't recreate existing tables.

### Step 3: Start Using the System

Navigate through the menu:
```
???????????????????????????????????????
         KIOSK MANAGEMENT SYSTEM       
      Data Provider: EFCore (or Dapper)
???????????????????????????????????????

1. Customer Management
2. Product Management
3. Employee Management
0. Exit
```

## ?? Quick Examples

### Add a Customer
```
1. Customer Management
2. Add New Customer

First Name: Alice
Last Name: Johnson
Email: alice@example.com
Phone: 555-0101
```

### Add a Product
```
2. Product Management
2. Add New Product

Name: Wireless Mouse
Description: Ergonomic wireless mouse
Price: 29.99
Stock Quantity: 50
SKU: MOUSE-001
```

### Add an Employee
```
3. Employee Management
2. Add New Employee

First Name: Bob
Last Name: Wilson
Email: bob@company.com
Hourly Rate: 18.50
Phone: 555-0202
```

## ?? Switching Between Data Providers

You can switch between EF Core and Dapper at any time:

1. Exit the application
2. Run it again
3. Choose a different provider
4. **Your data is still there!** Both use the same database.

**Important**: The application intelligently detects if the database was created by the other provider and adapts accordingly.

## ?? What to Try

1. **Add data with EF Core**: Add some customers and products
2. **Switch to Dapper**: Exit and restart, choose Dapper
3. **See the same data**: List customers - you'll see what you added!
4. **Update with Dapper**: Make some changes
5. **Switch back to EF Core**: Exit and restart, choose EF Core
6. **Verify changes**: Your updates are still there!

This demonstrates that both implementations are truly interchangeable.

## ? Common Questions

**Q: Which provider should I use?**
- **EF Core**: Easier to use, better for complex queries, automatic migrations
- **Dapper**: Faster performance, more control over SQL, lightweight

**Q: Can I use both at the same time?**
- Yes! They both work with the same database. You can switch anytime.

**Q: What if I get an error about the database?**
- For EF Core: The app will detect existing tables and use them automatically
- For Dapper: Make sure you ran the SQL script (it's safe to run multiple times)

**Q: Where is my data stored?**
- In SQL Server LocalDB, database name: `KioskDb`
- Connection string: See `Console\appsettings.json`

**Q: I see "Database schema already exists" - is that OK?**
- Yes! This is normal if you've run the app before or created the database with Dapper
- The app will use the existing schema

## ??? Troubleshooting

### "Cannot open database" error
```bash
# Check if LocalDB is running
sqllocaldb info
sqllocaldb start mssqllocaldb
```

### "Tables already exist" message
This is **not an error**! It means:
- You've already run the app before, OR
- You created the database with one provider and are now using the other

The app will work correctly with the existing tables.

### Want to start fresh?
```sql
-- In SSMS or Azure Data Studio
USE master;
DROP DATABASE KioskDb;
```
Then restart the app with either provider to recreate the database.

### EF Core and Dapper schema conflicts
The schemas are **100% compatible**! Both implementations:
- Use the same table names
- Use the same column names and types
- Use the same foreign keys
- Work with the same data

You can freely switch between them without any issues.

## ?? Next Steps

- Check out `README.md` for detailed documentation
- Review `IMPLEMENTATION_PLAN.md` to see what was built
- Explore the code to learn the architecture

Happy coding! ??
