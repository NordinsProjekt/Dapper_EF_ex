namespace BlazorServer;

/// <summary>
/// Data provider selection for the Blazor Server application.
/// Change this value to switch between EF Core and Dapper.
/// </summary>
public enum DataProvider
{
    /// <summary>
    /// Use Entity Framework Core for data access
    /// </summary>
    EFCore,
    
    /// <summary>
    /// Use Dapper for data access
    /// </summary>
    Dapper
}
