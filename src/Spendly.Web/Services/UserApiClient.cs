using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Spendly.Web.Services
{
    public class UserApiClient
    {
        private readonly HttpClient _http;
        private readonly IHttpContextAccessor _ctx;

        public UserApiClient(HttpClient http, IHttpContextAccessor ctx)
        {
            _http = http;
            _ctx = ctx;
        }

        private void SetAuth()
        {
            var token = _ctx.HttpContext?.Session.GetString("token");
            _http.DefaultRequestHeaders.Authorization = string.IsNullOrEmpty(token)
                ? null
                : new AuthenticationHeaderValue("Bearer", token);
        }

        public async Task<UserProfileResponse?> GetProfileAsync()
        {
            SetAuth();
            var response = await _http.GetAsync("api/user/profile");
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<UserProfileResponse>();
        }

        public async Task<(bool Success, string? Error)> UpdateProfileAsync(UpdateProfileRequest request)
        {
            SetAuth();
            var response = await _http.PutAsJsonAsync("api/user/profile", request);
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                return (false, body);
            }
            return (true, null);
        }

        public async Task<(bool Success, string? Error)> ChangePasswordAsync(ChangePasswordRequest request)
        {
            SetAuth();
            var response = await _http.PutAsJsonAsync("api/user/change-password", request);
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                return (false, body);
            }
            return (true, null);
        }
    }

    public class UserProfileResponse
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public string PreferredCurrency { get; set; } = "USD";
        public string TimeZone { get; set; } = "UTC";
        public DateTime CreatedAt { get; set; }
    }

    public class UpdateProfileRequest
    {
        public string? FullName { get; set; }
        public string PreferredCurrency { get; set; } = "USD";
        public string TimeZone { get; set; } = "UTC";
    }

    public class ChangePasswordRequest
    {
        public string CurrentPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
        public string ConfirmNewPassword { get; set; } = string.Empty;
    }
}
