# Dapper Auto-Initialization Fix

## Problem
When selecting **Dapper** as the data provider after deleting the database, the application would fail with:
```
Error: Cannot open database "KioskDb" requested by the login. The login failed.
Login failed for user 'DESKTOP-MBPUR5V\belfe'.
```

## Root Cause
The application only showed a warning message for Dapper users to manually run the initialization script, but it didn't automatically create the database like EF Core does.

## Solution
Updated `Console\Program.cs` to automatically:
1. **Check if the database exists** when Dapper is selected
2. **Automatically run the initialization script** (`Infrastructure.Dapper\Scripts\InitializeDatabase.sql`) if the database doesn't exist
3. **Create all 8 required tables** with proper schema and seed data

## Changes Made

### Added Methods to `Program.cs`:

#### 1. `CheckDatabaseExistsAsync()`
```csharp
private static async Task<bool> CheckDatabaseExistsAsync(string connectionString)
```
- Connects to the `master` database
- Queries `sys.databases` to check if `KioskDb` exists
- Returns `true` if database exists, `false` otherwise

#### 2. `InitializeDapperDatabaseAsync()`
```csharp
private static async Task InitializeDapperDatabaseAsync(string connectionString)
```
- Reads `Infrastructure.Dapper\Scripts\InitializeDatabase.sql`
- Splits the script by `GO` statements into batches
- Executes each batch sequentially
- Creates database and all 8 tables with seed data

### Updated `InitializeDatabaseAsync()`
- Added Dapper-specific initialization logic
- Automatically detects missing database
- Runs initialization script without user intervention
- Provides clear feedback during the process

## How It Works Now

### Scenario 1: Fresh Installation (No Database)
```
Select Data Provider:
1. Entity Framework Core
2. Dapper

Enter choice (1 or 2): 2

Using Dapper as data provider...

Initializing database...
Checking if database exists...
? Database does not exist.
Creating database using Dapper initialization script...

? Dapper database created and initialized successfully.
```

### Scenario 2: Database Already Exists
```
Select Data Provider:
1. Entity Framework Core
2. Dapper

Enter choice (1 or 2): 2

Using Dapper as data provider...

Initializing database...
Checking if database exists...
? Database already exists.
```

## Benefits

? **Automatic Database Creation** - No manual SQL script execution needed
? **Consistent Experience** - Both EF Core and Dapper work the same way
? **Developer Friendly** - Just run the app and select Dapper
? **Error Prevention** - Prevents the "database not found" error
? **Idempotent** - Safe to run multiple times (script checks if objects exist)

## Testing the Fix

### Test 1: After Database Deletion
1. Delete the database using `CompleteReset.sql`
2. Run the Console application
3. Select option **2 (Dapper)**
4. ? Database should be created automatically
5. Try listing customers (should work, even if empty)

### Test 2: With Existing Database
1. Ensure database exists (from previous run)
2. Run the Console application
3. Select option **2 (Dapper)**
4. ? Should detect existing database and continue
5. All functionality should work normally

### Test 3: EF Core Still Works
1. Delete the database using `CompleteReset.sql`
2. Run the Console application
3. Select option **1 (EF Core)**
4. ? EF Core should create database as before
5. All functionality should work normally

## Error Handling

The fix includes robust error handling:

```csharp
catch (Exception ex)
{
    Console.WriteLine($"? Warning: Could not initialize Dapper database: {ex.Message}");
    Console.WriteLine("  You may need to manually run the initialization script:");
    Console.WriteLine("  Infrastructure.Dapper\\Scripts\\InitializeDatabase.sql\n");
}
```

If automatic initialization fails:
- Clear error message is displayed
- Fallback instructions are provided
- Application continues (user can fix manually)

## Technical Details

### Connection String Handling
```csharp
var builder = new SqlConnectionStringBuilder(connectionString);
var databaseName = builder.InitialCatalog;
builder.InitialCatalog = "master"; // Connect to master to create database
```

### Script Parsing
```csharp
var batches = script.Split(new[] { "\nGO", "\rGO", "\r\nGO" }, 
    StringSplitOptions.RemoveEmptyEntries);
```
Handles different line ending formats (Windows/Unix).

### Script Path Resolution
```csharp
var scriptPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, 
    "..", "..", "..", "..", "Infrastructure.Dapper", "Scripts", "InitializeDatabase.sql");
```
Works from the Console project's bin directory.

## Related Files

- **Modified**: `Console\Program.cs`
- **Used**: `Infrastructure.Dapper\Scripts\InitializeDatabase.sql`
- **Reference**: `Infrastructure.Dapper\Scripts\CompleteReset.sql` (for testing)

## Future Enhancements

Potential improvements:
1. Add progress indicator for each table creation
2. Validate all 8 tables exist after initialization
3. Option to re-run initialization if tables are missing
4. Log initialization output to file for debugging

## Conclusion

This fix ensures that **Dapper works seamlessly** just like EF Core, providing a consistent developer experience regardless of which data provider is chosen. No more manual script execution required! ??
