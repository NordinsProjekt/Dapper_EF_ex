-- Database Reset Script for KioskDb
-- Run this script to completely reset the database to a clean state
-- WARNING: This will delete ALL data!

USE master;
GO

-- Close any existing connections to KioskDb
IF EXISTS (SELECT name FROM sys.databases WHERE name = 'KioskDb')
BEGIN
    ALTER DATABASE KioskDb SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE KioskDb;
    PRINT 'Database KioskDb dropped successfully.';
END
ELSE
BEGIN
    PRINT 'Database KioskDb does not exist.';
END
GO

PRINT '';
PRINT '========================================';
PRINT 'Database reset completed!';
PRINT 'You can now run the application with either provider to create a fresh database.';
PRINT '========================================';
GO
