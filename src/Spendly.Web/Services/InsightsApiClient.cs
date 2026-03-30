using Spendly.Application.Services;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

public class InsightsApiClient
{
    private readonly HttpClient _http;
    private readonly IHttpContextAccessor _context;
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public InsightsApiClient(HttpClient http, IHttpContextAccessor context)
    {
        _http = http;
        _context = context;
    }

    public async Task<InsightsSummaryDto> GetInsightsAsync()
    {
        var token = _context.HttpContext?.Session.GetString("token");
        _http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var response = await _http.GetAsync("api/insights");
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<InsightsSummaryDto>(_jsonOptions);
    }
}