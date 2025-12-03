# Troubleshooting: Database Issues

## Problem 1: Database Not Found (Dapper)

### ? **FIXED** - Now Automatically Handled!

When running the application with Dapper after deleting the database, you used to see:
```
Error: Cannot open database "KioskDb" requested by the login. The login failed.
Login failed for user 'DESKTOP-MBPUR5V\belfe'.
```

### Solution: Automatic Database Creation

**The application now creates the database automatically!**

When you select Dapper and the database doesn't exist, you'll see:
```
Initializing database...
Checking if database exists...
? Database does not exist.
Creating database using Dapper initialization script...

? Dapper database created and initialized successfully.
```

**No manual steps required!** Just run the app and select Dapper.

### What Happens Behind the Scenes
1. App detects database doesn't exist
2. Reads `Infrastructure.Dapper\Scripts\InitializeDatabase.sql`
3. Executes the script to create database and all 8 tables
4. Inserts seed data (PaymentMethods)
5. Continues normally

### If Auto-Creation Fails
If you see an error during automatic creation, you can manually run:
```bash
sqlcmd -S "(localdb)\mssqllocaldb" -i "Infrastructure.Dapper\Scripts\InitializeDatabase.sql"
```

---

## Problem 2: Database Already Exists (EF Core)

### Scenario 1: Fresh Database (No Database Exists)
```
Initializing database...
? EF Core database created and initialized successfully.
```

### Scenario 2: Database Created by Dapper (Tables Exist)
```
Initializing database...
? Database schema already exists.
? EF Core is using the existing database schema.
```

### Scenario 3: Database Already Has EF Core Migrations
```
Initializing database...
? EF Core database is up to date.
```

### Scenario 4: Migration History Incomplete (The Problem You Had)
```
Initializing database...
? Database schema already exists.
[Automatically updating migration history]
? EF Core is using the existing database schema.
```

## Step-by-Step Resolution

### If You See the Error Right Now:

**Step 1: Close the application** (if it's still running)

**Step 2: Choose your approach:**

#### Approach A: Let the app fix it (Easiest)
```bash
# Just run the app again
cd Console
dotnet run
# Choose option 1 (EF Core)
```
The app will detect the issue and fix it automatically.

#### Approach B: Start fresh (Clean slate)
```bash
# Run in PowerShell or Command Prompt
sqlcmd -S "(localdb)\mssqllocaldb" -i "Infrastructure.Dapper\Scripts\ResetDatabase.sql"

# Then run the app
cd Console
dotnet run
# Choose option 1 (EF Core)
```

#### Approach C: Manual database fix (Keep your data)
1. Open the manual fix SQL above in SSMS or Azure Data Studio
2. Execute it against KioskDb database
3. Run the app - it should work now

## Understanding the Migration History

The `__EFMigrationsHistory` table tracks which migrations have been applied:

```sql
SELECT * FROM [__EFMigrationsHistory];
```

Should show:
```
MigrationId                      | ProductVersion
-------------------------------- | --------------
20251203092523_InitialCreate    | 9.0.0
```

If this table is **empty** or **missing**, EF Core thinks it needs to create all tables, even if they already exist from Dapper.

## Prevention

### Best Practices for First Run:

**Option 1: Start with EF Core** (Recommended)
```bash
# First run - use EF Core
dotnet run
# Choose: 1 (EF Core)
# Database is created with migrations

# Later runs - use either
dotnet run
# Choose: 1 (EF Core) or 2 (Dapper)
```

**Option 2: Start with Dapper**
```bash
# First, run the Dapper SQL script manually
sqlcmd -S "(localdb)\mssqllocaldb" -i "Infrastructure.Dapper\Scripts\InitializeDatabase.sql"

# Then run the app with EF Core
dotnet run
# Choose: 1 (EF Core)
# App will detect tables and set up migration history
```

## Verify Your Setup

Run this SQL to check your database state:

```sql
USE KioskDb;
GO

-- Check tables exist
SELECT TABLE_NAME 
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_TYPE = 'BASE TABLE'
ORDER BY TABLE_NAME;

-- Should see 8 tables plus __EFMigrationsHistory

-- Check migration history
SELECT * FROM [__EFMigrationsHistory];

-- Should see at least one entry
```

## Still Having Issues?

### Check Connection String
`Console\appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=KioskDb;Trusted_Connection=true;MultipleActiveResultSets=true"
  }
}
```

### Verify LocalDB is Running
```bash
sqllocaldb info
sqllocaldb start mssqllocaldb
```

### Get Migration ID
If you need to know your migration ID:
```bash
cd Infrastructure.EFCore
dotnet ef migrations list
```

### Check EF Core Logs
The application shows detailed EF Core logs. Look for:
- `Applying migration '20251203092523_InitialCreate'` - Trying to apply
- `Failed executing DbCommand` - Error occurred
- `There is already an object named 'Customers'` - Tables exist

## Why Both Providers Work Together

The implementations are **100% compatible**:

### Same Schema
- EF Core migration creates exact same tables as Dapper SQL script
- Column names, types, and constraints match perfectly
- Foreign keys are identical

### Same Connection String
Both use: `Server=(localdb)\mssqllocaldb;Database=KioskDb;...`

### Same Database Name
Both use: `KioskDb`

### Interchangeable
- Data added with EF Core can be read with Dapper
- Data added with Dapper can be read with EF Core
- No conversion or migration needed!

## Testing the Fix

Run these steps to verify:

1. **Start fresh**:
   ```bash
   sqlcmd -S "(localdb)\mssqllocaldb" -i "Infrastructure.Dapper\Scripts\ResetDatabase.sql"
   ```

2. **Create with Dapper**:
   ```bash
   sqlcmd -S "(localdb)\mssqllocaldb" -i "Infrastructure.Dapper\Scripts\InitializeDatabase.sql"
   ```

3. **Run with EF Core**:
   ```bash
   cd Console
   dotnet run
   # Choose: 1 (EF Core)
   ```

4. **Verify**:
   - Should see "Database schema already exists"
   - Should see "EF Core is using the existing database schema"
   - No errors!

5. **Add data**:
   - Add a customer
   - Exit the app

6. **Verify with Dapper**:
   ```bash
   dotnet run
   # Choose: 2 (Dapper)
   # List customers - you should see your data!
   ```

## Summary

? **The issue is fixed!** The app now:
- Detects existing databases
- Checks for all 8 tables
- Updates migration history automatically
- Shows clear, helpful messages
- Preserves your data
- Supports seamless provider switching

**You can now freely switch between EF Core and Dapper without any database conflicts!** ??

## Related Files

- `Console\Program.cs` - Database initialization logic
- `Console\Helpers\DatabaseHelper.cs` - Schema detection and compatibility
- `Infrastructure.Dapper\Scripts\InitializeDatabase.sql` - Idempotent creation script
- `Infrastructure.Dapper\Scripts\ResetDatabase.sql` - Database cleanup script
- `Infrastructure.EFCore\Migrations\*` - EF Core migrations
