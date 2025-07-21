using NewsSite.BL;

namespace NewsSite.BL.Services
{
    /// <summary>
    /// News Service - Business Logic Layer
    /// Implements news article-related business operations and validation
    /// </summary>
    public class NewsService : INewsService
    {
        private readonly DBservices _dbService;

        public NewsService(DBservices dbService)
        {
            _dbService = dbService;
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

            return await Task.FromResult(_dbService.CreateNewsArticle(article));
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
            if (articleId <= 0 || userId <= 0)
            {
                throw new ArgumentException("Valid Article ID and User ID are required");
            }

            return await Task.FromResult(_dbService.ToggleArticleLike(articleId, userId));
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
    }
}
