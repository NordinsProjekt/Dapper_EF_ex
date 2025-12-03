using BlazorServer;
using BlazorServer.Components;
using Domain.Entities;
using Infrastructure.EFCore;
using Infrastructure.EFCore.Repositories;
using Infrastructure.Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using Application.Services.Interfaces;
using Application.Services;

var builder = WebApplication.CreateBuilder(args);

// ============================================
// CONFIGURATION: Choose your data provider
// ============================================
const DataProvider DATA_PROVIDER = DataProvider.Dapper; // Change to DataProvider.Dapper to use Dapper
// ============================================

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

Console.WriteLine("========================================");
Console.WriteLine("  BLAZOR SERVER - KIOSK MANAGEMENT");
Console.WriteLine("========================================");
Console.WriteLine($"Data Provider: {DATA_PROVIDER}");
Console.WriteLine($"Environment: {builder.Environment.EnvironmentName}");
Console.WriteLine("========================================\n");

// Register data access layer based on provider
if (DATA_PROVIDER == DataProvider.EFCore)
{
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(connectionString));

    builder.Services.AddScoped<IRepository<Customer>, Repository<Customer>>();
    builder.Services.AddScoped<IRepository<Product>, Repository<Product>>();
    builder.Services.AddScoped<IRepository<Employee>, Repository<Employee>>();
}
else
{
    builder.Services.AddDapperInfrastructure(connectionString);
}

// Register APPLICATION SERVICES (business logic layer)
// These services contain the business logic and are shared between Console and Blazor
builder.Services.AddScoped<CustomerService>();
builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<EmployeeService>();

// Store the provider choice for UI display (using a wrapper class)
builder.Services.AddSingleton<IDataProviderService>(new DataProviderService(DATA_PROVIDER));

// Add services to the container
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

// Initialize database
await InitializeDatabaseAsync(app.Services, DATA_PROVIDER, connectionString);

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAntiforgery();
app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

Console.WriteLine("\nApplication started successfully!");
Console.WriteLine("Navigate to the application in your browser.\n");

app.Run();

// Database initialization logic
static async Task InitializeDatabaseAsync(IServiceProvider services, DataProvider provider, string connectionString)
{
    Console.WriteLine("Initializing database...");

    if (provider == DataProvider.EFCore)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        try
        {
            var canConnect = await context.Database.CanConnectAsync();
            
            if (!canConnect)
            {
                Console.WriteLine("Creating database...");
                await context.Database.MigrateAsync();
                Console.WriteLine("Database created successfully with EF Core.\n");
            }
            else
            {
                var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
                if (pendingMigrations.Any())
                {
                    Console.WriteLine($"Applying {pendingMigrations.Count()} pending migrations...");
                    await context.Database.MigrateAsync();
                    Console.WriteLine("Migrations applied successfully.\n");
                }
                else
                {
                    Console.WriteLine("Database is up to date.\n");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Could not initialize EF Core database: {ex.Message}\n");
        }
    }
    else
    {
        try
        {
            bool databaseExists = await CheckDatabaseExistsAsync(connectionString);

            if (!databaseExists)
            {
                Console.WriteLine("Database does not exist. Creating database with Dapper...");
                await InitializeDapperDatabaseAsync(connectionString);
                Console.WriteLine("Database created successfully with Dapper.\n");
            }
            else
            {
                Console.WriteLine("Database already exists.\n");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Could not initialize Dapper database: {ex.Message}");
            Console.WriteLine("You may need to manually run: Infrastructure.Dapper\\Scripts\\InitializeDatabase.sql\n");
        }
    }
}

static async Task<bool> CheckDatabaseExistsAsync(string connectionString)
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

static async Task InitializeDapperDatabaseAsync(string connectionString)
{
    var scriptPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
        "..", "..", "..", "..", "Infrastructure.Dapper", "Scripts", "InitializeDatabase.sql");

    if (!File.Exists(scriptPath))
    {
        throw new FileNotFoundException($"Initialization script not found at: {scriptPath}");
    }

    var script = await File.ReadAllTextAsync(scriptPath);
    var batches = script.Split(new[] { "\nGO", "\rGO", "\r\nGO" },
        StringSplitOptions.RemoveEmptyEntries);

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
            Console.WriteLine($"Warning executing batch: {ex.Message}");
        }
    }
}

// Service interface and implementation for DataProvider
public interface IDataProviderService
{
    DataProvider Provider { get; }
}

public class DataProviderService : IDataProviderService
{
    public DataProvider Provider { get; }
    
    public DataProviderService(DataProvider provider)
    {
        Provider = provider;
    }
}
