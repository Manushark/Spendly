using System.Net.Http.Headers;
using System.Net.Http.Json;
using Spendly.Web.Contracts.SavingsGoals;

namespace Spendly.Web.Services
{
    public class SavingsGoalApiClient
    {
        private readonly HttpClient _http;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SavingsGoalApiClient(HttpClient http, IHttpContextAccessor httpContextAccessor)
        {
            _http = http;
            _httpContextAccessor = httpContextAccessor;
        }

        private void SetAuthHeader()
        {
            var token = _httpContextAccessor.HttpContext?.Session.GetString("token");
            _http.DefaultRequestHeaders.Authorization = string.IsNullOrEmpty(token)
                ? null
                : new AuthenticationHeaderValue("Bearer", token);
        }

        public async Task<List<SavingsGoalResponseDto>> GetAllAsync()
        {
            SetAuthHeader();
            try
            {
                var response = await _http.GetAsync("api/savings-goals");
                if (!response.IsSuccessStatusCode) return [];
                return await response.Content.ReadFromJsonAsync<List<SavingsGoalResponseDto>>() ?? [];
            }
            catch { return []; }
        }

        public async Task<SavingsGoalResponseDto?> GetByIdAsync(int id)
        {
            SetAuthHeader();
            try
            {
                var response = await _http.GetAsync($"api/savings-goals/{id}");
                if (!response.IsSuccessStatusCode) return null;
                return await response.Content.ReadFromJsonAsync<SavingsGoalResponseDto>();
            }
            catch { return null; }
        }

        public async Task<bool> CreateAsync(CreateSavingsGoalRequest request)
        {
            SetAuthHeader();
            var response = await _http.PostAsJsonAsync("api/savings-goals", request);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateAsync(int id, UpdateSavingsGoalRequest request)
        {
            SetAuthHeader();
            var response = await _http.PutAsJsonAsync($"api/savings-goals/{id}", request);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> AddFundsAsync(int id, decimal amount)
        {
            SetAuthHeader();
            var response = await _http.PostAsJsonAsync($"api/savings-goals/{id}/add-funds", new { amount });
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            SetAuthHeader();
            var response = await _http.DeleteAsync($"api/savings-goals/{id}");
            return response.IsSuccessStatusCode;
        }
    }
}
