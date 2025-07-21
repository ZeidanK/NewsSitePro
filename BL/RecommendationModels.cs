using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NewsSite.BL
{
    // Enhanced User Interest Tracking
    public class UserInterest
    {
        public int InterestID { get; set; }
        public int UserID { get; set; }
        public string Category { get; set; } = string.Empty;
        public double InterestScore { get; set; } = 0.0; // 0-1 scale based on interactions
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
        
        // Interest types
        public InterestType Type { get; set; } = InterestType.Category;
        public string? Keywords { get; set; } // JSON array of keywords
        public int InteractionCount { get; set; } = 0;
    }

    public enum InterestType
    {
        Category,
        Keyword,
        Source,
        Author,
        Geographic
    }

    // User Reading Behavior Analytics
    public class UserBehavior
    {
        public int UserID { get; set; }
        public int TotalViews { get; set; }
        public int TotalLikes { get; set; }
        public int TotalShares { get; set; }
        public int TotalComments { get; set; }
        public double AvgSessionDuration { get; set; }
        public DateTime LastActivity { get; set; }
        public TimeSpan PreferredReadingTime { get; set; }
        public int MostActiveHour { get; set; }
        public List<string> FavoriteCategories { get; set; } = new List<string>();
        
        // Enhanced behavior tracking
        public string? PreferredTimeSlots { get; set; } // JSON: ["morning", "evening"]
        public string? PreferredContentTypes { get; set; } // JSON: ["long_reads", "breaking_news"]
        public double AverageReadingTime { get; set; } // in seconds
        public string? PreferredSources { get; set; } // JSON array
        public string? DevicePreferences { get; set; } // mobile, desktop, tablet
        public string? GeographicPreferences { get; set; } // JSON: country codes
    }

    // Article Interaction Tracking
    public class ArticleInteraction
    {
        public int InteractionID { get; set; }
        public int UserID { get; set; }
        public int ArticleID { get; set; }
        public string InteractionType { get; set; } = string.Empty; // view, like, share, comment, save
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public double? ReadingProgress { get; set; } // 0-1 scale
        public int? TimeSpentSeconds { get; set; }
        public string? DeviceType { get; set; }
        public string? ReferrerSource { get; set; } // how they found the article
    }

    public enum InteractionTypeEnum
    {
        View = 1,
        Like = 2,
        Share = 3,
        Save = 4,
        Comment = 5,
        Click = 6,
        FullRead = 7, // Read to end
        QuickExit = 8 // Left quickly (negative signal)
    }

    // Content Similarity and Clustering
    public class ArticleSimilarity
    {
        public int ArticleID1 { get; set; }
        public int ArticleID2 { get; set; }
        public double SimilarityScore { get; set; } // 0-1
        public string SimilarityType { get; set; } = string.Empty; // content, category, keywords
        public DateTime ComputedAt { get; set; } = DateTime.UtcNow;
    }

    // Trending Topics
    public class TrendingTopic
    {
        public int TrendID { get; set; }
        public string Topic { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public double TrendScore { get; set; } // Calculated based on engagement
        public int TotalInteractions { get; set; }
        public DateTime LastUpdated { get; set; }
        public string? RelatedKeywords { get; set; } // JSON array
        public string? GeographicRegions { get; set; } // JSON array of regions where trending
    }

    // User Follow Relationships
    public class UserFollow
    {
        public int FollowerUserID { get; set; }
        public int FollowedUserID { get; set; }
        public DateTime FollowDate { get; set; } = DateTime.UtcNow;
        public bool NotificationsEnabled { get; set; } = true;
    }

    // Enhanced News Article for Recommendations
    public class RecommendedArticle
    {
        public NewsArticle Article { get; set; } = new NewsArticle();
        public double RecommendationScore { get; set; } = 0.0; // 0-1 scale
        public string RecommendationReason { get; set; } = string.Empty; // Why this was recommended
        public List<string> RecommendationFactors { get; set; } = new List<string>();
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
        public bool IsPersonalized { get; set; } = false;
        public bool IsTrending { get; set; } = false;
        public bool IsFromFollowedUser { get; set; } = false;
        public double FreshnessScore { get; set; } = 1.0; // Decreases with age
        public double PopularityScore { get; set; } = 0.0; // Based on total engagement
        public double PersonalizationScore { get; set; } = 0.0; // Based on user interests
    }

    // Feed Configuration
    public class FeedConfiguration
    {
        public int UserID { get; set; }
        public FeedAlgorithm Algorithm { get; set; } = FeedAlgorithm.Balanced;
        public bool ShowTrendingContent { get; set; } = true;
        public bool ShowFollowedUsers { get; set; } = true;
        public bool ShowRecommendedContent { get; set; } = true;
        public double PersonalizationWeight { get; set; } = 0.6; // 0-1
        public double FreshnessWeight { get; set; } = 0.3; // 0-1
        public double PopularityWeight { get; set; } = 0.1; // 0-1
        public double SerendipityWeight { get; set; } = 0.1; // 0-1
        public int MaxArticlesPerFeed { get; set; } = 20;
        public List<string> PreferredCategories { get; set; } = new List<string>();
        public List<string> ExcludedCategories { get; set; } = new List<string>();
        public DateTime LastUpdated { get; set; }
        
        // Enhanced configuration
        public string? BlockedSources { get; set; } // JSON array
        public string? BlockedUsers { get; set; } // JSON array of user IDs
        public bool EnableSerendipity { get; set; } = true; // Show some random content
    }

    public enum FeedAlgorithm
    {
        Chronological, // Most recent first
        Popular, // Most liked/viewed first
        Personalized, // Based on user interests
        Balanced, // Mix of all factors
        Trending, // Currently trending topics
        Following // Only from followed users
    }

    // Feed Sorting Options
    public class FeedSortOptions
    {
        public SortBy SortBy { get; set; } = SortBy.Recommended;
        public SortOrder Order { get; set; } = SortOrder.Descending;
        public TimeFilter TimeFilter { get; set; } = TimeFilter.All;
        public string? CategoryFilter { get; set; }
        public string? SourceFilter { get; set; }
        public bool OnlyFollowed { get; set; } = false;
        public bool IncludeTrending { get; set; } = true;
    }

    public enum SortBy
    {
        Recommended,
        Recent,
        Popular,
        Trending,
        MostLiked,
        MostViewed,
        MostCommented
    }

    public enum SortOrder
    {
        Ascending,
        Descending
    }

    public enum TimeFilter
    {
        LastHour,
        Last24Hours,
        LastWeek,
        LastMonth,
        All
    }

    // Content Quality Metrics
    public class ContentQuality
    {
        public int ArticleID { get; set; }
        public double QualityScore { get; set; } // 0-1 based on various factors
        public double EngagementRate { get; set; } // likes+comments+shares/views
        public double CompletionRate { get; set; } // how many read to end
        public double ShareabilityScore { get; set; } // how often it's shared
        public bool IsFakeNews { get; set; } = false; // Moderation flag
        public bool IsSpam { get; set; } = false;
        public int ReportCount { get; set; } = 0;
        public DateTime LastQualityCheck { get; set; } = DateTime.UtcNow;
    }

    // Geographic Interest Tracking
    public class GeographicInterest
    {
        public int UserID { get; set; }
        public string CountryCode { get; set; } = string.Empty;
        public string? RegionCode { get; set; }
        public string? CityName { get; set; }
        public double InterestScore { get; set; } = 0.0;
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }

    // Advanced Analytics Models
    public class UserEngagementAnalytics
    {
        public int UserID { get; set; }
        public DateTime AnalyticsDate { get; set; }
        public int TotalViews { get; set; }
        public int TotalLikes { get; set; }
        public int TotalShares { get; set; }
        public int TotalComments { get; set; }
        public int TotalSaves { get; set; }
        public double AverageSessionTime { get; set; }
        public int SessionCount { get; set; }
        public string? TopCategories { get; set; } // JSON array
        public string? TopSources { get; set; } // JSON array
        public double EngagementScore { get; set; } // Overall engagement metric
    }

    // API Request/Response Models
    public class FeedRequest
    {
        public int PageSize { get; set; } = 20;
        public int PageNumber { get; set; } = 1;
        public string? Algorithm { get; set; } // personalized, trending, popular, recent
        public List<string>? Categories { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public FeedSortOptions? SortOptions { get; set; }
    }

    public class FeedResponse
    {
        public List<RecommendedArticle> Articles { get; set; } = new List<RecommendedArticle>();
        public int TotalCount { get; set; }
        public int PageSize { get; set; }
        public int PageNumber { get; set; }
        public string Algorithm { get; set; } = string.Empty;
        public DateTime GeneratedAt { get; set; }
        public List<TrendingTopic> TrendingTopics { get; set; } = new List<TrendingTopic>();
        public UserEngagementAnalytics? UserAnalytics { get; set; }
        public List<string> AppliedFilters { get; set; } = new List<string>();
    }

    public class InteractionRequest
    {
        public int ArticleID { get; set; }
        public string InteractionType { get; set; } = string.Empty;
        public string? AdditionalData { get; set; }
        public int? TimeSpentSeconds { get; set; }
        public double? ReadingProgress { get; set; }
        public string? DeviceType { get; set; }
    }

    public class RecommendationScore
    {
        public double Score { get; set; }
        public string Reason { get; set; } = string.Empty;
        public List<string> Factors { get; set; } = new List<string>();
        public double PersonalizationScore { get; set; }
        public double FreshnessScore { get; set; }
        public double PopularityScore { get; set; }
        public double SerendipityScore { get; set; }
    }

    // Machine Learning Features
    public class MLFeatureVector
    {
        public int UserID { get; set; }
        public int ArticleID { get; set; }
        public double[] Features { get; set; } = new double[0];
        public string[] FeatureNames { get; set; } = new string[0];
        public DateTime ComputedAt { get; set; } = DateTime.UtcNow;
    }

    public class PersonalizationInsights
    {
        public int UserID { get; set; }
        public Dictionary<string, double> CategoryAffinities { get; set; } = new Dictionary<string, double>();
        public Dictionary<string, double> SourceAffinities { get; set; } = new Dictionary<string, double>();
        public Dictionary<int, double> TimeSlotPreferences { get; set; } = new Dictionary<int, double>(); // hour -> preference
        public List<string> RecentInterests { get; set; } = new List<string>();
        public List<string> EmergingInterests { get; set; } = new List<string>();
        public double DiversityPreference { get; set; } = 0.5; // 0 = very focused, 1 = very diverse
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }
}
