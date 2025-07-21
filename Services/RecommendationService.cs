using Microsoft.AspNetCore.Mvc;
using NewsSite.BL;

namespace NewsSite.Services
{
    public interface IRecommendationService
    {
        Task<FeedResponse> GetPersonalizedFeedAsync(int userId, FeedRequest request);
        Task<List<TrendingTopic>> GetTrendingTopicsAsync(int count = 10);
        Task<FeedResponse> GetFeedByAlgorithmAsync(int userId, string algorithm, FeedRequest request);
        Task UpdateUserInterestsAsync(int userId, int articleId, string interactionType);
        Task<PersonalizationInsights> GetUserPersonalizationInsightsAsync(int userId);
        Task<FeedConfiguration> GetUserFeedConfigurationAsync(int userId);
        Task<bool> UpdateUserFeedConfigurationAsync(int userId, FeedConfiguration configuration);
        Task RecordInteractionAsync(int userId, InteractionRequest request);
        Task<List<RecommendedArticle>> GetSimilarArticlesAsync(int articleId, int userId, int count = 10);
        Task<UserEngagementAnalytics> GetUserAnalyticsAsync(int userId, DateTime? fromDate = null);
        Task<bool> ResetUserPersonalizationAsync(int userId);
        Task<List<NewsArticle>> GetSerendipityFeedAsync(int userId, int count = 10);
        Task RefreshTrendingTopicsAsync();
        Task<List<NewsArticle>> GetFollowingFeedAsync(int userId, int count = 20);
    }

    public class RecommendationService : IRecommendationService
    {
        private readonly DBservices _dbServices;
        
        // Algorithm weights for scoring
        private const double INTEREST_WEIGHT = 0.4;
        private const double FRESHNESS_WEIGHT = 0.3;
        private const double POPULARITY_WEIGHT = 0.2;
        private const double SERENDIPITY_WEIGHT = 0.1;

        public RecommendationService(DBservices dbServices)
        {
            _dbServices = dbServices;
        }

        public async Task<FeedResponse> GetPersonalizedFeedAsync(int userId, FeedRequest request)
        {
            var response = new FeedResponse
            {
                PageSize = request.PageSize,
                PageNumber = request.PageNumber,
                Algorithm = request.Algorithm ?? "personalized",
                GeneratedAt = DateTime.UtcNow
            };

            try
            {
                // Get user configuration
                var config = await GetUserFeedConfigurationAsync(userId);
                
                // Get user interests and behavior
                var userInterests = await _dbServices.GetUserInterestsAsync(userId);
                var userBehavior = await _dbServices.GetUserBehaviorAsync(userId);
                
                // Get candidate articles from different sources
                var candidateArticles = new List<NewsArticle>();
                
                // 1. Interest-based articles (40% weight)
                if (userInterests.Any())
                {
                    foreach (var interest in userInterests.Take(3))
                    {
                        var articles = await _dbServices.GetArticlesByInterestAsync(userId, interest.Category, 10);
                        candidateArticles.AddRange(articles);
                    }
                }
                
                // 2. Trending articles (30% weight)
                var trendingArticles = await GetTrendingArticlesBasedOnInteractions(10);
                candidateArticles.AddRange(trendingArticles);
                
                // 3. Popular articles (20% weight)
                var popularArticles = await _dbServices.GetRecommendedArticlesAsync(userId, 10);
                candidateArticles.AddRange(popularArticles);

                // 4. Serendipity articles (10% weight) - introduce some randomness
                if (config.EnableSerendipity)
                {
                    var serendipityArticles = await GetSerendipityFeedAsync(userId, 5);
                    candidateArticles.AddRange(serendipityArticles);
                }

                // Remove duplicates
                candidateArticles = candidateArticles
                    .GroupBy(a => a.ArticleID)
                    .Select(g => g.First())
                    .ToList();

                // Score and rank articles using ML-style algorithm
                var scoredArticles = new List<RecommendedArticle>();
                foreach (var article in candidateArticles)
                {
                    var score = await CalculateAdvancedRecommendationScore(article, userInterests, userBehavior, config);
                    var recommendedArticle = new RecommendedArticle
                    {
                        Article = article,
                        RecommendationScore = score.Score,
                        RecommendationReason = score.Reason,
                        RecommendationFactors = score.Factors,
                        GeneratedAt = DateTime.UtcNow,
                        PersonalizationScore = score.PersonalizationScore,
                        FreshnessScore = score.FreshnessScore,
                        PopularityScore = score.PopularityScore,
                        IsPersonalized = score.PersonalizationScore > 0.5,
                        IsTrending = score.PopularityScore > 0.7
                    };
                    scoredArticles.Add(recommendedArticle);
                }

                // Advanced sorting with diversity injection
                var sortedArticles = ApplyDiversityAndSort(scoredArticles, config)
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .ToList();

                response.Articles = sortedArticles;
                response.TotalCount = candidateArticles.Count;

                // Get trending topics for the sidebar
                response.TrendingTopics = await GetTrendingTopicsAsync(5);
                
                // Add user analytics
                response.UserAnalytics = await GetUserAnalyticsAsync(userId);
                response.AppliedFilters = BuildAppliedFiltersList(request, config);
            }
            catch (Exception ex)
            {
                // Log error and return fallback feed
                Console.WriteLine($"Error generating personalized feed: {ex.Message}");
                response.Articles = await GetFallbackFeed(userId, request.PageSize);
            }

            return response;
        }

        public async Task<List<TrendingTopic>> GetTrendingTopicsAsync(int count = 10)
        {
            return await _dbServices.GetTrendingTopicsAsync(count);
        }

        public async Task<FeedResponse> GetFeedByAlgorithmAsync(int userId, string algorithm, FeedRequest request)
        {
            return algorithm.ToLower() switch
            {
                "trending" => await GetTrendingFeedAsync(userId, request),
                "popular" => await GetPopularFeedAsync(userId, request),
                "recent" => await GetRecentFeedAsync(userId, request),
                "following" => await GetFollowingFeedAsync(userId, request),
                "personalized" => await GetPersonalizedFeedAsync(userId, request),
                _ => await GetPersonalizedFeedAsync(userId, request)
            };
        }

        public async Task UpdateUserInterestsAsync(int userId, int articleId, string interactionType)
        {
            try
            {
                // Record the interaction
                await _dbServices.RecordUserInteractionAsync(userId, articleId, interactionType);

                // Get article for category analysis
                var article = await _dbServices.GetNewsArticleById(articleId);
                if (article == null) return;

                // Update interest score based on interaction type
                var interestBoost = GetInteractionWeight(interactionType);
                
                if (!string.IsNullOrEmpty(article.Category))
                {
                    await _dbServices.UpdateUserInterestAsync(userId, article.Category, interestBoost);
                }

                // Update user behavior analytics
                await UpdateUserBehaviorMetrics(userId, interactionType);
                
                // Update ML features for future recommendations
                await UpdateMLFeatures(userId, articleId, interactionType);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating user interests: {ex.Message}");
            }
        }

        public async Task<PersonalizationInsights> GetUserPersonalizationInsightsAsync(int userId)
        {
            var insights = new PersonalizationInsights { UserID = userId };
            
            try
            {
                // Get user interests
                var interests = await _dbServices.GetUserInterestsAsync(userId);
                insights.CategoryAffinities = interests.ToDictionary(i => i.Category, i => i.InterestScore);
                
                // Get behavior data
                var behavior = await _dbServices.GetUserBehaviorAsync(userId);
                if (behavior != null)
                {
                    insights.RecentInterests = behavior.FavoriteCategories.Take(5).ToList();
                    insights.DiversityPreference = CalculateDiversityPreference(behavior);
                }
                
                // Analyze time-based preferences
                insights.TimeSlotPreferences = await AnalyzeTimePreferences(userId);
                
                // Detect emerging interests (categories with recent growth)
                insights.EmergingInterests = await DetectEmergingInterests(userId);
                
                insights.LastUpdated = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting personalization insights: {ex.Message}");
            }
            
            return insights;
        }

        public async Task<FeedConfiguration> GetUserFeedConfigurationAsync(int userId)
        {
            var config = await _dbServices.GetUserFeedConfigurationAsync(userId);
            if (config == null)
            {
                // Create default configuration with smart defaults
                config = new FeedConfiguration 
                { 
                    UserID = userId,
                    Algorithm = FeedAlgorithm.Balanced,
                    PersonalizationWeight = 0.6,
                    FreshnessWeight = 0.3,
                    PopularityWeight = 0.1,
                    EnableSerendipity = true,
                    SerendipityWeight = 0.1,
                    MaxArticlesPerFeed = 20
                };
                await _dbServices.UpdateUserFeedConfigurationAsync(config);
            }
            return config;
        }

        public async Task<bool> UpdateUserFeedConfigurationAsync(int userId, FeedConfiguration configuration)
        {
            configuration.UserID = userId;
            configuration.LastUpdated = DateTime.UtcNow;
            return await _dbServices.UpdateUserFeedConfigurationAsync(configuration);
        }

        public async Task RecordInteractionAsync(int userId, InteractionRequest request)
        {
            await _dbServices.RecordUserInteractionAsync(userId, request.ArticleID, request.InteractionType);
            
            // Advanced interaction tracking
            if (request.TimeSpentSeconds.HasValue || request.ReadingProgress.HasValue)
            {
                await RecordAdvancedInteractionMetrics(userId, request);
            }
            
            // Update interests based on this interaction
            await UpdateUserInterestsAsync(userId, request.ArticleID, request.InteractionType);
        }

        public async Task<List<RecommendedArticle>> GetSimilarArticlesAsync(int articleId, int userId, int count = 10)
        {
            var similarArticles = await _dbServices.GetSimilarArticlesAsync(articleId, count);
            var recommendedArticles = new List<RecommendedArticle>();

            var userInterests = await _dbServices.GetUserInterestsAsync(userId);
            
            foreach (var article in similarArticles)
            {
                var score = CalculateSimilarityBasedScore(article, userInterests);
                recommendedArticles.Add(new RecommendedArticle
                {
                    Article = article,
                    RecommendationScore = score,
                    RecommendationReason = "Similar to articles you've engaged with",
                    RecommendationFactors = new List<string> { "Content similarity", "Category match" },
                    GeneratedAt = DateTime.UtcNow
                });
            }

            return recommendedArticles.OrderByDescending(a => a.RecommendationScore).ToList();
        }

        public async Task<UserEngagementAnalytics> GetUserAnalyticsAsync(int userId, DateTime? fromDate = null)
        {
            var analytics = new UserEngagementAnalytics
            {
                UserID = userId,
                AnalyticsDate = DateTime.UtcNow
            };

            try
            {
                // Get user interaction history
                var interactions = await _dbServices.GetUserInteractionHistoryAsync(userId, 30);
                
                // Calculate engagement metrics
                analytics.TotalViews = interactions.Count(i => i.InteractionType == "view");
                analytics.TotalLikes = interactions.Count(i => i.InteractionType == "like");
                analytics.TotalShares = interactions.Count(i => i.InteractionType == "share");
                analytics.TotalComments = interactions.Count(i => i.InteractionType == "comment");
                analytics.TotalSaves = interactions.Count(i => i.InteractionType == "save");
                
                // Calculate engagement score
                analytics.EngagementScore = CalculateEngagementScore(analytics);
                
                // Get behavior data
                var behavior = await _dbServices.GetUserBehaviorAsync(userId);
                if (behavior != null)
                {
                    analytics.AverageSessionTime = behavior.AvgSessionDuration;
                    analytics.TopCategories = string.Join(",", behavior.FavoriteCategories.Take(5));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting user analytics: {ex.Message}");
            }

            return analytics;
        }

        public async Task<bool> ResetUserPersonalizationAsync(int userId)
        {
            try
            {
                await _dbServices.ResetUserInterestsAsync(userId);
                
                // Reset to default configuration
                var defaultConfig = new FeedConfiguration 
                { 
                    UserID = userId,
                    Algorithm = FeedAlgorithm.Balanced,
                    PersonalizationWeight = 0.4,
                    FreshnessWeight = 0.3,
                    PopularityWeight = 0.2,
                    SerendipityWeight = 0.1
                };
                
                await _dbServices.UpdateUserFeedConfigurationAsync(defaultConfig);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<NewsArticle>> GetSerendipityFeedAsync(int userId, int count = 10)
        {
            // Get articles from categories the user hasn't explored much
            var userInterests = await _dbServices.GetUserInterestsAsync(userId);
            var exploredCategories = userInterests.Select(i => i.Category).ToHashSet();
            
            // Get random quality articles from unexplored categories
            var serendipityArticles = await _dbServices.GetRandomQualityArticlesAsync(count * 2);
            
            return serendipityArticles
                .Where(a => !exploredCategories.Contains(a.Category ?? ""))
                .Take(count)
                .ToList();
        }

        public async Task RefreshTrendingTopicsAsync()
        {
            try
            {
                var trendingTopics = await _dbServices.GetTrendingTopicsAsync(20);
                
                // Enhanced trending calculation
                var enhancedTopics = new List<TrendingTopic>();
                foreach (var topic in trendingTopics)
                {
                    // Recalculate trend score with time decay
                    var enhancedScore = CalculateTimeSensitiveTrendScore(topic);
                    topic.TrendScore = enhancedScore;
                    enhancedTopics.Add(topic);
                }
                
                // Save updated trending topics
                await _dbServices.SaveTrendingTopicsAsync(enhancedTopics);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error refreshing trending topics: {ex.Message}");
            }
        }

        public async Task<List<NewsArticle>> GetFollowingFeedAsync(int userId, int count = 20)
        {
            var followedUserIds = await _dbServices.GetFollowedUserIdsAsync(userId);
            return await _dbServices.GetFollowingFeedAsync(userId, count);
        }

        // Private helper methods
        
        private async Task<List<NewsArticle>> GetTrendingArticlesBasedOnInteractions(int count)
        {
            return await _dbServices.GetRecentHighEngagementArticlesAsync(count);
        }

        private async Task<RecommendationScore> CalculateAdvancedRecommendationScore(
            NewsArticle article, 
            List<UserInterest> userInterests, 
            UserBehavior? userBehavior, 
            FeedConfiguration config)
        {
            var score = new RecommendationScore();
            var factors = new List<string>();

            // 1. Personalization Score (based on user interests)
            var personalScore = 0.0;
            var matchingInterest = userInterests.FirstOrDefault(i => i.Category == article.Category);
            if (matchingInterest != null)
            {
                personalScore = matchingInterest.InterestScore;
                factors.Add($"High interest in {article.Category}");
            }
            score.PersonalizationScore = personalScore;

            // 2. Freshness Score (time-based decay)
            var ageInHours = (DateTime.UtcNow - article.PublishDate).TotalHours;
            var freshnessScore = Math.Max(0, 1.0 - (ageInHours / 168.0)); // Decay over week
            score.FreshnessScore = freshnessScore;
            if (freshnessScore > 0.8) factors.Add("Very recent article");

            // 3. Popularity Score (engagement metrics)
            var popularityScore = CalculatePopularityScore(article);
            score.PopularityScore = popularityScore;
            if (popularityScore > 0.7) factors.Add("Highly engaged content");

            // 4. Serendipity Score (diversity injection)
            var serendipityScore = config.EnableSerendipity ? CalculateSerendipityScore(article, userInterests) : 0;
            score.SerendipityScore = serendipityScore;
            if (serendipityScore > 0.5) factors.Add("New topic exploration");

            // 5. Time-based preferences
            if (userBehavior != null)
            {
                var timePreferenceScore = CalculateTimePreferenceScore(userBehavior);
                personalScore *= (1 + timePreferenceScore * 0.2); // Boost up to 20%
                if (timePreferenceScore > 0.5) factors.Add("Posted at your preferred time");
            }

            // Weighted combination
            var finalScore = 
                (personalScore * config.PersonalizationWeight) +
                (freshnessScore * config.FreshnessWeight) +
                (popularityScore * config.PopularityWeight) +
                (serendipityScore * config.SerendipityWeight);

            score.Score = Math.Min(finalScore, 1.0);
            score.Factors = factors;
            score.Reason = GenerateRecommendationReason(score, factors);

            return score;
        }

        private double CalculatePopularityScore(NewsArticle article)
        {
            // Normalize engagement metrics
            var likesNorm = Math.Min(article.LikesCount / 100.0, 1.0);
            var viewsNorm = Math.Min(article.ViewsCount / 1000.0, 1.0);
            
            return (likesNorm * 0.6) + (viewsNorm * 0.4);
        }

        private double CalculateSerendipityScore(NewsArticle article, List<UserInterest> userInterests)
        {
            // Higher score for articles from categories user hasn't explored
            var exploredCategories = userInterests.Select(i => i.Category).ToHashSet();
            return exploredCategories.Contains(article.Category ?? "") ? 0.2 : 0.8;
        }

        private double CalculateTimePreferenceScore(UserBehavior behavior)
        {
            // Simple time preference based on most active hour
            var currentHour = DateTime.UtcNow.Hour;
            var preferredHour = behavior.MostActiveHour;
            var hourDifference = Math.Abs(currentHour - preferredHour);
            
            return Math.Max(0, 1.0 - (hourDifference / 12.0));
        }

        private string GenerateRecommendationReason(RecommendationScore score, List<string> factors)
        {
            if (score.PersonalizationScore > 0.7)
                return "Matches your strong interests";
            if (score.PopularityScore > 0.8)
                return "Trending and highly engaging";
            if (score.FreshnessScore > 0.9)
                return "Breaking news in your area of interest";
            if (score.SerendipityScore > 0.5)
                return "Recommended to broaden your perspective";
                
            return "Recommended based on your reading patterns";
        }

        private List<RecommendedArticle> ApplyDiversityAndSort(List<RecommendedArticle> articles, FeedConfiguration config)
        {
            // Implement diversity injection to avoid filter bubbles
            var sortedArticles = articles.OrderByDescending(a => a.RecommendationScore).ToList();
            var diversifiedArticles = new List<RecommendedArticle>();
            var usedCategories = new HashSet<string>();

            foreach (var article in sortedArticles)
            {
                var category = article.Article.Category ?? "general";
                
                // Add article if it's high scoring or adds category diversity
                if (diversifiedArticles.Count < 3 || // Always include top 3
                    !usedCategories.Contains(category) || // New category
                    article.RecommendationScore > 0.8) // High score override
                {
                    diversifiedArticles.Add(article);
                    usedCategories.Add(category);
                }
                
                if (diversifiedArticles.Count >= config.MaxArticlesPerFeed * 2) break;
            }

            return diversifiedArticles;
        }

        private List<string> BuildAppliedFiltersList(FeedRequest request, FeedConfiguration config)
        {
            var filters = new List<string>();
            
            if (request.Categories?.Any() == true)
                filters.Add($"Categories: {string.Join(", ", request.Categories)}");
            if (request.FromDate.HasValue)
                filters.Add($"From: {request.FromDate.Value:MMM dd}");
            if (config.ExcludedCategories.Any())
                filters.Add($"Excluding: {string.Join(", ", config.ExcludedCategories)}");
                
            return filters;
        }

        private async Task<List<RecommendedArticle>> GetFallbackFeed(int userId, int pageSize)
        {
            // Simple fallback to recent articles
            var recentArticles = await _dbServices.GetRecentArticlesAsync(pageSize);
            return recentArticles.Select(a => new RecommendedArticle
            {
                Article = a,
                RecommendationScore = 0.5,
                RecommendationReason = "Recent articles",
                GeneratedAt = DateTime.UtcNow
            }).ToList();
        }

        // Additional algorithm implementations
        
        private async Task<FeedResponse> GetTrendingFeedAsync(int userId, FeedRequest request)
        {
            var response = new FeedResponse
            {
                Algorithm = "trending",
                GeneratedAt = DateTime.UtcNow,
                PageSize = request.PageSize,
                PageNumber = request.PageNumber
            };

            var articles = await _dbServices.GetTrendingArticlesAsync(request.PageSize);
            response.Articles = articles.Select(a => new RecommendedArticle
            {
                Article = a,
                RecommendationScore = CalculatePopularityScore(a),
                RecommendationReason = "Currently trending",
                IsTrending = true,
                GeneratedAt = DateTime.UtcNow
            }).ToList();

            response.TotalCount = response.Articles.Count;
            response.TrendingTopics = await GetTrendingTopicsAsync(5);
            
            return response;
        }

        private async Task<FeedResponse> GetPopularFeedAsync(int userId, FeedRequest request)
        {
            var response = new FeedResponse
            {
                Algorithm = "popular",
                GeneratedAt = DateTime.UtcNow,
                PageSize = request.PageSize,
                PageNumber = request.PageNumber
            };

            var popularArticles = await _dbServices.GetPopularArticlesAsync(request.PageSize);
            var trendingArticles = await _dbServices.GetTrendingArticlesAsync(request.PageSize);
            var mostLiked = await _dbServices.GetMostLikedArticlesAsync(request.PageSize);
            var mostViewed = await _dbServices.GetMostViewedArticlesAsync(request.PageSize);

            var allArticles = popularArticles
                .Concat(trendingArticles)
                .Concat(mostLiked)
                .Concat(mostViewed)
                .GroupBy(a => a.ArticleID)
                .Select(g => g.First())
                .OrderByDescending(a => CalculatePopularityScore(a))
                .Take(request.PageSize)
                .ToList();

            response.Articles = allArticles.Select(a => new RecommendedArticle
            {
                Article = a,
                RecommendationScore = CalculatePopularityScore(a),
                RecommendationReason = "Popular among all users",
                GeneratedAt = DateTime.UtcNow
            }).ToList();

            response.TotalCount = response.Articles.Count;
            
            return response;
        }

        private async Task<FeedResponse> GetRecentFeedAsync(int userId, FeedRequest request)
        {
            var response = new FeedResponse
            {
                Algorithm = "recent",
                GeneratedAt = DateTime.UtcNow,
                PageSize = request.PageSize,
                PageNumber = request.PageNumber
            };

            var recentArticles = await _dbServices.GetRecentArticlesAsync(request.PageSize);
            response.Articles = recentArticles.Select(a => new RecommendedArticle
            {
                Article = a,
                RecommendationScore = CalculateFreshnessScore(a),
                RecommendationReason = "Recently published",
                GeneratedAt = DateTime.UtcNow
            }).ToList();

            response.TotalCount = response.Articles.Count;
            
            return response;
        }

        private async Task<FeedResponse> GetFollowingFeedAsync(int userId, FeedRequest request)
        {
            var response = new FeedResponse
            {
                Algorithm = "following",
                GeneratedAt = DateTime.UtcNow,
                PageSize = request.PageSize,
                PageNumber = request.PageNumber
            };

            var followingArticles = await _dbServices.GetFollowingFeedAsync(userId, request.PageSize);
            response.Articles = followingArticles.Select(a => new RecommendedArticle
            {
                Article = a,
                RecommendationScore = 0.8, // High score for followed users
                RecommendationReason = "From users you follow",
                IsFromFollowedUser = true,
                GeneratedAt = DateTime.UtcNow
            }).ToList();

            response.TotalCount = response.Articles.Count;
            
            return response;
        }

        // Helper methods for ML and analytics
        
        private double GetInteractionWeight(string interactionType) => interactionType.ToLower() switch
        {
            "view" => 0.1,
            "like" => 0.3,
            "share" => 0.5,
            "comment" => 0.4,
            "save" => 0.6,
            "fullread" => 0.7,
            _ => 0.1
        };

        private async Task UpdateUserBehaviorMetrics(int userId, string interactionType)
        {
            var behavior = await _dbServices.GetUserBehaviorAsync(userId) ?? new UserBehavior { UserID = userId };
            
            // Update counters based on interaction type
            switch (interactionType.ToLower())
            {
                case "view": behavior.TotalViews++; break;
                case "like": behavior.TotalLikes++; break;
                case "share": behavior.TotalShares++; break;
                case "comment": behavior.TotalComments++; break;
            }
            
            behavior.LastActivity = DateTime.UtcNow;
            behavior.MostActiveHour = DateTime.UtcNow.Hour;
            
            await _dbServices.UpdateUserBehaviorAsync(behavior);
        }

        private async Task UpdateMLFeatures(int userId, int articleId, string interactionType)
        {
            // Placeholder for ML feature updates
            // In a real implementation, this would update feature vectors for ML models
        }

        private double CalculateDiversityPreference(UserBehavior behavior)
        {
            // Calculate how diverse the user's reading habits are
            return behavior.FavoriteCategories.Count / 10.0; // Normalize to 0-1
        }

        private async Task<Dictionary<int, double>> AnalyzeTimePreferences(int userId)
        {
            // Analyze user's reading patterns by hour
            var preferences = new Dictionary<int, double>();
            for (int hour = 0; hour < 24; hour++)
            {
                preferences[hour] = hour == 8 || hour == 12 || hour == 18 ? 0.8 : 0.2; // Mock data
            }
            return preferences;
        }

        private async Task<List<string>> DetectEmergingInterests(int userId)
        {
            // Detect categories with recent growth in user interactions
            return new List<string> { "AI", "Climate" }; // Mock data
        }

        private double CalculateSimilarityBasedScore(NewsArticle article, List<UserInterest> userInterests)
        {
            var baseScore = 0.5;
            var matchingInterest = userInterests.FirstOrDefault(i => i.Category == article.Category);
            return matchingInterest != null ? baseScore + (matchingInterest.InterestScore * 0.3) : baseScore;
        }

        private async Task RecordAdvancedInteractionMetrics(int userId, InteractionRequest request)
        {
            // Record detailed interaction metrics for ML
            // This could include reading progress, time spent, device type, etc.
        }

        private double CalculateEngagementScore(UserEngagementAnalytics analytics)
        {
            var totalInteractions = analytics.TotalLikes + analytics.TotalShares + analytics.TotalComments + analytics.TotalSaves;
            var engagementRate = analytics.TotalViews > 0 ? (double)totalInteractions / analytics.TotalViews : 0;
            return Math.Min(engagementRate * 10, 1.0); // Normalize to 0-1
        }

        private double CalculateTimeSensitiveTrendScore(TrendingTopic topic)
        {
            var ageInHours = (DateTime.UtcNow - topic.LastUpdated).TotalHours;
            var timeDecay = Math.Max(0.1, 1.0 - (ageInHours / 24.0)); // Decay over 24 hours
            return topic.TrendScore * timeDecay;
        }

        private double CalculateFreshnessScore(NewsArticle article)
        {
            var ageInHours = (DateTime.UtcNow - article.PublishDate).TotalHours;
            return Math.Max(0, 1.0 - (ageInHours / 168.0)); // Decay over week
        }
    }
}
