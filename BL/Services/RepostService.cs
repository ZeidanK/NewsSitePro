/**
 * RepostService.cs
 * 
 * Business Logic service for repost functionality.
 * Handles repost creation, management, likes, comments, and notifications.
 * Provides small, reusable methods for scalability and maintains clean architecture.
 * 
 * Author: System
 * Date: 2025-08-03
 */

using NewsSite.BL;
using NewsSite.BL.Interfaces;

namespace NewsSite.BL.Services
{
    /// <summary>
    /// Service for managing reposts, repost likes, and repost comments
    /// </summary>
    public class RepostService : IRepostService
    {
        private readonly DBservices _dbServices;
        private readonly NotificationService _notificationService;

        public RepostService(DBservices dbServices, NotificationService notificationService)
        {
            _dbServices = dbServices;
            _notificationService = notificationService;
        }

        /// <summary>
        /// Creates a new repost and sends notification to original author
        /// </summary>
        /// <param name="request">Repost creation request</param>
        /// <returns>Result of the repost creation</returns>
        public async Task<RepostResult> CreateRepostAsync(CreateRepostRequest request)
        {
            try
            {
                // Validate request
                if (request.OriginalArticleID <= 0)
                    throw new ArgumentException("Valid article ID is required");

                if (request.UserID <= 0)
                    throw new ArgumentException("Valid user ID is required");

                // Create the repost
                int repostId = await _dbServices.CreateRepostAsync(request);
                
                if (repostId > 0)
                {
                    // Get article details for notification
                    var article = await _dbServices.GetNewsArticleById(request.OriginalArticleID);
                    var user = _dbServices.GetUserById(request.UserID);
                    
                    if (article != null && user != null)
                    {
                        // Create notification for original author
                        await _notificationService.CreateRepostNotificationAsync(
                            repostId,
                            request.UserID,
                            article.UserID,
                            user.Name ?? "Unknown User",
                            article.Title ?? "Article"
                        );
                    }

                    return new RepostResult 
                    { 
                        Success = true, 
                        Message = "Repost created successfully",
                        RepostID = repostId
                    };
                }

                return new RepostResult 
                { 
                    Success = false, 
                    Message = "Failed to create repost"
                };
            }
            catch (Exception ex)
            {
                return new RepostResult 
                { 
                    Success = false, 
                    Message = ex.Message
                };
            }
        }

        /// <summary>
        /// Gets reposts for a user's feed
        /// </summary>
        /// <param name="userId">ID of the user</param>
        /// <param name="pageNumber">Page number</param>
        /// <param name="pageSize">Number of reposts per page</param>
        /// <returns>List of reposts</returns>
        public async Task<List<Repost>> GetUserFeedRepostsAsync(int userId, int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                return await _dbServices.GetUserFeedRepostsAsync(userId, pageNumber, pageSize);
            }
            catch (Exception)
            {
                return new List<Repost>();
            }
        }

        /// <summary>
        /// Gets reposts created by a specific user
        /// </summary>
        /// <param name="userId">ID of the user</param>
        /// <param name="pageNumber">Page number</param>
        /// <param name="pageSize">Number of reposts per page</param>
        /// <returns>List of user's reposts</returns>
        public async Task<List<Repost>> GetUserRepostsAsync(int userId, int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                return await _dbServices.GetUserRepostsAsync(userId, pageNumber, pageSize);
            }
            catch (Exception)
            {
                return new List<Repost>();
            }
        }

        /// <summary>
        /// Gets a specific repost by ID
        /// </summary>
        /// <param name="repostId">ID of the repost</param>
        /// <returns>Repost object or null if not found</returns>
        public async Task<Repost?> GetRepostByIdAsync(int repostId)
        {
            try
            {
                return await _dbServices.GetRepostByIdAsync(repostId);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Deletes a repost (soft delete)
        /// </summary>
        /// <param name="repostId">ID of the repost to delete</param>
        /// <param name="userId">ID of the user requesting deletion</param>
        /// <returns>Result of the deletion operation</returns>
        public async Task<RepostResult> DeleteRepostAsync(int repostId, int userId)
        {
            try
            {
                var success = await _dbServices.DeleteRepostAsync(repostId, userId);
                
                return new RepostResult
                {
                    Success = success,
                    Message = success ? "Repost deleted successfully" : "Failed to delete repost"
                };
            }
            catch (Exception ex)
            {
                return new RepostResult
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }

        /// <summary>
        /// Toggles like on a repost and creates notification if liked
        /// </summary>
        /// <param name="repostId">ID of the repost</param>
        /// <param name="userId">ID of the user</param>
        /// <returns>Result of the like operation</returns>
        public async Task<RepostResult> ToggleRepostLikeAsync(int repostId, int userId)
        {
            try
            {
                var result = await _dbServices.ToggleRepostLikeAsync(repostId, userId);
                
                // Create notification if repost was liked
                if (result.Success && result.IsLiked == true)
                {
                    var repost = await GetRepostByIdAsync(repostId);
                    var user = _dbServices.GetUserById(userId);
                    
                    if (repost != null && user != null)
                    {
                        await _notificationService.CreateRepostLikeNotificationAsync(
                            repostId,
                            userId,
                            repost.UserID,
                            user.Name ?? "Unknown User"
                        );
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                return new RepostResult 
                { 
                    Success = false,
                    Message = ex.Message
                };
            }
        }

        /// <summary>
        /// Creates a comment on a repost and sends notification to repost owner
        /// </summary>
        /// <param name="request">Repost comment creation request</param>
        /// <returns>Result of the comment creation</returns>
        public async Task<RepostResult> CreateRepostCommentAsync(CreateRepostCommentRequest request)
        {
            try
            {
                // Validate request
                if (request.RepostID <= 0)
                    throw new ArgumentException("Valid repost ID is required");

                if (request.UserID <= 0)
                    throw new ArgumentException("Valid user ID is required");

                if (string.IsNullOrWhiteSpace(request.Content))
                    throw new ArgumentException("Comment content is required");

                // Create the comment
                int commentId = await _dbServices.CreateRepostCommentAsync(request);
                
                if (commentId > 0)
                {
                    // Get repost and user details for notification
                    var repost = await GetRepostByIdAsync(request.RepostID);
                    var user = _dbServices.GetUserById(request.UserID);
                    
                    if (repost != null && user != null)
                    {
                        // Create notification for repost owner
                        await _notificationService.CreateRepostCommentNotificationAsync(
                            request.RepostID,
                            commentId,
                            request.UserID,
                            repost.UserID,
                            user.Name ?? "Unknown User",
                            request.Content
                        );
                    }

                    return new RepostResult 
                    { 
                        Success = true, 
                        Message = "Comment created successfully",
                        RepostID = commentId
                    };
                }

                return new RepostResult 
                { 
                    Success = false, 
                    Message = "Failed to create comment"
                };
            }
            catch (Exception ex)
            {
                return new RepostResult 
                { 
                    Success = false, 
                    Message = ex.Message
                };
            }
        }

        /// <summary>
        /// Gets the number of likes on a repost
        /// </summary>
        /// <param name="repostId">ID of the repost</param>
        /// <returns>Number of likes</returns>
        public async Task<int> GetRepostLikeCountAsync(int repostId)
        {
            try
            {
                return await _dbServices.GetRepostLikeCountAsync(repostId);
            }
            catch (Exception)
            {
                return 0;
            }
        }

        /// <summary>
        /// Checks if a user has liked a specific repost
        /// </summary>
        /// <param name="repostId">ID of the repost</param>
        /// <param name="userId">ID of the user</param>
        /// <returns>True if user has liked the repost</returns>
        public async Task<bool> HasUserLikedRepostAsync(int repostId, int userId)
        {
            try
            {
                return await _dbServices.HasUserLikedRepostAsync(repostId, userId);
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Gets comments for a specific repost
        /// </summary>
        /// <param name="repostId">ID of the repost</param>
        /// <returns>List of repost comments</returns>
        public async Task<List<RepostComment>> GetRepostCommentsAsync(int repostId)
        {
            try
            {
                return await _dbServices.GetRepostCommentsAsync(repostId);
            }
            catch (Exception)
            {
                return new List<RepostComment>();
            }
        }

        /// <summary>
        /// Gets the number of comments on a repost
        /// </summary>
        /// <param name="repostId">ID of the repost</param>
        /// <returns>Number of comments</returns>
        public async Task<int> GetRepostCommentCountAsync(int repostId)
        {
            try
            {
                return await _dbServices.GetRepostCommentCountAsync(repostId);
            }
            catch (Exception)
            {
                return 0;
            }
        }

        /// <summary>
        /// Validates if a user can perform an action on a repost
        /// </summary>
        /// <param name="repostId">ID of the repost</param>
        /// <param name="userId">ID of the user</param>
        /// <returns>True if user can perform action</returns>
        public async Task<bool> CanUserAccessRepostAsync(int repostId, int userId)
        {
            try
            {
                var repost = await GetRepostByIdAsync(repostId);
                return repost != null && !repost.IsDeleted;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Validates if a user owns a specific repost
        /// </summary>
        /// <param name="repostId">ID of the repost</param>
        /// <param name="userId">ID of the user</param>
        /// <returns>True if user owns the repost</returns>
        public async Task<bool> IsRepostOwnerAsync(int repostId, int userId)
        {
            try
            {
                var repost = await GetRepostByIdAsync(repostId);
                return repost?.UserID == userId;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}