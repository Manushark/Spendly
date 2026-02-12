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
            if (!string.IsNullOrEmpty(token))
            {
                _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }

        public async Task<List<ExpenseDto>> GetAllAsync()
        {
            SetAuthHeader();
            return await _http.GetFromJsonAsync<List<ExpenseDto>>("api/expenses")
                   ?? new List<ExpenseDto>();
        }

        public async Task CreateAsync(ExpenseDto dto)
        {
            SetAuthHeader();
            await _http.PostAsJsonAsync("api/expenses", dto);
        }

        public async Task UpdateAsync(int id, ExpenseDto dto)
        {
            SetAuthHeader();
            await _http.PutAsJsonAsync($"api/expenses/{id}", dto);
        }

        public async Task DeleteAsync(int id)
        {
            SetAuthHeader();
            await _http.DeleteAsync($"api/expenses/{id}");
        }
    }
}
