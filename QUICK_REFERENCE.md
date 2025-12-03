# Quick Reference: Database Issues

## ?? Error: "Tables Already Exist"

```
Failed executing DbCommand...
There is already an object named 'Customers' in the database.
```

### ? Quick Fix (1 Step)
```bash
# Just run the app again - it will auto-fix!
cd Console
dotnet run
# Choose: 1 (EF Core)
```

---

## ?? Common Scenarios

### Scenario 1: First Time Setup
```bash
cd Console
dotnet run
# Choose: 1 (EF Core) - Creates DB automatically
```
? **Database created with migrations**

---

### Scenario 2: Already Have Dapper Database
```bash
cd Console
dotnet run
# Choose: 1 (EF Core)
```
? **App detects tables and adapts automatically**

---

### Scenario 3: Switching Between Providers
```bash
# Run 1
dotnet run ? Choose: 1 (EF Core) ? Add data

# Run 2
dotnet run ? Choose: 2 (Dapper) ? See same data!

# Run 3
dotnet run ? Choose: 1 (EF Core) ? Data still there!
```
? **Both providers share the same database**

---

### Scenario 4: Want Fresh Start
```bash
# Drop database
sqlcmd -S "(localdb)\mssqllocaldb" -i "Infrastructure.Dapper\Scripts\ResetDatabase.sql"

# Recreate
cd Console
dotnet run
# Choose: 1 (EF Core)
```
? **Clean slate created**

---

## ?? Database State Check

```sql
USE KioskDb;

-- Check tables (should see 9 total)
SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES;

-- Check migration history (should have entries)
SELECT * FROM [__EFMigrationsHistory];

-- Check data
SELECT COUNT(*) FROM Customers;
SELECT COUNT(*) FROM Products;
SELECT COUNT(*) FROM Employees;
```

---

## ?? Manual Fixes

### Fix Missing Migration History
```sql
USE KioskDb;

-- Create history table if missing
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '__EFMigrationsHistory')
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END

-- Mark migration as applied
INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion]) 
VALUES ('20251203092523_InitialCreate', '9.0.0');
```

---

### Fix LocalDB Not Running
```bash
sqllocaldb info
sqllocaldb start mssqllocaldb
```

---

## ?? Decision Tree

```
Do you have a database?
?? NO ? Run with EF Core (option 1)
?        ? Creates database automatically
?
?? YES ? What created it?
    ?? Dapper ? Run with EF Core (option 1)
    ?           ? Auto-detects and adapts
    ?
    ?? EF Core ? Use either provider
    ?            ? Works seamlessly
    ?
    ?? Not sure ? Run with EF Core (option 1)
                  ? Handles any scenario
```

---

## ?? What Messages Mean

| Message | Meaning | Action |
|---------|---------|--------|
| "Database created successfully" | Fresh DB created | None - all good ? |
| "Database schema already exists" | Tables found | None - will adapt ? |
| "EF Core is using existing schema" | Adapted to Dapper DB | None - working! ? |
| "Database is up to date" | EF migrations current | None - all good ? |
| "Could not initialize..." | Real error | Check connection string ?? |

---

## ?? Best Practices

### ? DO:
- Start with EF Core for automatic setup
- Switch freely between providers
- Let the app handle schema detection
- Keep your data - both providers share it

### ? DON'T:
- Manually create tables and migrations simultaneously
- Worry about "schema already exists" messages (it's info, not error)
- Delete data when switching providers

---

## ?? Emergency Commands

### Drop Everything
```sql
USE master;
ALTER DATABASE KioskDb SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
DROP DATABASE KioskDb;
```

### Check Connection
```bash
sqlcmd -S "(localdb)\mssqllocaldb" -Q "SELECT @@VERSION"
```

### List Databases
```bash
sqlcmd -S "(localdb)\mssqllocaldb" -Q "SELECT name FROM sys.databases"
```

---

## ?? Remember

1. **Both providers = Same database** ??
2. **EF Core auto-detects** existing tables ??
3. **Safe to run multiple times** ??
4. **Your data is preserved** ??
5. **Switch anytime** ?

---

## ?? More Help

- **Detailed Guide**: [TROUBLESHOOTING_DB.md](TROUBLESHOOTING_DB.md)
- **Quick Start**: [QUICK_START.md](QUICK_START.md)
- **Full Docs**: [README.md](README.md)

---

**Last Updated**: 2024-12-03  
**Status**: ? All issues resolved - app auto-fixes database conflicts!
