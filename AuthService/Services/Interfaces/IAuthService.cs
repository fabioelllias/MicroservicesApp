public interface IAuthService
{
    Task<AuthResponse> LoginAsync(string username, string password);    
}
