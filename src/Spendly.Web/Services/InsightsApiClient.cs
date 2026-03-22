using Spendly.Application.Services;
using System.Net.Http.Headers;

public class InsightsApiClient
{
    private readonly HttpClient _http;
    private readonly IHttpContextAccessor _context;

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

        return await response.Content.ReadFromJsonAsync<InsightsSummaryDto>();
    }
}