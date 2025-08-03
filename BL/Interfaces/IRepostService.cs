/**
 * IRepostService.cs
 * 
 * Interface for repost service functionality.
 * Defines contracts for repost creation, management, likes, comments, and notifications.
 * 
 * Author: System
 * Date: 2025-08-03
 */

namespace NewsSite.BL.Interfaces
{
    /// <summary>
    /// Interface for repost service operations
    /// </summary>
    public interface IRepostService
    {
        #region Repost Management
        
        /// <summary>
        /// Creates a new repost for an article
        /// </summary>
        /// <param name="request">Repost creation request</param>
        /// <returns>Result of the repost creation</returns>
        Task<RepostResult> CreateRepostAsync(CreateRepostRequest request);

        /// <summary>
        /// Gets reposts for a user's feed
        /// </summary>
        /// <param name="userId">ID of the user requesting the feed</param>
        /// <param name="pageNumber">Page number for pagination</param>
        /// <param name="pageSize">Number of reposts per page</param>
        /// <returns>List of reposts for the user's feed</returns>
        Task<List<Repost>> GetUserFeedRepostsAsync(int userId, int pageNumber = 1, int pageSize = 10);

        /// <summary>
        /// Gets reposts created by a specific user
        /// </summary>
        /// <param name="userId">ID of the user</param>
        /// <param name="pageNumber">Page number for pagination</param>
        /// <param name="pageSize">Number of reposts per page</param>
        /// <returns>List of reposts by the user</returns>
        Task<List<Repost>> GetUserRepostsAsync(int userId, int pageNumber = 1, int pageSize = 10);

        /// <summary>
        /// Deletes a repost (soft delete)
        /// </summary>
        /// <param name="repostId">ID of the repost to delete</param>
        /// <param name="userId">ID of the user requesting deletion</param>
        /// <returns>Result of the deletion operation</returns>
        Task<RepostResult> DeleteRepostAsync(int repostId, int userId);

        #endregion

        #region Repost Likes

        /// <summary>
        /// Toggles like on a repost
        /// </summary>
        /// <param name="repostId">ID of the repost</param>
        /// <param name="userId">ID of the user</param>
        /// <returns>Result of the like toggle operation</returns>
        Task<RepostResult> ToggleRepostLikeAsync(int repostId, int userId);

        /// <summary>
        /// Gets like count for a repost
        /// </summary>
        /// <param name="repostId">ID of the repost</param>
        /// <returns>Number of likes on the repost</returns>
        Task<int> GetRepostLikeCountAsync(int repostId);

        /// <summary>
        /// Checks if a user has liked a specific repost
        /// </summary>
        /// <param name="repostId">ID of the repost</param>
        /// <param name="userId">ID of the user</param>
        /// <returns>True if user has liked the repost</returns>
        Task<bool> HasUserLikedRepostAsync(int repostId, int userId);

        #endregion

        #region Repost Comments

        /// <summary>
        /// Creates a new comment on a repost
        /// </summary>
        /// <param name="request">Comment creation request</param>
        /// <returns>Result of the comment creation</returns>
        Task<RepostResult> CreateRepostCommentAsync(CreateRepostCommentRequest request);

        /// <summary>
        /// Gets comments for a specific repost
        /// </summary>
        /// <param name="repostId">ID of the repost</param>
        /// <returns>List of comments for the repost</returns>
        Task<List<RepostComment>> GetRepostCommentsAsync(int repostId);

        /// <summary>
        /// Gets comment count for a repost
        /// </summary>
        /// <param name="repostId">ID of the repost</param>
        /// <returns>Number of comments on the repost</returns>
        Task<int> GetRepostCommentCountAsync(int repostId);

        #endregion
    }
}
