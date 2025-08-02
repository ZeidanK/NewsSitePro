using NewsSite.Services;
using Microsoft.Extensions.Caching.Memory;

namespace NewsSite.BackgroundServices
{
    public class NewsApiBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<NewsApiBackgroundService> _logger;
        private readonly TimeSpan _period = TimeSpan.FromHours(24); // Fetch news every 24 hours to conserve API calls

        public NewsApiBackgroundService(IServiceProvider serviceProvider, ILogger<NewsApiBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("News API Background Service started - Running every 24 hours to conserve API quota");

            // Initial delay to let the application start up
            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Check if background service is enabled via cache
                    using var scope = _serviceProvider.CreateScope();
                    var memoryCache = scope.ServiceProvider.GetService<IMemoryCache>();
                    var isEnabled = false; // Default to DISABLED for safety

                    if (memoryCache != null && memoryCache.TryGetValue("BackgroundService:NewsSync:Enabled", out var cachedValue))
                    {
                        isEnabled = (bool)cachedValue;
                    }
                    else
                    {
                        // If no cache value exists, check configuration or default to disabled
                        var configuration = scope.ServiceProvider.GetService<IConfiguration>();
                        isEnabled = configuration?.GetValue<bool>("BackgroundServices:NewsSync:Enabled") ?? false;
                        
                        // Store the initial value in cache
                        if (memoryCache != null)
                        {
                            memoryCache.Set("BackgroundService:NewsSync:Enabled", isEnabled, TimeSpan.FromHours(24));
                        }
                    }

                    if (!isEnabled)
                    {
                        _logger.LogInformation("Background service is disabled by admin. Skipping news sync.");
                        await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken); // Check more frequently when disabled
                        continue;
                    }

                    var newsApiService = scope.ServiceProvider.GetRequiredService<INewsApiService>();
                    
                    _logger.LogInformation("Starting daily news sync to conserve API requests...");
                    var articlesAdded = await newsApiService.SyncNewsArticlesToDatabase();
                    _logger.LogInformation($"Daily background sync completed. Added {articlesAdded} new articles");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred during background news sync");
                }

                await Task.Delay(_period, stoppingToken); 
            }
        }
    }
}
