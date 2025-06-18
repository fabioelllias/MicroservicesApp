using Microsoft.Extensions.Options;
public class Auth0AuthService : IAuthService
{
    private readonly HttpClient _httpClient;
    private readonly Auth0Settings _settings;

    public Auth0AuthService(HttpClient httpClient, IOptions<Auth0Settings> settings)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
    }

    public async Task<AuthResponse> LoginAsync(string username, string password)
    {
        var url = $"{_settings.Domain.TrimEnd('/')}/dbconnections/login";

        var payload = new
        {
            client_id = _settings.ClientId,
            username = username,
            password = password,
            connection = _settings.Connection,
            scope = "openid profile email",
            grant_type = "password"
        };

        Console.WriteLine($"url: {url}");
        Console.WriteLine($"payload: {payload}");

        var response = await _httpClient.PostAsJsonAsync(url, payload);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"Authentication failed: {error}");
        }

        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
        return authResponse!;
    }
}
