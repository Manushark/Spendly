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

        public async Task<(AuthResponse? Result, string? ErrorMessage)> LoginAsync(LoginViewModel model)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/auth/login", model);

                if (response.IsSuccessStatusCode)
                    return (await response.Content.ReadFromJsonAsync<AuthResponse>(), null);

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    return (null, "Invalid email or password.");

                // Server error (500, 503, etc.) — likely DB timeout or app recycling
                return (null, "The server is temporarily unavailable. Please try again in a few seconds.");
            }
            catch (Exception)
            {
                return (null, "Could not connect to the server. Please try again in a moment.");
            }
        }

        public async Task<(AuthResponse? Result, string? ErrorMessage)> RegisterAsync(RegisterViewModel model)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/auth/register", model);

                if (response.IsSuccessStatusCode)
                    return (await response.Content.ReadFromJsonAsync<AuthResponse>(), null);

                if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                    return (null, "Registration failed. Email may already be in use.");

                return (null, "The server is temporarily unavailable. Please try again in a few seconds.");
            }
            catch (Exception)
            {
                return (null, "Could not connect to the server. Please try again in a moment.");
            }
        }
    }
}

