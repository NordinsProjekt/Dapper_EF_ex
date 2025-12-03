-- Complete Database Reset and Verification Script
-- This script will drop and recreate the database with the correct schema

USE master;
GO

PRINT 'Step 1: Dropping existing database...';

-- Close any existing connections to KioskDb
IF EXISTS (SELECT name FROM sys.databases WHERE name = 'KioskDb')
BEGIN
    ALTER DATABASE KioskDb SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE KioskDb;
    PRINT '  ✓ Database KioskDb dropped successfully.';
END
ELSE
BEGIN
    PRINT '  ℹ Database KioskDb does not exist.';
END
GO

PRINT '';
PRINT 'Step 2: Creating fresh database...';

-- Create fresh database
CREATE DATABASE KioskDb;
GO

USE KioskDb;
GO

PRINT '  ✓ Database KioskDb created.';
PRINT '';
PRINT '========================================';
PRINT 'Database reset completed!';
PRINT '';
PRINT 'Next steps:';
PRINT '1. Run your console application';
PRINT '2. Choose option 1 (Entity Framework Core)';
PRINT '3. EF Core will create all tables with correct schema';
PRINT '========================================';
GO