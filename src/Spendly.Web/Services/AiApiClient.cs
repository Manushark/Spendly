using System.Net.Http.Headers;
using System.Net.Http.Json;
using Spendly.Application.DTOs.Ai;

namespace Spendly.Web.Services
{
    public class AiApiClient
    {
        private readonly HttpClient _http;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<AiApiClient> _logger;

        public AiApiClient(
            HttpClient http,
            IHttpContextAccessor httpContextAccessor,
            ILogger<AiApiClient> logger)
        {
            _http = http;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        private void SetAuthHeader()
        {
            var token = Helpers.TokenHelper.GetToken(_httpContextAccessor.HttpContext);
            _http.DefaultRequestHeaders.Authorization = string.IsNullOrEmpty(token)
                ? null
                : new AuthenticationHeaderValue("Bearer", token);
        }

        public async Task<AiFinancialPlanDto?> ParseCommandAsync(string prompt, CancellationToken cancellationToken = default)
        {
            try
            {
                SetAuthHeader();
                var response = await _http.PostAsJsonAsync("api/ai/parse", new AiCommandRequestDto { Prompt = prompt }, cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("AiApiClient: Parse request returned status {StatusCode}", response.StatusCode);
                    return null;
                }

                return await response.Content.ReadFromJsonAsync<AiFinancialPlanDto>(cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AiApiClient: Error calling api/ai/parse");
                return null;
            }
        }

        public async Task<AiExecutionResultDto?> ExecutePlanAsync(AiFinancialPlanDto plan, CancellationToken cancellationToken = default)
        {
            try
            {
                SetAuthHeader();
                var response = await _http.PostAsJsonAsync("api/ai/execute", plan, cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("AiApiClient: Execute request returned status {StatusCode}", response.StatusCode);
                    return new AiExecutionResultDto
                    {
                        Success = false,
                        Message = $"Server error: {response.StatusCode}"
                    };
                }

                return await response.Content.ReadFromJsonAsync<AiExecutionResultDto>(cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AiApiClient: Error calling api/ai/execute");
                return new AiExecutionResultDto
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }
    }
}
