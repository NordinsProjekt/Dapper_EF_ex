namespace Infrastructure.ApiClient;

public interface IApiClient
{
    // Configuration methods
    void SetBaseUri(string baseUri);
    void SetEndpoint(string endpoint);
    string GetCurrentUri();

    // Header management
    void AddHeader(string name, string value);
    void AddHeaders(Dictionary<string, string> headers);
    void RemoveHeader(string name);
    void ClearHeaders();
    void SetAuthorizationHeader(string token, string scheme = "Bearer");

    // Generic CRUD operations
    Task<IEnumerable<T>> GetAllAsync<T>(string? path = null);
    Task<T?> GetByIdAsync<T>(object id, string? path = null);
    Task<T> CreateAsync<T>(T entity, string? path = null);
    Task<T> UpdateAsync<T>(object id, T entity, string? path = null);
    Task<bool> DeleteAsync(object id, string? path = null);

    // Additional utility methods
    Task<TResponse> PostAsync<TRequest, TResponse>(TRequest request, string? path = null);
    Task<TResponse> PutAsync<TRequest, TResponse>(TRequest request, string? path = null);
    Task<TResponse> GetAsync<TResponse>(string path);
}