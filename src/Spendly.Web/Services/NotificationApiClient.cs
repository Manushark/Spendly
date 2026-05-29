using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Spendly.Web.Services
{
    public class NotificationApiClient
    {
        private readonly HttpClient _http;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public NotificationApiClient(HttpClient http, IHttpContextAccessor httpContextAccessor)
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

        public async Task<List<NotificationDto>> GetAllAsync(int page = 1, int pageSize = 20)
        {
            SetAuthHeader();
            try
            {
                var response = await _http.GetAsync($"api/notifications?page={page}&pageSize={pageSize}");
                if (!response.IsSuccessStatusCode) return [];
                return await response.Content.ReadFromJsonAsync<List<NotificationDto>>() ?? [];
            }
            catch { return []; }
        }

        public async Task<int> GetUnreadCountAsync()
        {
            SetAuthHeader();
            try
            {
                var response = await _http.GetAsync("api/notifications/unread-count");
                if (!response.IsSuccessStatusCode) return 0;
                var result = await response.Content.ReadFromJsonAsync<UnreadCountResponse>();
                return result?.Count ?? 0;
            }
            catch { return 0; }
        }

        public async Task MarkAsReadAsync(int id)
        {
            SetAuthHeader();
            await _http.PutAsync($"api/notifications/{id}/read", null);
        }

        public async Task MarkAllAsReadAsync()
        {
            SetAuthHeader();
            await _http.PutAsync("api/notifications/read-all", null);
        }

        public async Task DeleteAsync(int id)
        {
            SetAuthHeader();
            await _http.DeleteAsync($"api/notifications/{id}");
        }

        public async Task DeleteAllAsync()
        {
            SetAuthHeader();
            await _http.DeleteAsync("api/notifications/clear-all");
        }
    }

    public class NotificationDto
    {
        public int Id { get; set; }
        public string Message { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
        public int? RelatedEntityId { get; set; }
    }

    public class UnreadCountResponse
    {
        public int Count { get; set; }
    }
}
