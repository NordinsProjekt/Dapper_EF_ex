using Microsoft.EntityFrameworkCore;
using Infrastructure.EFCore;

namespace Presentation.KioskViewer.Helpers;

public static class DatabaseHelper
{
    /// <summary>
    /// Ensures the database schema is compatible with EF Core, even if created by Dapper
    /// </summary>
    public static async Task EnsureSchemaCompatibilityAsync(ApplicationDbContext context)
    {
        try
        {
            // Get pending migrations
            var pendingMigrations = (await context.Database.GetPendingMigrationsAsync()).ToList();
            
            Console.WriteLine($"  Found {pendingMigrations.Count} pending migrations.");
            
            if (!pendingMigrations.Any())
            {
                Console.WriteLine("  No pending migrations to process.");
                return; // No pending migrations, we're good
            }

            // Check if __EFMigrationsHistory table exists
            bool tableExists = false;
            
            var connection = context.Database.GetDbConnection();
            var wasOpen = connection.State == System.Data.ConnectionState.Open;
            
            if (!wasOpen)
            {
                await context.Database.OpenConnectionAsync();
            }
            
            try
            {
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        SELECT COUNT(*) 
                        FROM INFORMATION_SCHEMA.TABLES 
                        WHERE TABLE_NAME = '__EFMigrationsHistory'";
                    
                    var result = await command.ExecuteScalarAsync();
                    tableExists = Convert.ToInt32(result) > 0;
                }
            }
            finally
            {
                if (!wasOpen)
                {
                    await context.Database.CloseConnectionAsync();
                }
            }

            if (!tableExists)
            {
                // Create migrations history table
                Console.WriteLine("  Creating __EFMigrationsHistory table...");
                await context.Database.ExecuteSqlRawAsync(@"
                    CREATE TABLE [__EFMigrationsHistory] (
                        [MigrationId] nvarchar(150) NOT NULL,
                        [ProductVersion] nvarchar(32) NOT NULL,
                        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
                    );
                ");
            }

            // Mark all pending migrations as applied since tables already exist
            Console.WriteLine($"  Marking {pendingMigrations.Count} migrations as applied...");
            foreach (var migration in pendingMigrations)
            {
                Console.WriteLine($"    - {migration}");
                await context.Database.ExecuteSqlRawAsync(
                    $"IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = '{migration}') " +
                    $"INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES ('{migration}', '9.0.0')");
            }
            
            Console.WriteLine("  Migration history updated successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  Warning: Could not ensure schema compatibility: {ex.Message}");
            throw; // Re-throw to let caller handle
        }
    }

    /// <summary>
    /// Checks if the database schema exists (has the expected tables)
    /// </summary>
    /// <returns>Tuple of (schemaExists: bool, tableCount: int)</returns>
    public static async Task<(bool schemaExists, int tableCount)> CheckSchemaExistsAsync(ApplicationDbContext context)
    {
        try
        {
            // First check if we can connect
            var canConnect = await context.Database.CanConnectAsync();
            if (!canConnect)
            {
                Console.WriteLine("  Cannot connect to database.");
                return (false, 0);
            }

            var connection = context.Database.GetDbConnection();
            var wasOpen = connection.State == System.Data.ConnectionState.Open;
            
            if (!wasOpen)
            {
                await context.Database.OpenConnectionAsync();
            }
            
            try
            {
                using var command = connection.CreateCommand();
                command.CommandText = @"
                    SELECT COUNT(*) 
                    FROM INFORMATION_SCHEMA.TABLES 
                    WHERE TABLE_NAME IN ('Customers', 'Products', 'Employees', 'Receipts', 
                                         'ReceiptItems', 'PaymentMethods', 'TimeEntries', 'Paychecks')";
                
                var result = await command.ExecuteScalarAsync();
                var tableCount = Convert.ToInt32(result);
                
                // All 8 tables should exist for schema to be considered complete
                bool schemaExists = tableCount == 8;
                
                return (schemaExists, tableCount);
            }
            finally
            {
                if (!wasOpen)
                {
                    await context.Database.CloseConnectionAsync();
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  Error checking schema: {ex.Message}");
            return (false, 0);
        }
    }
    
    /// <summary>
    /// Gets the list of existing tables from the 8 expected tables
    /// </summary>
    public static async Task<List<string>> GetExistingTablesAsync(ApplicationDbContext context)
    {
        try
        {
            var connection = context.Database.GetDbConnection();
            var wasOpen = connection.State == System.Data.ConnectionState.Open;
            
            if (!wasOpen)
            {
                await context.Database.OpenConnectionAsync();
            }
            
            try
            {
                using var command = connection.CreateCommand();
                command.CommandText = @"
                    SELECT TABLE_NAME 
                    FROM INFORMATION_SCHEMA.TABLES 
                    WHERE TABLE_NAME IN ('Customers', 'Products', 'Employees', 'Receipts', 
                                         'ReceiptItems', 'PaymentMethods', 'TimeEntries', 'Paychecks')
                    ORDER BY TABLE_NAME";
                
                var tables = new List<string>();
                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    tables.Add(reader.GetString(0));
                }
                
                return tables;
            }
            finally
            {
                if (!wasOpen)
                {
                    await context.Database.CloseConnectionAsync();
                }
            }
        }
        catch
        {
            return new List<string>();
        }
    }
}
