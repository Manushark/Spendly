using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace Spendly.Web.Services
{
    public class CategoryApiClient
    {
        private readonly HttpClient _http;
        private readonly IHttpContextAccessor _ctx;

        public CategoryApiClient(HttpClient http, IHttpContextAccessor ctx)
        {
            _http = http;
            _ctx = ctx;
        }

        private void SetAuth()
        {
            var token = Helpers.TokenHelper.GetToken(_ctx.HttpContext);
            _http.DefaultRequestHeaders.Authorization = string.IsNullOrEmpty(token)
                ? null
                : new AuthenticationHeaderValue("Bearer", token);
        }

        public async Task<List<CategoryViewModel>> GetAllAsync()
        {
            SetAuth();
            var response = await _http.GetAsync("api/categories");
            if (!response.IsSuccessStatusCode) return [];
            return await response.Content.ReadFromJsonAsync<List<CategoryViewModel>>() ?? [];
        }

        private async Task<string?> ExtractErrorMessageAsync(HttpResponseMessage response)
        {
            var body = await response.Content.ReadAsStringAsync();
            try
            {
                var errorObj = JsonSerializer.Deserialize<JsonElement>(body);
                if (errorObj.TryGetProperty("error", out var errorProp))
                {
                    return errorProp.GetString();
                }
            }
            catch { }
            return body;
        }

        public async Task<(bool Success, string? Error)> CreateAsync(CreateCategoryRequest request)
        {
            SetAuth();
            var response = await _http.PostAsJsonAsync("api/categories", request);
            if (!response.IsSuccessStatusCode)
            {
                var error = await ExtractErrorMessageAsync(response);
                return (false, error);
            }
            return (true, null);
        }

        public async Task<(bool Success, string? Error)> UpdateAsync(int id, UpdateCategoryRequest request)
        {
            SetAuth();
            var response = await _http.PutAsJsonAsync($"api/categories/{id}", request);
            if (!response.IsSuccessStatusCode)
            {
                var error = await ExtractErrorMessageAsync(response);
                return (false, error);
            }
            return (true, null);
        }

        public async Task<(bool Success, string? Error)> DeleteAsync(int id)
        {
            SetAuth();
            var response = await _http.DeleteAsync($"api/categories/{id}");
            if (!response.IsSuccessStatusCode)
            {
                var error = await ExtractErrorMessageAsync(response);
                return (false, error);
            }
            return (true, null);
        }
    }

    public class CategoryViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public bool IsDefault { get; set; }
    }

    public class CreateCategoryRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
    }

    public class UpdateCategoryRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
    }
}
