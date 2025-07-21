using NewsSite.BL;

namespace NewsSite.BL.Services
{
    public interface ICommentService
    {
        Task<List<Comment>> GetCommentsByPostIdAsync(int postId);
        Task<bool> CreateCommentAsync(Comment comment);
        Task<bool> UpdateCommentAsync(int commentId, int userId, string content);
        Task<bool> DeleteCommentAsync(int commentId, int userId);
        Task<int> GetCommentsCountAsync(int postId);
        Task<string> ToggleCommentLikeAsync(int commentId, int userId);
    }
}
