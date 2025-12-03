namespace ApiClient;

public sealed class ApiClientOptions
{
    public required string BaseUrl { get; set; }
    public Dictionary<string,string> DefaultHeaders { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}
