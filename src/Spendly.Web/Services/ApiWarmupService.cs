namespace Spendly.Web.Services
{
    /// <summary>
    /// Background service that periodically pings the API to keep it warm.
    /// This prevents Azure F1 Free Tier from putting the API to sleep.
    /// Only runs when the Web App is alive (kept alive by UptimeRobot).
    /// </summary>
    public class ApiWarmupService : BackgroundService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<ApiWarmupService> _logger;
        private readonly string _apiBaseUrl;
        private readonly TimeSpan _interval = TimeSpan.FromMinutes(14);

        public ApiWarmupService(
            IHttpClientFactory httpClientFactory,
            ILogger<ApiWarmupService> logger,
            IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _apiBaseUrl = configuration["ApiBaseUrl"] ?? "";
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Yield immediately so we don't block app startup
            await Task.Yield();

            if (string.IsNullOrEmpty(_apiBaseUrl))
            {
                _logger.LogWarning("ApiWarmupService: ApiBaseUrl not configured. Service disabled.");
                return;
            }

            _logger.LogInformation("ApiWarmupService started. Pinging {Url} every {Interval} min.",
                _apiBaseUrl, _interval.TotalMinutes);

            // Wait 10 seconds for the app to fully start before first ping
            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var client = _httpClientFactory.CreateClient();
                    client.Timeout = TimeSpan.FromSeconds(90); // Azure SQL can take up to 60s to wake

                    var sw = System.Diagnostics.Stopwatch.StartNew();
                    var response = await client.GetAsync($"{_apiBaseUrl}api/health", stoppingToken);
                    sw.Stop();

                    _logger.LogInformation(
                        "ApiWarmupService: Ping {Status} in {Elapsed}ms",
                        response.StatusCode, sw.ElapsedMilliseconds);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "ApiWarmupService: Ping failed.");
                }

                await Task.Delay(_interval, stoppingToken);
            }
        }
    }
}
