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
        public async Task<(bool Success, string? ErrorMessage)> ForgotPasswordAsync(string email)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/auth/forgot-password", new { email });
                return response.IsSuccessStatusCode
                    ? (true, null)
                    : (false, "Something went wrong. Please try again.");
            }
            catch (Exception)
            {
                return (false, "Could not connect to the server. Please try again in a moment.");
            }
        }

        public async Task<(bool Success, string? ErrorMessage)> ResetPasswordAsync(string token, string newPassword, string confirmPassword)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/auth/reset-password", new
                {
                    token,
                    newPassword,
                    confirmPassword
                });

                if (response.IsSuccessStatusCode) return (true, null);

                var body = await response.Content.ReadAsStringAsync();
                return (false, body.Contains("expired") ? "This link has expired. Please request a new one."
                    : "Invalid or already used link.");
            }
            catch (Exception)
            {
                return (false, "Could not connect to the server. Please try again in a moment.");
            }
        }
    }
}

