namespace Spendly.Web.Services
{
    /// <summary>
    /// Background service that periodically pings the API to keep it warm.
    /// Uses /api/ping (NO database) to avoid burning Azure SQL free tier quota.
    /// Only runs when the Web App is alive (kept alive by UptimeRobot).
    /// </summary>
    public class ApiWarmupService : BackgroundService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<ApiWarmupService> _logger;
        private readonly string _apiBaseUrl;
        private readonly TimeSpan _interval = TimeSpan.FromMinutes(4);
        private readonly TimeSpan _retryInterval = TimeSpan.FromSeconds(15);
        private const int MaxRetries = 3;

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

            // Wait 2 seconds instead of 10 for the app to start before first ping
            await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                bool success = false;
                int attempts = 0;

                while (attempts < MaxRetries && !success && !stoppingToken.IsCancellationRequested)
                {
                    attempts++;
                    try
                    {
                        var client = _httpClientFactory.CreateClient();
                        client.Timeout = TimeSpan.FromSeconds(45); // Give Azure API plenty of time to wake up

                        var sw = System.Diagnostics.Stopwatch.StartNew();
                        var response = await client.GetAsync($"{_apiBaseUrl}api/ping", stoppingToken);
                        sw.Stop();

                        if (response.IsSuccessStatusCode)
                        {
                            success = true;
                            _logger.LogInformation(
                                "ApiWarmupService: Ping successful (Attempt {Attempt}/{Max}): Status {Status} in {Elapsed}ms",
                                attempts, MaxRetries, response.StatusCode, sw.ElapsedMilliseconds);
                        }
                        else
                        {
                            _logger.LogWarning(
                                "ApiWarmupService: Ping returned non-success (Attempt {Attempt}/{Max}): Status {Status} in {Elapsed}ms",
                                attempts, MaxRetries, response.StatusCode, sw.ElapsedMilliseconds);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "ApiWarmupService: Ping failed (Attempt {Attempt}/{Max}).", attempts, MaxRetries);
                    }

                    if (!success && attempts < MaxRetries && !stoppingToken.IsCancellationRequested)
                    {
                        _logger.LogInformation("ApiWarmupService: Waiting {Delay}s before retrying ping...", _retryInterval.TotalSeconds);
                        await Task.Delay(_retryInterval, stoppingToken);
                    }
                }

                await Task.Delay(_interval, stoppingToken);
            }
        }
    }
}
