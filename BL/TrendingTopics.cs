using System.ComponentModel.DataAnnotations;

namespace NewsSite.BL
{
    /// <summary>
    /// Represents a trending topic in the system
    /// Enhanced with engagement metrics and calculated scores
    /// </summary>
    public class TrendingTopic
    {
        public int TrendID { get; set; }
        
        [Required]
        [StringLength(200)]
        public string Topic { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100)]
        public string Category { get; set; } = string.Empty;
        
        /// <summary>
        /// Calculated trend score based on engagement metrics with time decay
        /// Higher scores indicate more trending content
        /// </summary>
        public double TrendScore { get; set; }
        
        /// <summary>
        /// Total number of interactions (likes + comments + views)
        /// </summary>
        public int TotalInteractions { get; set; }
        
        /// <summary>
        /// When this trending topic was last calculated/updated
        /// </summary>
        public DateTime LastUpdated { get; set; }
        
        /// <summary>
        /// JSON array of related keywords for this trending topic
        /// </summary>
        public string? RelatedKeywords { get; set; }
        
        /// <summary>
        /// JSON array of geographic regions where this topic is trending
        /// </summary>
        public string? GeographicRegions { get; set; }
        
        // Additional properties for enhanced trending analysis
        public int LikesCount { get; set; }
        public int CommentsCount { get; set; }
        public int ViewsCount { get; set; }
        public double EngagementRate { get; set; }
        public int RelatedArticlesCount { get; set; }
    }

    /// <summary>
    /// Request model for trending topics with filtering options
    /// </summary>
    public class TrendingTopicsRequest
    {
        /// <summary>
        /// Number of trending topics to return (default: 10)
        /// </summary>
        public int Count { get; set; } = 10;
        
        /// <summary>
        /// Filter by specific category (optional)
        /// </summary>
        public string? Category { get; set; }
        
        /// <summary>
        /// Minimum trend score threshold (default: 0.0)
        /// </summary>
        public double MinScore { get; set; } = 0.0;
        
        /// <summary>
        /// Time window in hours for trending calculation (default: 24)
        /// </summary>
        public int TimeWindowHours { get; set; } = 24;
        
        /// <summary>
        /// Whether to include related articles count
        /// </summary>
        public bool IncludeRelatedArticles { get; set; } = false;
    }

    /// <summary>
    /// Response model for trending topics API
    /// </summary>
    public class TrendingTopicsResponse
    {
        public List<TrendingTopic> Topics { get; set; } = new List<TrendingTopic>();
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
        public int TotalCount { get; set; }
        public string? Category { get; set; }
        public double MinScore { get; set; }
        public int TimeWindowHours { get; set; }
    }

    /// <summary>
    /// Model for trending topic calculation parameters
    /// </summary>
    public class TrendingCalculationConfig
    {
        /// <summary>
        /// Weight for likes in trend score calculation (default: 3.0)
        /// </summary>
        public double LikesWeight { get; set; } = 3.0;
        
        /// <summary>
        /// Weight for comments in trend score calculation (default: 5.0)
        /// </summary>
        public double CommentsWeight { get; set; } = 5.0;
        
        /// <summary>
        /// Weight for views in trend score calculation (default: 1.0)
        /// </summary>
        public double ViewsWeight { get; set; } = 1.0;
        
        /// <summary>
        /// Time decay factor for trending calculation (default: 0.9)
        /// Higher values mean slower decay
        /// </summary>
        public double TimeDecayFactor { get; set; } = 0.9;
        
        /// <summary>
        /// Maximum number of trending topics to maintain (default: 20)
        /// </summary>
        public int MaxTrendingTopics { get; set; } = 20;
        
        /// <summary>
        /// Time window in hours for trending calculation (default: 24)
        /// </summary>
        public int TimeWindowHours { get; set; } = 24;
        
        /// <summary>
        /// Minimum interactions required for a topic to be considered trending
        /// </summary>
        public int MinInteractionsThreshold { get; set; } = 5;
    }

    /// <summary>
    /// Model for trending topic analytics and insights
    /// </summary>
    public class TrendingTopicAnalytics
    {
        public string Topic { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public double CurrentScore { get; set; }
        public double PreviousScore { get; set; }
        public double ScoreChange { get; set; }
        public string Trend { get; set; } = string.Empty; // "rising", "falling", "stable"
        public int HoursInTrending { get; set; }
        public List<TrendingMetric> HourlyMetrics { get; set; } = new List<TrendingMetric>();
    }

    /// <summary>
    /// Model for hourly trending metrics
    /// </summary>
    public class TrendingMetric
    {
        public DateTime Hour { get; set; }
        public double Score { get; set; }
        public int Interactions { get; set; }
        public int Likes { get; set; }
        public int Comments { get; set; }
        public int Views { get; set; }
    }

    /// <summary>
    /// Model for trending topic with related articles
    /// </summary>
    public class TrendingTopicWithArticles
    {
        public TrendingTopic Topic { get; set; } = new TrendingTopic();
        public List<NewsArticle> RelatedArticles { get; set; } = new List<NewsArticle>();
        public int TotalRelatedArticles { get; set; }
    }
}
