using System.Net.Http.Headers;
using System.Net.Http.Json;
using Spendly.Web.Contracts.Expenses;

namespace Spendly.Web.Services
{
    public class ExpenseApiClient
    {
        private readonly HttpClient _http;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ExpenseApiClient(HttpClient http, IHttpContextAccessor httpContextAccessor)
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

        public async Task<List<ExpenseDto>> GetAllAsync()
        {
            SetAuthHeader();
            return await _http.GetFromJsonAsync<List<ExpenseDto>>("api/expenses")
                   ?? new List<ExpenseDto>();
        }

        public async Task<ExpenseDto?> GetByIdAsync(int id)
        {
            SetAuthHeader();
            var response = await _http.GetAsync($"api/expenses/{id}");

            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<ExpenseDto>();
        }

        public async Task CreateAsync(ExpenseDto dto)
        {
            SetAuthHeader();
            var response = await _http.PostAsJsonAsync("api/expenses", dto);
            response.EnsureSuccessStatusCode();
        }

        public async Task UpdateAsync(int id, ExpenseDto dto)
        {
            SetAuthHeader();
            var response = await _http.PutAsJsonAsync($"api/expenses/{id}", dto);
            response.EnsureSuccessStatusCode();
        }

        public async Task DeleteAsync(int id)
        {
            SetAuthHeader();
            var response = await _http.DeleteAsync($"api/expenses/{id}");
            response.EnsureSuccessStatusCode();
        }
    }
}
