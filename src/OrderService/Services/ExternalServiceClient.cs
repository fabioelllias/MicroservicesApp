// Services/ExternalServiceClient.cs
public class ExternalServiceClient : IExternalServiceClient
{
    private readonly HttpClient _httpClient;
    
    private readonly ILogger<ExternalServiceClient> _logger;

    public ExternalServiceClient(IHttpClientFactory httpClientFactory, ILogger<ExternalServiceClient> logger)
    {
        _httpClient = httpClientFactory.CreateClient("ExternalServiceClient");
        _logger = logger;
    }

    public async Task<string> GetDataAsync()
    {
        var response = await _httpClient.GetAsync("/api/fakeserver/fail"); // ou qualquer endpoint que dê erro
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }
}
