using Application.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Domain.Entities;
using Infrastructure.EFCore;
using Infrastructure.EFCore.Repositories;
using Infrastructure.Dapper;
using Presentation.KioskViewer.Services;
using Presentation.KioskViewer.UI;
using Presentation.KioskViewer.Helpers;
using Microsoft.Data.SqlClient;

namespace Presentation.KioskViewer;

internal class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("═══════════════════════════════════════");
        Console.WriteLine("   KIOSK MANAGEMENT SYSTEM - STARTUP   ");
        Console.WriteLine("═══════════════════════════════════════\n");

        // Build configuration
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        // Ask user which data provider to use
        Console.WriteLine("Select Data Provider:");
        Console.WriteLine("1. Entity Framework Core");
        Console.WriteLine("2. Dapper");
        Console.Write("\nEnter choice (1 or 2): ");
        
        var providerChoice = Console.ReadLine();
        var dataProvider = providerChoice == "2" ? DataProvider.Dapper : DataProvider.EFCore;

        Console.WriteLine($"\nUsing {dataProvider} as data provider...\n");

        // Build host with dependency injection
        var host = Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                // Register configuration
                services.AddSingleton<IConfiguration>(configuration);

                // Register data access layer based on provider
                if (dataProvider == DataProvider.EFCore)
                {
                    // Register EF Core
                    services.AddDbContext<ApplicationDbContext>(options =>
                        options.UseSqlServer(connectionString));

                    services.AddScoped<IRepository<Customer>, Repository<Customer>>();
                    services.AddScoped<IRepository<Product>, Repository<Product>>();
                    services.AddScoped<IRepository<Employee>, Repository<Employee>>();
                }
                else
                {
                    // Register Dapper
                    services.AddDapperInfrastructure(connectionString);
                }

                // Register application services
                services.AddScoped<CustomerService>();
                services.AddScoped<ProductService>();
                services.AddScoped<EmployeeService>();

                // Register UI
                services.AddScoped(sp => new MainMenu(
                    sp.GetRequiredService<CustomerService>(),
                    sp.GetRequiredService<ProductService>(),
                    sp.GetRequiredService<EmployeeService>(),
                    dataProvider.ToString()
                ));
            })
            .Build();

        // Initialize database
        await InitializeDatabaseAsync(host.Services, dataProvider, connectionString);

        // Run the application
        using (var scope = host.Services.CreateScope())
        {
            var mainMenu = scope.ServiceProvider.GetRequiredService<MainMenu>();
            await mainMenu.ShowAsync();
        }
    }

    private static async Task InitializeDatabaseAsync(IServiceProvider services, DataProvider provider, string connectionString)
    {
        Console.WriteLine("Initializing database...");

        if (provider == DataProvider.EFCore)
        {
            using var scope = services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            
            try
            {
                // Check if database and schema already exist
                Console.WriteLine("Checking if database schema exists...");
                var (schemaExists, tableCount) = await DatabaseHelper.CheckSchemaExistsAsync(context);
                Console.WriteLine($"Schema status: {tableCount} out of 8 tables found.");
                
                if (schemaExists)
                {
                    // All 8 tables exist
                    Console.WriteLine("ℹ Database schema already exists.");
                    
                    // Ensure EF Core migrations history is set up
                    Console.WriteLine("Ensuring schema compatibility...");
                    await DatabaseHelper.EnsureSchemaCompatibilityAsync(context);
                    
                    // Verify no pending migrations remain
                    var stillPending = await context.Database.GetPendingMigrationsAsync();
                    if (stillPending.Any())
                    {
                        Console.WriteLine($"Warning: Still have {stillPending.Count()} pending migrations after compatibility check.");
                        Console.WriteLine("Attempting to apply them...");
                        await context.Database.MigrateAsync();
                    }
                    
                    Console.WriteLine("✓ EF Core is using the existing database schema.\n");
                }
                else if (tableCount > 0 && tableCount < 8)
                {
                    // Partial schema exists - this is a problem
                    Console.WriteLine($"⚠ WARNING: Partial database schema detected ({tableCount} out of 8 tables exist).");
                    Console.WriteLine("  This usually means a previous migration or setup was incomplete.");
                    Console.WriteLine("\n  Options:");
                    Console.WriteLine("  1. Drop the database and start fresh (recommended):");
                    Console.WriteLine("     sqlcmd -S \"(localdb)\\mssqllocaldb\" -i \"Infrastructure.Dapper\\Scripts\\ResetDatabase.sql\"");
                    Console.WriteLine("\n  2. Manually inspect and fix the database schema.");
                    Console.WriteLine("\n  The application will attempt to continue, but may encounter errors.\n");
                    
                    // Try to continue anyway - might work if the missing tables get created
                    try
                    {
                        await context.Database.MigrateAsync();
                        Console.WriteLine("✓ Migration completed successfully despite partial schema.\n");
                    }
                    catch (Exception migEx)
                    {
                        Console.WriteLine($"✗ Migration failed: {migEx.Message}");
                        Console.WriteLine("  Please reset the database and try again.\n");
                    }
                }
                else
                {
                    // No tables exist - fresh database
                    Console.WriteLine("Creating database schema...");
                    await context.Database.MigrateAsync();
                    Console.WriteLine("✓ EF Core database created and initialized successfully.\n");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠ Warning: Could not initialize EF Core database: {ex.Message}");
                Console.WriteLine("  The application will attempt to continue.\n");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"  Details: {ex.InnerException.Message}\n");
                }
            }
        }
        else
        {
            // Dapper: Check if database exists, and create it if not
            try
            {
                Console.WriteLine("Checking if database exists...");
                bool databaseExists = await CheckDatabaseExistsAsync(connectionString);
                
                if (!databaseExists)
                {
                    Console.WriteLine("✗ Database does not exist.");
                    Console.WriteLine("Creating database using Dapper initialization script...\n");
                    
                    await InitializeDapperDatabaseAsync(connectionString);
                    
                    Console.WriteLine("✓ Dapper database created and initialized successfully.\n");
                }
                else
                {
                    Console.WriteLine("✓ Database already exists.\n");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠ Warning: Could not initialize Dapper database: {ex.Message}");
                Console.WriteLine("  You may need to manually run the initialization script:");
                Console.WriteLine("  Infrastructure.Dapper\\Scripts\\InitializeDatabase.sql\n");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"  Details: {ex.InnerException.Message}\n");
                }
            }
        }

        Console.WriteLine("Press any key to continue...");
        Console.ReadKey();
    }

    private static async Task<bool> CheckDatabaseExistsAsync(string connectionString)
    {
        var builder = new SqlConnectionStringBuilder(connectionString);
        var databaseName = builder.InitialCatalog;
        builder.InitialCatalog = "master";
        
        using var connection = new SqlConnection(builder.ConnectionString);
        await connection.OpenAsync();
        
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM sys.databases WHERE name = @DatabaseName";
        command.Parameters.AddWithValue("@DatabaseName", databaseName);
        
        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result) > 0;
    }

    private static async Task InitializeDapperDatabaseAsync(string connectionString)
    {
        // Read the initialization script
        var scriptPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, 
            "..", "..", "..", "..", "Infrastructure.Dapper", "Scripts", "InitializeDatabase.sql");
        
        if (!File.Exists(scriptPath))
        {
            throw new FileNotFoundException(
                $"Initialization script not found at: {scriptPath}\n" +
                "Please ensure Infrastructure.Dapper\\Scripts\\InitializeDatabase.sql exists.");
        }
        
        var script = await File.ReadAllTextAsync(scriptPath);
        
        // Split script by GO statements
        var batches = script.Split(new[] { "\nGO", "\rGO", "\r\nGO" }, 
            StringSplitOptions.RemoveEmptyEntries);
        
        // Connect to master database to create the KioskDb database
        var builder = new SqlConnectionStringBuilder(connectionString);
        builder.InitialCatalog = "master";
        
        using var connection = new SqlConnection(builder.ConnectionString);
        await connection.OpenAsync();
        
        foreach (var batch in batches)
        {
            var trimmedBatch = batch.Trim();
            if (string.IsNullOrWhiteSpace(trimmedBatch) || trimmedBatch.StartsWith("--"))
                continue;
            
            try
            {
                using var command = connection.CreateCommand();
                command.CommandText = trimmedBatch;
                await command.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠ Warning executing batch: {ex.Message}");
                // Continue with other batches
            }
        }
    }
}
