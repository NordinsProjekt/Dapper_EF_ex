using System.Text;
using System.Text.Json;

namespace Infrastructure.ApiClient;

public class ApiClient : IApiClient
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly Dictionary<string, string> _defaultHeaders;
    private string _baseUri = string.Empty;
    private string _endpoint = string.Empty;

    public ApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _defaultHeaders = new Dictionary<string, string>();
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };
    }

    public void SetBaseUri(string baseUri)
    {
        _baseUri = baseUri.TrimEnd('/');
        UpdateHttpClientBaseAddress();
    }

    public void SetEndpoint(string endpoint)
    {
        _endpoint = endpoint.Trim('/');
        UpdateHttpClientBaseAddress();
    }

    public string GetCurrentUri()
    {
        return string.IsNullOrEmpty(_endpoint) ? _baseUri : $"{_baseUri}/{_endpoint}";
    }

    public void AddHeader(string name, string value)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException("Header name cannot be null or empty", nameof(name));

        _defaultHeaders[name] = value;
        
        // Update HttpClient default headers
        if (_httpClient.DefaultRequestHeaders.Contains(name))
        {
            _httpClient.DefaultRequestHeaders.Remove(name);
        }
        _httpClient.DefaultRequestHeaders.Add(name, value);
    }

    public void AddHeaders(Dictionary<string, string> headers)
    {
        if (headers == null)
            throw new ArgumentNullException(nameof(headers));

        foreach (var header in headers)
        {
            AddHeader(header.Key, header.Value);
        }
    }

    public void RemoveHeader(string name)
    {
        if (string.IsNullOrEmpty(name))
            return;

        _defaultHeaders.Remove(name);
        _httpClient.DefaultRequestHeaders.Remove(name);
    }

    public void ClearHeaders()
    {
        _defaultHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Clear();
    }

    public void SetAuthorizationHeader(string token, string scheme = "Bearer")
    {
        if (string.IsNullOrEmpty(token))
            throw new ArgumentException("Token cannot be null or empty", nameof(token));

        _httpClient.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue(scheme, token);
    }

    private void UpdateHttpClientBaseAddress()
    {
        var uri = GetCurrentUri();
        if (!string.IsNullOrEmpty(uri))
        {
            _httpClient.BaseAddress = new Uri(uri);
        }
    }

    public async Task<IEnumerable<T>> GetAllAsync<T>(string? path = null)
    {
        var url = BuildUrl(path);
        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();
        
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<IEnumerable<T>>(content, _jsonOptions) ?? Enumerable.Empty<T>();
    }

    public async Task<T?> GetByIdAsync<T>(object id, string? path = null)
    {
        var url = BuildUrl(path, id.ToString());
        var response = await _httpClient.GetAsync(url);
        
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return default(T);
            
        response.EnsureSuccessStatusCode();
        
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(content, _jsonOptions);
    }

    public async Task<T> CreateAsync<T>(T entity, string? path = null)
    {
        var url = BuildUrl(path);
        var json = JsonSerializer.Serialize(entity, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        var response = await _httpClient.PostAsync(url, content);
        response.EnsureSuccessStatusCode();
        
        var responseContent = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(responseContent, _jsonOptions)!;
    }

    public async Task<T> UpdateAsync<T>(object id, T entity, string? path = null)
    {
        var url = BuildUrl(path, id.ToString());
        var json = JsonSerializer.Serialize(entity, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        var response = await _httpClient.PutAsync(url, content);
        response.EnsureSuccessStatusCode();
        
        var responseContent = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(responseContent, _jsonOptions)!;
    }

    public async Task<bool> DeleteAsync(object id, string? path = null)
    {
        var url = BuildUrl(path, id.ToString());
        var response = await _httpClient.DeleteAsync(url);
        
        return response.IsSuccessStatusCode;
    }

    public async Task<TResponse> PostAsync<TRequest, TResponse>(TRequest request, string? path = null)
    {
        var url = BuildUrl(path);
        var json = JsonSerializer.Serialize(request, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        var response = await _httpClient.PostAsync(url, content);
        response.EnsureSuccessStatusCode();
        
        var responseContent = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<TResponse>(responseContent, _jsonOptions)!;
    }

    public async Task<TResponse> PutAsync<TRequest, TResponse>(TRequest request, string? path = null)
    {
        var url = BuildUrl(path);
        var json = JsonSerializer.Serialize(request, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        var response = await _httpClient.PutAsync(url, content);
        response.EnsureSuccessStatusCode();
        
        var responseContent = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<TResponse>(responseContent, _jsonOptions)!;
    }

    public async Task<TResponse> GetAsync<TResponse>(string path)
    {
        var url = BuildUrl(path);
        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();
        
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<TResponse>(content, _jsonOptions)!;
    }

    private string BuildUrl(string? path = null, string? id = null)
    {
        var segments = new List<string>();
        
        if (!string.IsNullOrEmpty(path))
            segments.Add(path.Trim('/'));
            
        if (!string.IsNullOrEmpty(id))
            segments.Add(id);
            
        return segments.Count > 0 ? string.Join("/", segments) : string.Empty;
    }
}