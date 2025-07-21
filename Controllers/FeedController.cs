using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NewsSite.BL;
using NewsSite.Services;
using System.Security.Claims;

namespace NewsSite.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FeedController : ControllerBase
    {
        private readonly IRecommendationService _recommendationService;
        private readonly DBservices _dbServices;

        public FeedController(IRecommendationService recommendationService, DBservices dbServices)
        {
            _recommendationService = recommendationService;
            _dbServices = dbServices;
        }

        /// <summary>
        /// Get personalized feed for the authenticated user
        /// </summary>
        [HttpGet("personalized")]
        [Authorize]
        public async Task<ActionResult<FeedResponse>> GetPersonalizedFeed([FromQuery] FeedRequest request)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                    return Unauthorized("User not authenticated");

                var feed = await _recommendationService.GetPersonalizedFeedAsync(userId.Value, request);
                return Ok(feed);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to generate personalized feed", details = ex.Message });
            }
        }

        /// <summary>
        /// Get feed by specific algorithm (trending, popular, recent, following)
        /// </summary>
        [HttpGet("{algorithm}")]
        public async Task<ActionResult<FeedResponse>> GetFeedByAlgorithm(
            string algorithm, 
            [FromQuery] FeedRequest request)
        {
            try
            {
                var userId = GetCurrentUserId() ?? 0; // Use 0 for anonymous users
                var feed = await _recommendationService.GetFeedByAlgorithmAsync(userId, algorithm, request);
                return Ok(feed);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"Failed to generate {algorithm} feed", details = ex.Message });
            }
        }

        /// <summary>
        /// Get trending topics
        /// </summary>
        [HttpGet("trending-topics")]
        public async Task<ActionResult<List<TrendingTopic>>> GetTrendingTopics([FromQuery] int count = 10)
        {
            try
            {
                var topics = await _recommendationService.GetTrendingTopicsAsync(count);
                return Ok(topics);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to get trending topics", details = ex.Message });
            }
        }

        /// <summary>
        /// Record user interaction with an article
        /// </summary>
        [HttpPost("interaction")]
        [Authorize]
        public async Task<ActionResult> RecordInteraction([FromBody] InteractionRequest request)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                    return Unauthorized("User not authenticated");

                await _recommendationService.RecordInteractionAsync(userId.Value, request);
                return Ok(new { success = true, message = "Interaction recorded successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to record interaction", details = ex.Message });
            }
        }

        /// <summary>
        /// Get similar articles to a specific article
        /// </summary>
        [HttpGet("similar/{articleId}")]
        public async Task<ActionResult<List<RecommendedArticle>>> GetSimilarArticles(
            int articleId, 
            [FromQuery] int count = 10)
        {
            try
            {
                var userId = GetCurrentUserId() ?? 0;
                var similarArticles = await _recommendationService.GetSimilarArticlesAsync(articleId, userId, count);
                return Ok(similarArticles);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to get similar articles", details = ex.Message });
            }
        }

        /// <summary>
        /// Get user's personalization insights
        /// </summary>
        [HttpGet("insights")]
        [Authorize]
        public async Task<ActionResult<PersonalizationInsights>> GetPersonalizationInsights()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                    return Unauthorized("User not authenticated");

                var insights = await _recommendationService.GetUserPersonalizationInsightsAsync(userId.Value);
                return Ok(insights);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to get personalization insights", details = ex.Message });
            }
        }

        /// <summary>
        /// Get user's feed configuration
        /// </summary>
        [HttpGet("configuration")]
        [Authorize]
        public async Task<ActionResult<FeedConfiguration>> GetFeedConfiguration()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                    return Unauthorized("User not authenticated");

                var config = await _recommendationService.GetUserFeedConfigurationAsync(userId.Value);
                return Ok(config);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to get feed configuration", details = ex.Message });
            }
        }

        /// <summary>
        /// Update user's feed configuration
        /// </summary>
        [HttpPut("configuration")]
        [Authorize]
        public async Task<ActionResult> UpdateFeedConfiguration([FromBody] FeedConfiguration configuration)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                    return Unauthorized("User not authenticated");

                var success = await _recommendationService.UpdateUserFeedConfigurationAsync(userId.Value, configuration);
                if (success)
                    return Ok(new { success = true, message = "Feed configuration updated successfully" });
                else
                    return BadRequest(new { error = "Failed to update feed configuration" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to update feed configuration", details = ex.Message });
            }
        }

        /// <summary>
        /// Get user engagement analytics
        /// </summary>
        [HttpGet("analytics")]
        [Authorize]
        public async Task<ActionResult<UserEngagementAnalytics>> GetUserAnalytics([FromQuery] DateTime? fromDate = null)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                    return Unauthorized("User not authenticated");

                var analytics = await _recommendationService.GetUserAnalyticsAsync(userId.Value, fromDate);
                return Ok(analytics);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to get user analytics", details = ex.Message });
            }
        }

        /// <summary>
        /// Reset user personalization (clear all learned preferences)
        /// </summary>
        [HttpPost("reset-personalization")]
        [Authorize]
        public async Task<ActionResult> ResetPersonalization()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                    return Unauthorized("User not authenticated");

                var success = await _recommendationService.ResetUserPersonalizationAsync(userId.Value);
                if (success)
                    return Ok(new { success = true, message = "Personalization reset successfully" });
                else
                    return BadRequest(new { error = "Failed to reset personalization" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to reset personalization", details = ex.Message });
            }
        }

        /// <summary>
        /// Get serendipity feed (diverse, unexplored content)
        /// </summary>
        [HttpGet("serendipity")]
        [Authorize]
        public async Task<ActionResult<List<NewsArticle>>> GetSerendipityFeed([FromQuery] int count = 10)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                    return Unauthorized("User not authenticated");

                var articles = await _recommendationService.GetSerendipityFeedAsync(userId.Value, count);
                return Ok(articles);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to get serendipity feed", details = ex.Message });
            }
        }

        /// <summary>
        /// Refresh trending topics (admin function)
        /// </summary>
        [HttpPost("refresh-trending")]
        [Authorize] // In production, this should be admin-only
        public async Task<ActionResult> RefreshTrendingTopics()
        {
            try
            {
                await _recommendationService.RefreshTrendingTopicsAsync();
                return Ok(new { success = true, message = "Trending topics refreshed successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to refresh trending topics", details = ex.Message });
            }
        }

        /// <summary>
        /// Get advanced sorted feed with multiple sorting options
        /// </summary>
        [HttpPost("sorted")]
        public async Task<ActionResult<FeedResponse>> GetSortedFeed([FromBody] SortedFeedRequest request)
        {
            try
            {
                var userId = GetCurrentUserId() ?? 0;
                
                // Get base articles
                var articles = new List<NewsArticle>();
                
                switch (request.SortOptions.SortBy)
                {
                    case SortBy.Popular:
                        articles = await _dbServices.GetPopularArticlesAsync(request.PageSize * 2);
                        break;
                    case SortBy.Trending:
                        articles = await _dbServices.GetTrendingArticlesAsync(request.PageSize * 2);
                        break;
                    case SortBy.MostLiked:
                        articles = await _dbServices.GetMostLikedArticlesAsync(request.PageSize * 2);
                        break;
                    case SortBy.MostViewed:
                        articles = await _dbServices.GetMostViewedArticlesAsync(request.PageSize * 2);
                        break;
                    case SortBy.Recent:
                        articles = await _dbServices.GetRecentArticlesAsync(request.PageSize * 2);
                        break;
                    default:
                        var personalizedFeed = await _recommendationService.GetPersonalizedFeedAsync(userId, new FeedRequest 
                        { 
                            PageSize = request.PageSize * 2,
                            PageNumber = 1 
                        });
                        articles = personalizedFeed.Articles.Select(ra => ra.Article).ToList();
                        break;
                }

                // Apply filters
                articles = ApplyFilters(articles, request.SortOptions);

                // Apply sorting
                articles = ApplySorting(articles, request.SortOptions);

                // Apply pagination
                var paginatedArticles = articles
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .ToList();

                var response = new FeedResponse
                {
                    Articles = paginatedArticles.Select(a => new RecommendedArticle 
                    { 
                        Article = a,
                        RecommendationScore = CalculateDisplayScore(a, request.SortOptions.SortBy),
                        RecommendationReason = GetSortingReason(request.SortOptions.SortBy),
                        GeneratedAt = DateTime.UtcNow
                    }).ToList(),
                    TotalCount = articles.Count,
                    PageSize = request.PageSize,
                    PageNumber = request.PageNumber,
                    Algorithm = $"sorted_{request.SortOptions.SortBy}",
                    GeneratedAt = DateTime.UtcNow,
                    AppliedFilters = BuildFilterDescription(request.SortOptions)
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to get sorted feed", details = ex.Message });
            }
        }

        /// <summary>
        /// Get mixed algorithm feed combining multiple approaches
        /// </summary>
        [HttpGet("mixed")]
        public async Task<ActionResult<FeedResponse>> GetMixedFeed([FromQuery] MixedFeedRequest request)
        {
            try
            {
                var userId = GetCurrentUserId() ?? 0;
                var allArticles = new List<RecommendedArticle>();

                // Get articles from different algorithms
                if (request.IncludePersonalized && userId > 0)
                {
                    var personalizedFeed = await _recommendationService.GetPersonalizedFeedAsync(userId, new FeedRequest { PageSize = request.PersonalizedCount });
                    allArticles.AddRange(personalizedFeed.Articles);
                }

                if (request.IncludeTrending)
                {
                    var trendingFeed = await _recommendationService.GetFeedByAlgorithmAsync(userId, "trending", new FeedRequest { PageSize = request.TrendingCount });
                    allArticles.AddRange(trendingFeed.Articles);
                }

                if (request.IncludePopular)
                {
                    var popularFeed = await _recommendationService.GetFeedByAlgorithmAsync(userId, "popular", new FeedRequest { PageSize = request.PopularCount });
                    allArticles.AddRange(popularFeed.Articles);
                }

                if (request.IncludeRecent)
                {
                    var recentFeed = await _recommendationService.GetFeedByAlgorithmAsync(userId, "recent", new FeedRequest { PageSize = request.RecentCount });
                    allArticles.AddRange(recentFeed.Articles);
                }

                // Remove duplicates and apply mixing algorithm
                var uniqueArticles = allArticles
                    .GroupBy(a => a.Article.ArticleID)
                    .Select(g => g.OrderByDescending(a => a.RecommendationScore).First())
                    .ToList();

                // Apply intelligent mixing (interleave different types)
                var mixedArticles = ApplyIntelligentMixing(uniqueArticles, request);

                // Apply pagination
                var paginatedArticles = mixedArticles
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .ToList();

                var response = new FeedResponse
                {
                    Articles = paginatedArticles,
                    TotalCount = uniqueArticles.Count,
                    PageSize = request.PageSize,
                    PageNumber = request.PageNumber,
                    Algorithm = "mixed",
                    GeneratedAt = DateTime.UtcNow,
                    TrendingTopics = await _recommendationService.GetTrendingTopicsAsync(5)
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to get mixed feed", details = ex.Message });
            }
        }

        /// <summary>
        /// Get feed for a specific category with smart recommendations
        /// </summary>
        [HttpGet("category/{category}")]
        public async Task<ActionResult<FeedResponse>> GetCategoryFeed(
            string category,
            [FromQuery] int pageSize = 20,
            [FromQuery] int pageNumber = 1,
            [FromQuery] string sortBy = "recommended")
        {
            try
            {
                var userId = GetCurrentUserId() ?? 0;
                
                // Get articles from the category
                var articles = await _dbServices.GetArticlesByInterestAsync(userId, category, pageSize * 2);
                
                // If user is authenticated, personalize the category feed
                if (userId > 0)
                {
                    var userInterests = await _dbServices.GetUserInterestsAsync(userId);
                    var categoryInterest = userInterests.FirstOrDefault(i => i.Category.Equals(category, StringComparison.OrdinalIgnoreCase));
                    
                    // Boost articles if user has shown interest in this category
                    if (categoryInterest != null)
                    {
                        articles = articles.OrderByDescending(a => 
                            CalculateDisplayScore(a, SortBy.Recommended) * (1 + categoryInterest.InterestScore * 0.5)
                        ).ToList();
                    }
                }

                // Apply sorting
                articles = sortBy.ToLower() switch
                {
                    "recent" => articles.OrderByDescending(a => a.PublishDate).ToList(),
                    "popular" => articles.OrderByDescending(a => a.LikesCount + a.ViewsCount).ToList(),
                    "trending" => articles.OrderByDescending(a => CalculateTrendingScore(a)).ToList(),
                    _ => articles // Keep recommended order
                };

                // Apply pagination
                var paginatedArticles = articles
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var response = new FeedResponse
                {
                    Articles = paginatedArticles.Select(a => new RecommendedArticle
                    {
                        Article = a,
                        RecommendationScore = CalculateDisplayScore(a, Enum.Parse<SortBy>(sortBy, true)),
                        RecommendationReason = $"From {category} category",
                        RecommendationFactors = new List<string> { $"Category: {category}" },
                        GeneratedAt = DateTime.UtcNow
                    }).ToList(),
                    TotalCount = articles.Count,
                    PageSize = pageSize,
                    PageNumber = pageNumber,
                    Algorithm = $"category_{category}_{sortBy}",
                    GeneratedAt = DateTime.UtcNow,
                    AppliedFilters = new List<string> { $"Category: {category}", $"Sort: {sortBy}" }
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"Failed to get category feed for {category}", details = ex.Message });
            }
        }

        // Helper methods
        
        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : null;
        }

        private List<NewsArticle> ApplyFilters(List<NewsArticle> articles, FeedSortOptions sortOptions)
        {
            var filteredArticles = articles.AsQueryable();

            // Time filter
            var cutoffDate = sortOptions.TimeFilter switch
            {
                TimeFilter.LastHour => DateTime.UtcNow.AddHours(-1),
                TimeFilter.Last24Hours => DateTime.UtcNow.AddDays(-1),
                TimeFilter.LastWeek => DateTime.UtcNow.AddDays(-7),
                TimeFilter.LastMonth => DateTime.UtcNow.AddDays(-30),
                _ => DateTime.MinValue
            };

            if (cutoffDate > DateTime.MinValue)
            {
                filteredArticles = filteredArticles.Where(a => a.PublishDate >= cutoffDate);
            }

            // Category filter
            if (!string.IsNullOrEmpty(sortOptions.CategoryFilter))
            {
                filteredArticles = filteredArticles.Where(a => 
                    a.Category != null && a.Category.Equals(sortOptions.CategoryFilter, StringComparison.OrdinalIgnoreCase));
            }

            // Source filter
            if (!string.IsNullOrEmpty(sortOptions.SourceFilter))
            {
                filteredArticles = filteredArticles.Where(a => 
                    a.SourceName != null && a.SourceName.Equals(sortOptions.SourceFilter, StringComparison.OrdinalIgnoreCase));
            }

            return filteredArticles.ToList();
        }

        private List<NewsArticle> ApplySorting(List<NewsArticle> articles, FeedSortOptions sortOptions)
        {
            var sortedArticles = sortOptions.SortBy switch
            {
                SortBy.Recent => articles.OrderByDescending(a => a.PublishDate),
                SortBy.Popular => articles.OrderByDescending(a => a.LikesCount + a.ViewsCount),
                SortBy.MostLiked => articles.OrderByDescending(a => a.LikesCount),
                SortBy.MostViewed => articles.OrderByDescending(a => a.ViewsCount),
                SortBy.Trending => articles.OrderByDescending(a => CalculateTrendingScore(a)),
                _ => articles.OrderByDescending(a => CalculateDisplayScore(a, sortOptions.SortBy))
            };

            return sortOptions.Order == SortOrder.Ascending 
                ? sortedArticles.Reverse().ToList() 
                : sortedArticles.ToList();
        }

        private double CalculateDisplayScore(NewsArticle article, SortBy sortBy)
        {
            return sortBy switch
            {
                SortBy.Popular => (article.LikesCount + article.ViewsCount) / 100.0,
                SortBy.MostLiked => article.LikesCount / 50.0,
                SortBy.MostViewed => article.ViewsCount / 500.0,
                SortBy.Recent => 1.0 - (DateTime.UtcNow - article.PublishDate).TotalDays / 7.0,
                SortBy.Trending => CalculateTrendingScore(article),
                _ => 0.5
            };
        }

        private double CalculateTrendingScore(NewsArticle article)
        {
            var ageInHours = (DateTime.UtcNow - article.PublishDate).TotalHours;
            var recencyScore = Math.Max(0, 1.0 - (ageInHours / 24.0));
            var engagementScore = (article.LikesCount * 2 + article.ViewsCount) / 100.0;
            return (recencyScore * 0.6) + (Math.Min(engagementScore, 1.0) * 0.4);
        }

        private string GetSortingReason(SortBy sortBy)
        {
            return sortBy switch
            {
                SortBy.Popular => "Popular among all users",
                SortBy.Trending => "Currently trending",
                SortBy.Recent => "Recently published",
                SortBy.MostLiked => "Most liked articles",
                SortBy.MostViewed => "Most viewed articles",
                _ => "Recommended for you"
            };
        }

        private List<string> BuildFilterDescription(FeedSortOptions sortOptions)
        {
            var filters = new List<string>();
            
            if (sortOptions.TimeFilter != TimeFilter.All)
                filters.Add($"Time: {sortOptions.TimeFilter}");
            if (!string.IsNullOrEmpty(sortOptions.CategoryFilter))
                filters.Add($"Category: {sortOptions.CategoryFilter}");
            if (!string.IsNullOrEmpty(sortOptions.SourceFilter))
                filters.Add($"Source: {sortOptions.SourceFilter}");
            if (sortOptions.OnlyFollowed)
                filters.Add("Only followed users");
                
            return filters;
        }

        private List<RecommendedArticle> ApplyIntelligentMixing(List<RecommendedArticle> articles, MixedFeedRequest request)
        {
            var mixed = new List<RecommendedArticle>();
            var personalized = articles.Where(a => a.IsPersonalized).ToList();
            var trending = articles.Where(a => a.IsTrending).ToList();
            var popular = articles.Where(a => a.PopularityScore > 0.5 && !a.IsTrending).ToList();
            var recent = articles.Where(a => !a.IsPersonalized && !a.IsTrending && a.PopularityScore <= 0.5).ToList();

            // Interleave different types of articles
            var maxLength = Math.Max(Math.Max(personalized.Count, trending.Count), Math.Max(popular.Count, recent.Count));
            
            for (int i = 0; i < maxLength; i++)
            {
                if (i < personalized.Count && request.IncludePersonalized) mixed.Add(personalized[i]);
                if (i < trending.Count && request.IncludeTrending) mixed.Add(trending[i]);
                if (i < popular.Count && request.IncludePopular) mixed.Add(popular[i]);
                if (i < recent.Count && request.IncludeRecent) mixed.Add(recent[i]);
            }

            return mixed.GroupBy(a => a.Article.ArticleID).Select(g => g.First()).ToList();
        }
    }

    // Request models for advanced feed operations
    
    public class SortedFeedRequest
    {
        public int PageSize { get; set; } = 20;
        public int PageNumber { get; set; } = 1;
        public FeedSortOptions SortOptions { get; set; } = new FeedSortOptions();
    }

    public class MixedFeedRequest
    {
        public int PageSize { get; set; } = 20;
        public int PageNumber { get; set; } = 1;
        public bool IncludePersonalized { get; set; } = true;
        public bool IncludeTrending { get; set; } = true;
        public bool IncludePopular { get; set; } = true;
        public bool IncludeRecent { get; set; } = true;
        public int PersonalizedCount { get; set; } = 8;
        public int TrendingCount { get; set; } = 6;
        public int PopularCount { get; set; } = 4;
        public int RecentCount { get; set; } = 2;
    }
}
