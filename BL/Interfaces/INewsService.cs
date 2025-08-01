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
        
        // Feed algorithm methods
        Task<List<NewsArticle>> GetPopularArticlesAsync(int pageSize = 10);
        Task<List<NewsArticle>> GetTrendingArticlesAsync(int pageSize = 10);
        Task<List<NewsArticle>> GetMostLikedArticlesAsync(int pageSize = 10);
        Task<List<NewsArticle>> GetMostViewedArticlesAsync(int pageSize = 10);
        Task<List<NewsArticle>> GetRecentArticlesAsync(int pageSize = 10);
        Task<List<NewsArticle>> GetArticlesByInterestAsync(int userId, string category, int pageSize = 10);
    }
}
