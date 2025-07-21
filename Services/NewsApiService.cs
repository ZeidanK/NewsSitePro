using NewsSite.BL;
using System.Text.Json;

namespace NewsSite.Services
{
    public interface INewsApiService
    {
        Task<List<NewsApiArticle>> FetchTopHeadlinesAsync(string category = "general", string country = "us", int pageSize = 20);
        Task<List<NewsApiArticle>> FetchEverythingAsync(string query, int pageSize = 20);
        Task<List<NewsApiArticle>> FetchSourceArticlesAsync(string sources, int pageSize = 20);
        Task<int> SyncNewsArticlesToDatabase();
        Task<bool> CreateSystemUserIfNotExists();
    }

    public class NewsApiService : INewsApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly DBservices _dbServices;
        private readonly NewsApiSettings _settings;
        private readonly ILogger<NewsApiService> _logger;

        public NewsApiService(HttpClient httpClient, IConfiguration configuration, DBservices dbServices, ILogger<NewsApiService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _dbServices = dbServices;
            _logger = logger;
            
            _settings = new NewsApiSettings
            {
                ApiKey = _configuration["NewsApi:ApiKey"] ?? throw new ArgumentException("NewsApi:ApiKey not configured"),
                BaseUrl = _configuration["NewsApi:BaseUrl"] ?? "https://newsapi.org/v2/",
                MaxArticlesPerFetch = int.Parse(_configuration["NewsApi:MaxArticlesPerFetch"] ?? "20"),
                FetchIntervalHours = int.Parse(_configuration["NewsApi:FetchIntervalHours"] ?? "1")
            };

            // News API expects the API key as a query parameter
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "NewsSitePro/1.0 (News Aggregator)");
        }

        public async Task<List<NewsApiArticle>> FetchTopHeadlinesAsync(string category = "general", string country = "us", int pageSize = 20)
        {
            try
            {
                var url = $"{_settings.BaseUrl}top-headlines?country={country}&category={category}&pageSize={pageSize}&apiKey={_settings.ApiKey}";
                _logger.LogInformation($"Fetching from URL: {url.Replace(_settings.ApiKey, "***APIKEY***")}");
                
                var response = await _httpClient.GetAsync(url);
                var jsonContent = await response.Content.ReadAsStringAsync();
                
                _logger.LogInformation($"Response Status: {response.StatusCode}");
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"Failed to fetch news: {response.StatusCode} - {response.ReasonPhrase}");
                    _logger.LogError($"Response content: {jsonContent}");
                    return new List<NewsApiArticle>();
                }

                var newsResponse = JsonSerializer.Deserialize<NewsApiResponse>(jsonContent);
                return newsResponse?.Articles ?? new List<NewsApiArticle>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching top headlines");
                return new List<NewsApiArticle>();
            }
        }

        public async Task<List<NewsApiArticle>> FetchEverythingAsync(string query, int pageSize = 20)
        {
            try
            {
                var url = $"{_settings.BaseUrl}everything?q={query}&pageSize={pageSize}&apiKey={_settings.ApiKey}";
                var response = await _httpClient.GetAsync(url);
                var jsonContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"Failed to fetch news: {response.StatusCode} - {response.ReasonPhrase}");
                    return new List<NewsApiArticle>();
                }

                var newsResponse = JsonSerializer.Deserialize<NewsApiResponse>(jsonContent);
                return newsResponse?.Articles ?? new List<NewsApiArticle>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching everything");
                return new List<NewsApiArticle>();
            }
        }

        public async Task<List<NewsApiArticle>> FetchSourceArticlesAsync(string sources, int pageSize = 20)
        {
            try
            {
                var url = $"{_settings.BaseUrl}top-headlines?sources={sources}&pageSize={pageSize}&apiKey={_settings.ApiKey}";
                var response = await _httpClient.GetAsync(url);
                var jsonContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"Failed to fetch news: {response.StatusCode} - {response.ReasonPhrase}");
                    return new List<NewsApiArticle>();
                }

                var newsResponse = JsonSerializer.Deserialize<NewsApiResponse>(jsonContent);
                return newsResponse?.Articles ?? new List<NewsApiArticle>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching source articles");
                return new List<NewsApiArticle>();
            }
        }

        public async Task<bool> CreateSystemUserIfNotExists()
        {
            try
            {
                // Use user ID 1 (admin) instead of -1 to avoid foreign key issues
                var systemUser = _dbServices.GetUserById(1);
                if (systemUser == null)
                {
                    _logger.LogWarning("User ID 1 (admin) not found. Please ensure at least one user exists in the database.");
                    return false;
                }
                
                _logger.LogInformation($"Using existing user ID 1 ({systemUser.Name}) for news articles");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking for admin user");
                return false;
            }
        }

        public async Task<int> SyncNewsArticlesToDatabase()
        {
            try
            {
                _logger.LogInformation("Starting news sync process...");
                
                // Ensure system user exists
                var systemUserExists = await CreateSystemUserIfNotExists();
                if (!systemUserExists)
                {
                    _logger.LogError("Failed to create or verify system user. Aborting sync.");
                    return 0;
                }
                
                int totalArticlesAdded = 0;
                
                _logger.LogInformation($"Syncing articles for {_settings.Categories.Count} categories: {string.Join(", ", _settings.Categories)}");
                
                // Fetch articles from different categories
                foreach (var category in _settings.Categories)
                {
                    try
                    {
                        _logger.LogInformation($"Fetching articles for category: {category}");
                        var articles = await FetchTopHeadlinesAsync(category, "us", 5); // Reduced to 5 per category to conserve API quota
                        
                        _logger.LogInformation($"Retrieved {articles.Count} articles for category: {category}");
                        
                        foreach (var apiArticle in articles)
                        {
                            try
                            {
                                // Check if article has required fields
                                if (string.IsNullOrWhiteSpace(apiArticle.Title))
                                {
                                    _logger.LogWarning("Skipping article with empty title");
                                    continue;
                                }

                                // Create NewsArticle object
                                var newsArticle = new NewsArticle
                                {
                                    Title = TruncateString(apiArticle.Title, 100), // Ensure title fits DB constraint
                                    Content = TruncateString(apiArticle.Content ?? apiArticle.Description ?? "", 4000), // Ensure content fits DB constraint
                                    SourceURL = TruncateString(apiArticle.Url, 500),
                                    SourceName = TruncateString(apiArticle.Source?.Name ?? "Unknown", 100),
                                    ImageURL = TruncateString(apiArticle.UrlToImage, 255),
                                    PublishDate = apiArticle.PublishedAt,
                                    Category = TruncateString(category, 50),
                                    UserID = 1, // Changed from -1 to 1 (using admin user instead of system user)
                                    Username = "NewsBot"
                                };

                                _logger.LogDebug($"Attempting to create article: {newsArticle.Title}");
                                
                                var articleId = _dbServices.CreateNewsArticle(newsArticle);
                                if (articleId > 0)
                                {
                                    totalArticlesAdded++;
                                    _logger.LogInformation($"✓ Created article ID {articleId}: {newsArticle.Title}");
                                }
                                else
                                {
                                    _logger.LogWarning($"✗ Failed to create article: {newsArticle.Title}");
                                }
                            }
                            catch (Exception articleEx)
                            {
                                _logger.LogError(articleEx, $"Error creating individual article: {apiArticle.Title}");
                            }
                        }
                        
                        // Small delay between categories to avoid rate limiting
                        await Task.Delay(2000); // Increased delay
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error syncing articles for category: {category}");
                    }
                }

                _logger.LogInformation($"Sync completed. Added {totalArticlesAdded} new articles out of {_settings.Categories.Count * 5} attempted");
                return totalArticlesAdded;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during news sync");
                return 0;
            }
        }

        private string TruncateString(string? input, int maxLength)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;
            
            return input.Length <= maxLength ? input : input.Substring(0, maxLength);
        }
    }
}
