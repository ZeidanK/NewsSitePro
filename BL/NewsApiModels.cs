using System.Text.Json.Serialization;

// ----------------------------------------------------------------------------------
// NewsApiModels.cs
//
// This file contains models for integrating with external news APIs in the NewsSitePro application.
// It defines response, article, source, configuration, system user, and job status models used for
// fetching, storing, and displaying news articles from third-party sources. Comments are added to key
// classes for clarity.
// ----------------------------------------------------------------------------------

namespace NewsSite.BL
{
    // Models for News API Integration
    public class NewsApiResponse
    // Model for the response returned by the News API
    {
        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("totalResults")]
        public int TotalResults { get; set; }

        [JsonPropertyName("articles")]
        public List<NewsApiArticle> Articles { get; set; } = new List<NewsApiArticle>();
    }

    public class NewsApiArticle
    // Model for a single news article returned by the News API
    {
        [JsonPropertyName("source")]
        public NewsApiSource Source { get; set; } = new NewsApiSource();

        [JsonPropertyName("author")]
        public string? Author { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; } = string.Empty;

        [JsonPropertyName("urlToImage")]
        public string? UrlToImage { get; set; }

        [JsonPropertyName("publishedAt")]
        public DateTime PublishedAt { get; set; }

        [JsonPropertyName("content")]
        public string? Content { get; set; }
    }

    public class NewsApiSource
    // Model for the source of a news article
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
    }

    // Configuration for News API
    public class NewsApiSettings
    // Configuration settings for the News API integration
    {
        public string ApiKey { get; set; } = string.Empty;
        public string BaseUrl { get; set; } = "https://newsapi.org/v2/";
        public List<string> Categories { get; set; } = new List<string> 
        { 
            "general", "business", "entertainment", "health", 
            "science", "sports", "technology" 
        };
        public List<string> Countries { get; set; } = new List<string> { "us", "gb", "ca" };
        public int MaxArticlesPerFetch { get; set; } = 20;
        public int FetchIntervalHours { get; set; } = 1;
    }

    // System user for posting News API articles
    public class SystemUser
    // Constants for the system user that posts News API articles
    {
        public const int SYSTEM_USER_ID = -1;
        public const string SYSTEM_USERNAME = "NewsBot";
        public const string SYSTEM_PROFILE_IMAGE = "/images/newsbot-avatar.png";
        public const string SYSTEM_BIO = "Automated news aggregator bringing you the latest headlines from trusted sources.";
    }

    // News fetch job status
    public class NewsFetchStatus
    // Model for tracking the status of news fetch jobs
    {
        public int Id { get; set; }
        public DateTime LastFetchTime { get; set; }
        public int ArticlesFetched { get; set; }
        public int ArticlesPosted { get; set; }
        public string? ErrorMessage { get; set; }
        public bool IsRunning { get; set; }
        public string? Category { get; set; }
    }
}
