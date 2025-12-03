# Implementation Plan

## Tasks Overview

### 1. Domain Layer - Create Entity Classes ?
- [x] Customer entity
- [x] Product entity
- [x] Receipt entity
- [x] ReceiptItem entity (additional entity for receipt details)
- [x] PaymentMethod entity
- [x] Paycheck entity
- [x] Employee entity (created to support Paycheck and TimeEntry)
- [x] TimeEntry entity

### 2. Infrastructure.Dapper - Dapper Implementation ?
- [x] Create repository interfaces (using IRepository from Application.Services)
- [x] Implement Dapper repositories for Customer, Product, and Employee entities
- [x] Create SQL initialization script (InitializeDatabase.sql)
- [x] Add ServiceCollectionExtensions for DI

### 3. Infrastructure.EFCore - Entity Framework Implementation ?
- [x] Create DbContext with all entities
- [x] Configure entity relationships
- [x] Create EF Core migration (InitialCreate)
- [x] Add DbContextFactory for design-time operations

### 4. Application.Services - Service Layer ?
- [x] IRepository interface exists in Application.Services
- [x] Repositories use the common interface

### 5. Presentation.KioskViewer (Console) - Console Application ?
- [x] Create configuration for implementation selection (DataProvider enum)
- [x] Create comprehensive menu system with MainMenu class
- [x] Implement CRUD operations for Customers, Products, and Employees
- [x] Add dependency injection container
- [x] Support both EF Core and Dapper implementations with runtime selection
- [x] Add appsettings.json for configuration

## Implementation Complete! ?

### Database Initialization Instructions

#### For EF Core:
1. The application will automatically run migrations on startup
2. Alternatively, run manually: `dotnet ef database update --project Infrastructure.EFCore`

#### For Dapper:
1. Run the SQL script located at: `Infrastructure.Dapper\Scripts\InitializeDatabase.sql`
2. This will create the database and all necessary tables

### Running the Application

```bash
cd Console
dotnet run
```

The application will prompt you to choose between:
1. Entity Framework Core
2. Dapper

Both implementations use the same database schema and are fully interchangeable!

## Features Implemented

- ? Multi-layered architecture (Domain, Infrastructure, Application, Presentation)
- ? Repository pattern with generic interface
- ? Dependency injection throughout
- ? Runtime data provider selection
- ? Comprehensive console menu system
- ? CRUD operations for all main entities:
  - Customers (with email validation)
  - Products (with stock management)
  - Employees (with hourly rate tracking)
- ? Both EF Core and Dapper support the same operations
- ? Proper entity relationships and foreign keys
- ? Database initialization for both providers
