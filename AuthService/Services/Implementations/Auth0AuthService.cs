using System.Text.Json;
using System.Net.Http.Headers;
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
        var url = $"https://{_settings.Domain}/oauth/token";

        var request = new Dictionary<string, string>
        {
            { "grant_type", "password" },
            { "username", username },
            { "password", password },
            { "audience", _settings.Audience },
            { "client_id", _settings.ClientId },
            { "client_secret", _settings.ClientSecret }
        };

        var response = await _httpClient.PostAsync(url, new FormUrlEncodedContent(request));

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"Authentication failed: {error}");
        }

        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<AuthResponse>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return result ?? throw new Exception("Failed to parse Auth0 response.");
    }

    public async Task<AuthResponse> RefreshTokenAsync(string refreshToken)
    {
        var url = $"https://{_settings.Domain}/oauth/token";

        var request = new Dictionary<string, string>
        {
            { "grant_type", "refresh_token" },
            { "refresh_token", refreshToken },
            { "client_id", _settings.ClientId },
            { "client_secret", _settings.ClientSecret }
        };

        var response = await _httpClient.PostAsync(url, new FormUrlEncodedContent(request));

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"Refresh token failed: {error}");
        }

        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<AuthResponse>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return result ?? throw new Exception("Failed to parse Auth0 response.");
    }
}
