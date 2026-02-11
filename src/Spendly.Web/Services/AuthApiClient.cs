using System.Net.Http.Json;
using Spendly.Web.Contracts.Auth;

namespace Spendly.Web.Services
{
    public class AuthApiClient
    {
        private readonly HttpClient _http;

        public AuthApiClient(HttpClient http)
        {
            _http = http;
        }

        public async Task<AuthResponse?> LoginAsync(LoginViewModel model)
        {
            var response = await _http.PostAsJsonAsync("api/auth/login", model);

            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<AuthResponse>();
        }

        public async Task<AuthResponse?> RegisterAsync(RegisterViewModel model)
        {
            var response = await _http.PostAsJsonAsync("api/auth/register", model);

            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<AuthResponse>();
        }
    }
}
