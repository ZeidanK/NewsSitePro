using NewsSite.Services;

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
                    using var scope = _serviceProvider.CreateScope();
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
