using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace NewsSite.BackgroundServices
{
    /// <summary>
    /// Background service to automatically calculate and refresh trending topics
    /// Runs periodically to keep trending data up-to-date based on engagement metrics
    /// </summary>
    public class TrendingTopicsBackgroundService : BackgroundService
    {
        private readonly ILogger<TrendingTopicsBackgroundService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly TimeSpan _refreshInterval = TimeSpan.FromMinutes(30); // Refresh every 30 minutes

        public TrendingTopicsBackgroundService(
            ILogger<TrendingTopicsBackgroundService> logger,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Main execution method for the background service
        /// </summary>
        /// <param name="stoppingToken">Cancellation token</param>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Trending Topics Background Service started");

            // Initial calculation after a short delay
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await RefreshTrendingTopics();
                    
                    // Wait for the next refresh interval
                    await Task.Delay(_refreshInterval, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    // Expected when the service is stopping
                    _logger.LogInformation("Trending Topics Background Service is stopping");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while refreshing trending topics");
                    
                    // Wait a shorter interval before retrying on error
                    await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
                }
            }
        }

        /// <summary>
        /// Refreshes trending topics calculations
        /// </summary>
        private async Task RefreshTrendingTopics()
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var dbServices = scope.ServiceProvider.GetRequiredService<DBservices>();

                _logger.LogInformation("Starting trending topics calculation...");

                // Calculate trending topics based on last 24 hours of engagement
                var (success, message) = await dbServices.CalculateTrendingTopicsAsync(
                    timeWindowHours: 24, 
                    maxTopics: 20
                );

                if (success)
                {
                    _logger.LogInformation("Trending topics calculated successfully: {Message}", message);
                    
                    // Clean up old trending topics (older than 48 hours)
                    var (deletedCount, cleanupMessage) = await dbServices.CleanupOldTrendingTopicsAsync(48);
                    
                    if (deletedCount > 0)
                    {
                        _logger.LogInformation("Cleaned up {DeletedCount} old trending topics: {CleanupMessage}", 
                            deletedCount, cleanupMessage);
                    }
                }
                else
                {
                    _logger.LogWarning("Failed to calculate trending topics: {Message}", message);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing trending topics");
                throw;
            }
        }

        /// <summary>
        /// Called when the service is stopping
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Trending Topics Background Service is stopping");
            await base.StopAsync(cancellationToken);
        }
    }
}
