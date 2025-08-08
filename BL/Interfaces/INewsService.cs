// ----------------------------------------------------------------------------------
// INewsService.cs
//
// This interface defines the contract for all news-related operations in the business logic layer.
// It centralizes the methods for creating, retrieving, updating, and deleting news articles, as well as
// user interactions like likes, saves, views, and reports. By using an interface, the codebase supports
// dependency injection, testability, and separation of concerns, allowing different implementations for
// news services if needed.
//
// All methods are asynchronous and return Task or Task<T> to ensure non-blocking I/O operations, scalability,
// and responsiveness. This is especially important for web applications where database and network calls
// should not block the main thread. Using async/Task enables efficient resource usage and better user experience.
// ----------------------------------------------------------------------------------
using NewsSite.BL;

namespace NewsSite.BL.Services
{
    public interface INewsService
    {
        Task<List<NewsArticle>> GetAllNewsArticlesAsync(int pageNumber = 1, int pageSize = 10, string? category = null, int? currentUserId = null);
        Task<NewsArticle?> GetNewsArticleByIdAsync(int articleId, int? currentUserId = null);
        Task<int> CreateNewsArticleAsync(NewsArticle article);
        Task<bool> UpdateNewsArticleAsync(NewsArticle article);
        Task<bool> DeleteNewsArticleAsync(int articleId);
        Task<List<NewsArticle>> GetArticlesByUserAsync(int userId, int pageNumber = 1, int pageSize = 10);
        Task<List<NewsArticle>> SearchArticlesAsync(string searchTerm, string category = "", int pageNumber = 1, int pageSize = 10, int? currentUserId = null);
        Task<string> ToggleArticleLikeAsync(int articleId, int userId);
        Task<string> ToggleSaveArticleAsync(int articleId, int userId);
        Task<bool> RecordArticleViewAsync(int articleId, int? userId = null);
        Task<bool> ReportArticleAsync(int articleId, int userId, string? reason = null);
        Task<List<NewsArticle>> GetLikedArticlesByUserAsync(int userId, int pageNumber = 1, int pageSize = 10);
        Task<List<NewsArticle>> GetSavedArticlesByUserAsync(int userId, int pageNumber = 1, int pageSize = 10);
        Task<List<NewsArticle>> GetSavedArticlesWithFiltersAsync(int userId, int pageNumber = 1, int pageSize = 10, string? category = null, string? search = null);
        Task<List<NewsArticle>> GetFollowingPostsAsync(int userId, int pageNumber = 1, int pageSize = 10, string? category = null);
        
        // Feed algorithm methods
        Task<List<NewsArticle>> GetPopularArticlesAsync(int pageSize = 10);
        Task<List<NewsArticle>> GetTrendingArticlesAsync(int pageSize = 10, string? category = null, int? currentUserId = null);
        Task<List<NewsArticle>> GetMostLikedArticlesAsync(int pageSize = 10);
        Task<List<NewsArticle>> GetMostViewedArticlesAsync(int pageSize = 10);
        Task<List<NewsArticle>> GetRecentArticlesAsync(int pageSize = 10);
        Task<List<NewsArticle>> GetArticlesByInterestAsync(int userId, string category, int pageSize = 10);
    }
}
