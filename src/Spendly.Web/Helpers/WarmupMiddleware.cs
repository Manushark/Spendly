using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Spendly.Web.Helpers
{
    public class WarmupMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<WarmupMiddleware> _logger;
        private readonly string _apiBaseUrl;
        private static int _warmupTriggered = 0;

        public WarmupMiddleware(
            RequestDelegate next,
            IHttpClientFactory httpClientFactory,
            ILogger<WarmupMiddleware> logger,
            IConfiguration configuration)
        {
            _next = next;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _apiBaseUrl = configuration["ApiBaseUrl"] ?? "";
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Trigger warmup once per app lifecycle on the very first incoming request
            if (Interlocked.Exchange(ref _warmupTriggered, 1) == 0)
            {
                if (!string.IsNullOrEmpty(_apiBaseUrl))
                {
                    _logger.LogInformation("WarmupMiddleware: First request detected. Triggering background API ping to {Url}...", _apiBaseUrl);
                    
                    // Fire and forget so we don't delay the incoming request
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            var client = _httpClientFactory.CreateClient();
                            client.Timeout = TimeSpan.FromSeconds(30);
                            var response = await client.GetAsync($"{_apiBaseUrl}api/ping");
                            _logger.LogInformation("WarmupMiddleware: Background API ping completed with status {Status}", response.StatusCode);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "WarmupMiddleware: Background API ping failed.");
                        }
                    });
                }
            }

            await _next(context);
        }
    }
}
