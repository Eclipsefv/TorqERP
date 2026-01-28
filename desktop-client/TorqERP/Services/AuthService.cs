using System.Net.Http.Json;
using TorqERP.DataModels;
using TorqERP.Services;

public class AuthService
{
    private readonly SimpleAuthStateProvider _authStateProvider;
    private readonly HttpClient _httpClient;

    public AuthService(SimpleAuthStateProvider authStateProvider, HttpClient httpClient)
    {
        _authStateProvider = authStateProvider;
        _httpClient = httpClient;
    }


    public async Task<bool> Login(string email, string password)
    {
        try
        {
            var loginData = new { email, password };
            var response = await _httpClient.PostAsJsonAsync("api/auth/login", loginData);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<LoginResponse>();

                if (result != null && !string.IsNullOrEmpty(result.Token) && result.User != null)
                {
                    string roleToSave = result.User.Role;
                    string emailToSave = result.User.Email;

                    await SecureStorage.Default.SetAsync("user_token", result.Token);
                    await SecureStorage.Default.SetAsync("user_role", roleToSave);
                    await SecureStorage.Default.SetAsync("user_email", emailToSave);

                    _authStateProvider.NotifyLogin(emailToSave, roleToSave);
                    return true;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("USER STRUCTURE NOT FOUND IN OBJECT");
                }
            }
            return false;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ERROR: {ex.Message}");
            return false;
        }
    }

    public async Task Logout()
    {
        SecureStorage.Default.Remove("user_session");
        _authStateProvider.NotifyLogout();
    }
}