using NewsSite.BL;

namespace NewsSite.BL.Services
{
    /// <summary>
    /// News Service - Business Logic Layer
    /// Implements news article-related business operations and validation
    /// Integrated with NotificationService for like and share notifications
    /// </summary>
    public class NewsService : INewsService
    {
        private readonly DBservices _dbService;
        private readonly NotificationService _notificationService;

        public NewsService(DBservices dbService, NotificationService notificationService)
        {
            _dbService = dbService;
            _notificationService = notificationService;
        }

        public async Task<List<NewsArticle>> GetAllNewsArticlesAsync(int pageNumber = 1, int pageSize = 10, string? category = null, int? currentUserId = null)
        {
            // Business logic validation
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 10; // Limit page size

            return await Task.FromResult(_dbService.GetAllNewsArticles(pageNumber, pageSize, category, currentUserId));
        }

        public async Task<NewsArticle?> GetNewsArticleByIdAsync(int articleId, int? currentUserId = null)
        {
            if (articleId <= 0)
            {
                throw new ArgumentException("Valid Article ID is required");
            }

            // Record view if user is provided
            if (currentUserId.HasValue)
            {
                await RecordArticleViewAsync(articleId, currentUserId);
            }

            // Use the single-parameter overload since the 2-parameter version has compilation issues
            return await _dbService.GetNewsArticleById(articleId);
        }

        public async Task<int> CreateNewsArticleAsync(NewsArticle article)
        {
            // Business logic validation
            if (string.IsNullOrWhiteSpace(article.Title))
            {
                throw new ArgumentException("Article title is required");
            }

            if (string.IsNullOrWhiteSpace(article.Content))
            {
                throw new ArgumentException("Article content is required");
            }

            if (string.IsNullOrWhiteSpace(article.Category))
            {
                throw new ArgumentException("Article category is required");
            }

            if (article.UserID <= 0)
            {
                throw new ArgumentException("Valid User ID is required");
            }

            // Validate content length
            if (article.Title.Length > 100)
            {
                throw new ArgumentException("Title cannot exceed 100 characters");
            }

            if (article.Content.Length > 4000)
            {
                throw new ArgumentException("Content cannot exceed 4000 characters");
            }

            var articleId = await Task.FromResult(_dbService.CreateNewsArticle(article));
            
            // Create new post notifications for followers
            if (articleId > 0)
            {
                try
                {
                    // Get author details
                    var author = _dbService.GetUserById(article.UserID);
                    if (author != null)
                    {
                        // Create notifications for all followers
                        await _notificationService.CreateNewPostNotificationsForFollowersAsync(
                            articleId,
                            article.UserID,
                            author.Name ?? "Unknown User",
                            article.Title
                        );
                    }
                }
                catch (Exception ex)
                {
                    // Log the notification error but don't fail the article creation
                    Console.WriteLine($"Failed to create new post notifications for followers: {ex.Message}");
                }
            }
            
            return articleId;
        }

        public async Task<bool> UpdateNewsArticleAsync(NewsArticle article)
        {
            // Business logic validation
            if (article.ArticleID <= 0)
            {
                throw new ArgumentException("Valid Article ID is required");
            }

            // Check if article exists
            var existingArticle = await GetNewsArticleByIdAsync(article.ArticleID);
            if (existingArticle == null)
            {
                throw new ArgumentException("Article not found");
            }

            // Business rule: Only the author or admin can update
            if (existingArticle.UserID != article.UserID)
            {
                throw new UnauthorizedAccessException("You can only update your own articles");
            }

            return await Task.FromResult(true); // Implement update logic in DBservices
        }

        public async Task<bool> DeleteNewsArticleAsync(int articleId)
        {
            return await _dbService.DeleteNewsArticle(articleId);
        }

        public async Task<List<NewsArticle>> GetArticlesByUserAsync(int userId, int pageNumber = 1, int pageSize = 10)
        {
            if (userId <= 0)
            {
                throw new ArgumentException("Valid User ID is required");
            }

            return await Task.FromResult(_dbService.GetArticlesByUser(userId, pageNumber, pageSize));
        }

        public async Task<List<NewsArticle>> SearchArticlesAsync(string searchTerm, string category = "", int pageNumber = 1, int pageSize = 10, int? currentUserId = null)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                throw new ArgumentException("Search term is required");
            }

            return await _dbService.SearchArticlesAsync(searchTerm, category, pageNumber, pageSize, currentUserId);
        }

        public async Task<string> ToggleArticleLikeAsync(int articleId, int userId)
        {
            Console.WriteLine($"[NewsService] ToggleArticleLikeAsync called - ArticleId: {articleId}, UserId: {userId}");
            
            if (articleId <= 0 || userId <= 0)
            {
                throw new ArgumentException("Valid Article ID and User ID are required");
            }

            var result = await Task.FromResult(_dbService.ToggleArticleLike(articleId, userId));
            Console.WriteLine($"[NewsService] ToggleArticleLike result: {result}");
            
            // Create like notification if article was liked (not unliked)
            if (result == "liked")
            {
                Console.WriteLine($"[NewsService] Article was liked, creating notification...");
                try
                {
                    // Get article details to find the article author
                    var article = await _dbService.GetNewsArticleById(articleId);
                    if (article != null)
                    {
                        Console.WriteLine($"[NewsService] Article found - AuthorId: {article.UserID}, Title: {article.Title}");
                        
                        // Get liker details
                        var liker = _dbService.GetUserById(userId);
                        if (liker != null)
                        {
                            Console.WriteLine($"[NewsService] Liker found - Name: {liker.Name}");
                            
                            // Create notification for article author
                            var notificationId = await _notificationService.CreateLikeNotificationAsync(
                                articleId,
                                userId,
                                article.UserID,
                                liker.Name ?? "Unknown User"
                            );
                            
                            Console.WriteLine($"[NewsService] Notification created with ID: {notificationId}");
                        }
                        else
                        {
                            Console.WriteLine($"[NewsService] Liker not found for UserId: {userId}");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"[NewsService] Article not found for ArticleId: {articleId}");
                    }
                }
                catch (Exception ex)
                {
                    // Log the notification error but don't fail the like action
                    Console.WriteLine($"[NewsService] Failed to create like notification: {ex.Message}");
                    Console.WriteLine($"[NewsService] Stack trace: {ex.StackTrace}");
                }
            }
            else
            {
                Console.WriteLine($"[NewsService] Article was not liked (result: {result}), skipping notification");
            }
            
            return result;
        }

        public async Task<string> ToggleSaveArticleAsync(int articleId, int userId)
        {
            if (articleId <= 0 || userId <= 0)
            {
                throw new ArgumentException("Valid Article ID and User ID are required");
            }

            return await Task.FromResult(_dbService.ToggleSaveArticle(articleId, userId));
        }

        public async Task<bool> RecordArticleViewAsync(int articleId, int? userId = null)
        {
            return await Task.FromResult(_dbService.RecordArticleView(articleId, userId));
        }

        public async Task<bool> ReportArticleAsync(int articleId, int userId, string? reason = null)
        {
            if (articleId <= 0 || userId <= 0)
            {
                throw new ArgumentException("Valid Article ID and User ID are required");
            }

            return await Task.FromResult(_dbService.ReportArticle(articleId, userId, reason));
        }

        public async Task<List<NewsArticle>> GetLikedArticlesByUserAsync(int userId, int pageNumber = 1, int pageSize = 10)
        {
            if (userId <= 0)
            {
                throw new ArgumentException("Valid User ID is required");
            }

            return await _dbService.GetLikedArticlesByUser(userId, pageNumber, pageSize);
        }

        public async Task<List<NewsArticle>> GetSavedArticlesByUserAsync(int userId, int pageNumber = 1, int pageSize = 10)
        {
            if (userId <= 0)
            {
                throw new ArgumentException("Valid User ID is required");
            }

            return await _dbService.GetSavedArticlesByUser(userId, pageNumber, pageSize);
        }

        // Feed algorithm methods
        public async Task<List<NewsArticle>> GetPopularArticlesAsync(int pageSize = 10)
        {
            if (pageSize < 1 || pageSize > 100) pageSize = 10;
            return await _dbService.GetPopularArticlesAsync(pageSize);
        }

        public async Task<List<NewsArticle>> GetTrendingArticlesAsync(int pageSize = 10)
        {
            if (pageSize < 1 || pageSize > 100) pageSize = 10;
            return await _dbService.GetTrendingArticlesAsync(pageSize);
        }

        public async Task<List<NewsArticle>> GetMostLikedArticlesAsync(int pageSize = 10)
        {
            if (pageSize < 1 || pageSize > 100) pageSize = 10;
            return await _dbService.GetMostLikedArticlesAsync(pageSize);
        }

        public async Task<List<NewsArticle>> GetMostViewedArticlesAsync(int pageSize = 10)
        {
            if (pageSize < 1 || pageSize > 100) pageSize = 10;
            return await _dbService.GetMostViewedArticlesAsync(pageSize);
        }

        public async Task<List<NewsArticle>> GetRecentArticlesAsync(int pageSize = 10)
        {
            if (pageSize < 1 || pageSize > 100) pageSize = 10;
            return await _dbService.GetRecentArticlesAsync(pageSize);
        }

        public async Task<List<NewsArticle>> GetArticlesByInterestAsync(int userId, string category, int pageSize = 10)
        {
            if (userId <= 0)
            {
                throw new ArgumentException("Valid User ID is required");
            }
            if (pageSize < 1 || pageSize > 100) pageSize = 10;
            
            return await _dbService.GetArticlesByInterestAsync(userId, category, pageSize);
        }
    }
}
