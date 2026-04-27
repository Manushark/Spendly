using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Spendly.Web.Services
{
    public class TagApiClient
    {
        private readonly HttpClient _http;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TagApiClient(HttpClient http, IHttpContextAccessor httpContextAccessor)
        {
            _http = http;
            _httpContextAccessor = httpContextAccessor;
        }

        private void SetAuthHeader()
        {
            var token = Helpers.TokenHelper.GetToken(_httpContextAccessor.HttpContext);
            _http.DefaultRequestHeaders.Authorization = string.IsNullOrEmpty(token)
                ? null
                : new AuthenticationHeaderValue("Bearer", token);
        }

        public async Task<List<TagDto>> GetAllAsync()
        {
            SetAuthHeader();
            try
            {
                var response = await _http.GetAsync("api/tags");
                if (!response.IsSuccessStatusCode) return [];
                return await response.Content.ReadFromJsonAsync<List<TagDto>>() ?? [];
            }
            catch { return []; }
        }

        public async Task<bool> CreateAsync(string name, string color)
        {
            SetAuthHeader();
            var response = await _http.PostAsJsonAsync("api/tags", new { name, color });
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateAsync(int id, string name, string color)
        {
            SetAuthHeader();
            var response = await _http.PutAsJsonAsync($"api/tags/{id}", new { name, color });
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            SetAuthHeader();
            var response = await _http.DeleteAsync($"api/tags/{id}");
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> SetExpenseTagsAsync(int expenseId, List<int> tagIds)
        {
            SetAuthHeader();
            var response = await _http.PutAsJsonAsync($"api/tags/expense/{expenseId}", new { tagIds });
            return response.IsSuccessStatusCode;
        }
    }

    public class TagDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public int ExpenseCount { get; set; }
    }
}
