-- Database initialization script for Dapper implementation
-- This script is idempotent - safe to run multiple times
-- It will only create objects that don't already exist

USE master;
GO

-- Create database if it doesn't exist
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'KioskDb')
BEGIN
    CREATE DATABASE KioskDb;
    PRINT 'Database KioskDb created.';
END
ELSE
BEGIN
    PRINT 'Database KioskDb already exists.';
END
GO

USE KioskDb;
GO

-- Create Customers table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Customers')
BEGIN
    CREATE TABLE Customers (
        Id UNIQUEIDENTIFIER PRIMARY KEY,
        FirstName NVARCHAR(100) NOT NULL,
        LastName NVARCHAR(100) NOT NULL,
        Email NVARCHAR(255) NOT NULL,
        Phone NVARCHAR(20) NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NULL,
        CONSTRAINT UQ_Customers_Email UNIQUE (Email)
    );
    PRINT 'Table Customers created.';
END
ELSE
BEGIN
    PRINT 'Table Customers already exists.';
END
GO

-- Create Products table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Products')
BEGIN
    CREATE TABLE Products (
        Id UNIQUEIDENTIFIER PRIMARY KEY,
        Name NVARCHAR(200) NOT NULL,
        Description NVARCHAR(1000) NULL,
        Price DECIMAL(18, 2) NOT NULL,
        StockQuantity INT NOT NULL DEFAULT 0,
        SKU NVARCHAR(50) NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NULL,
        CONSTRAINT UQ_Products_SKU UNIQUE (SKU)
    );
    PRINT 'Table Products created.';
END
ELSE
BEGIN
    PRINT 'Table Products already exists.';
END
GO

-- Create PaymentMethods table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'PaymentMethods')
BEGIN
    CREATE TABLE PaymentMethods (
        Id UNIQUEIDENTIFIER PRIMARY KEY,
        Name NVARCHAR(100) NOT NULL,
        Description NVARCHAR(500) NULL,
        IsActive BIT NOT NULL DEFAULT 1
    );
    PRINT 'Table PaymentMethods created.';
END
ELSE
BEGIN
    PRINT 'Table PaymentMethods already exists.';
END
GO

-- Create Employees table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Employees')
BEGIN
    CREATE TABLE Employees (
        Id UNIQUEIDENTIFIER PRIMARY KEY,
        FirstName NVARCHAR(100) NOT NULL,
        LastName NVARCHAR(100) NOT NULL,
        Email NVARCHAR(255) NOT NULL,
        Phone NVARCHAR(20) NULL,
        HireDate DATETIME2 NOT NULL,
        HourlyRate DECIMAL(18, 2) NOT NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        CONSTRAINT UQ_Employees_Email UNIQUE (Email)
    );
    PRINT 'Table Employees created.';
END
ELSE
BEGIN
    PRINT 'Table Employees already exists.';
END
GO

-- Create Receipts table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Receipts')
BEGIN
    CREATE TABLE Receipts (
        Id UNIQUEIDENTIFIER PRIMARY KEY,
        CustomerId UNIQUEIDENTIFIER NOT NULL,
        PaymentMethodId UNIQUEIDENTIFIER NOT NULL,
        PurchaseDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        TotalAmount DECIMAL(18, 2) NOT NULL,
        TaxAmount DECIMAL(18, 2) NULL,
        Notes NVARCHAR(1000) NULL,
        CONSTRAINT FK_Receipts_Customers FOREIGN KEY (CustomerId) REFERENCES Customers(Id),
        CONSTRAINT FK_Receipts_PaymentMethods FOREIGN KEY (PaymentMethodId) REFERENCES PaymentMethods(Id)
    );
    PRINT 'Table Receipts created.';
END
ELSE
BEGIN
    PRINT 'Table Receipts already exists.';
END
GO

-- Create ReceiptItems table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ReceiptItems')
BEGIN
    CREATE TABLE ReceiptItems (
        Id UNIQUEIDENTIFIER PRIMARY KEY,
        ReceiptId UNIQUEIDENTIFIER NOT NULL,
        ProductId UNIQUEIDENTIFIER NOT NULL,
        Quantity INT NOT NULL,
        UnitPrice DECIMAL(18, 2) NOT NULL,
        TotalPrice DECIMAL(18, 2) NOT NULL,
        CONSTRAINT FK_ReceiptItems_Receipts FOREIGN KEY (ReceiptId) REFERENCES Receipts(Id) ON DELETE CASCADE,
        CONSTRAINT FK_ReceiptItems_Products FOREIGN KEY (ProductId) REFERENCES Products(Id)
    );
    PRINT 'Table ReceiptItems created.';
END
ELSE
BEGIN
    PRINT 'Table ReceiptItems already exists.';
END
GO

-- Create TimeEntries table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'TimeEntries')
BEGIN
    CREATE TABLE TimeEntries (
        Id UNIQUEIDENTIFIER PRIMARY KEY,
        EmployeeId UNIQUEIDENTIFIER NOT NULL,
        ClockIn DATETIME2 NOT NULL,
        ClockOut DATETIME2 NULL,
        HoursWorked DECIMAL(5, 2) NULL,
        Notes NVARCHAR(500) NULL,
        CONSTRAINT FK_TimeEntries_Employees FOREIGN KEY (EmployeeId) REFERENCES Employees(Id) ON DELETE CASCADE
    );
    PRINT 'Table TimeEntries created.';
END
ELSE
BEGIN
    PRINT 'Table TimeEntries already exists.';
END
GO

-- Create Paychecks table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Paychecks')
BEGIN
    CREATE TABLE Paychecks (
        Id UNIQUEIDENTIFIER PRIMARY KEY,
        EmployeeId UNIQUEIDENTIFIER NOT NULL,
        PayPeriodStart DATETIME2 NOT NULL,
        PayPeriodEnd DATETIME2 NOT NULL,
        PayDate DATETIME2 NOT NULL,
        GrossPay DECIMAL(18, 2) NOT NULL,
        NetPay DECIMAL(18, 2) NOT NULL,
        TaxDeduction DECIMAL(18, 2) NULL,
        OtherDeductions DECIMAL(18, 2) NULL,
        Notes NVARCHAR(1000) NULL,
        CONSTRAINT FK_Paychecks_Employees FOREIGN KEY (EmployeeId) REFERENCES Employees(Id) ON DELETE CASCADE
    );
    PRINT 'Table Paychecks created.';
END
ELSE
BEGIN
    PRINT 'Table Paychecks already exists.';
END
GO

-- Insert seed data for PaymentMethods (only if table is empty)
IF NOT EXISTS (SELECT * FROM PaymentMethods)
BEGIN
    INSERT INTO PaymentMethods (Id, Name, Description, IsActive) VALUES
        (NEWID(), 'Cash', 'Cash payment', 1),
        (NEWID(), 'Credit Card', 'Credit card payment', 1),
        (NEWID(), 'Debit Card', 'Debit card payment', 1);
    PRINT 'Seed data for PaymentMethods inserted.';
END
ELSE
BEGIN
    PRINT 'PaymentMethods table already has data.';
END
GO

PRINT '';
PRINT '========================================';
PRINT 'Database initialization completed successfully!';
PRINT 'Database: KioskDb';
PRINT 'Tables: 8 (Customers, Products, Employees, PaymentMethods, Receipts, ReceiptItems, TimeEntries, Paychecks)';
PRINT '========================================';
GO
