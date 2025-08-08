using NewsSite.BL;
using NewsSite.BL.Services;

// ----------------------------------------------------------------------------------
// CommentService.cs
//
// This class implements comment-related business logic for the NewsSitePro application. It provides
// methods for creating, updating, deleting, and retrieving comments on posts and reposts, and integrates
// with NotificationService to send notifications for comment actions. The service enforces business rules
// (such as content validation, ownership checks, and time limits) and interacts with the database layer
// through DBservices. All methods are asynchronous for efficient, non-blocking operations. Comments are
// added to key functions for clarity.
// ----------------------------------------------------------------------------------

namespace NewsSite.BL.Services
{
    /// <summary>
    /// Comment Service - Business Logic Layer
    /// Implements comment-related business operations and validation
    /// Integrates with NotificationService for comment-related notifications
    /// </summary>
    public class CommentService : ICommentService
    {
        private readonly DBservices _dbService;
        private readonly NotificationService _notificationService;

        public CommentService(DBservices dbService, NotificationService notificationService)
        {
            _dbService = dbService;
            _notificationService = notificationService;
        }

        public async Task<List<Comment>> GetCommentsByPostIdAsync(int postId)
        // Get all comments for a specific post
        {
            if (postId <= 0)
            {
                throw new ArgumentException("Valid Post ID is required");
            }

            return await _dbService.GetCommentsByPostId(postId);
        }

        /// <summary>
        /// Creates a new comment and sends notification to post owner
        /// </summary>
        /// <param name="comment">Comment to create</param>
        /// <returns>True if comment was created successfully</returns>
        public async Task<bool> CreateCommentAsync(Comment comment)
        // Create a new comment and send notification to post owner
        {
            // Business logic validation
            if (string.IsNullOrWhiteSpace(comment.Content))
            {
                throw new ArgumentException("Comment content is required");
            }

            if (comment.PostID <= 0)
            {
                throw new ArgumentException("Valid Post ID is required");
            }

            if (comment.UserID <= 0)
            {
                throw new ArgumentException("Valid User ID is required");
            }

            // Validate content length
            if (comment.Content.Length > 1000)
            {
                throw new ArgumentException("Comment cannot exceed 1000 characters");
            }

            // Validate parent comment if provided
            if (comment.ParentCommentID.HasValue)
            {
                var parentComment = await GetCommentByIdAsync(comment.ParentCommentID.Value);
                if (parentComment == null)
                {
                    throw new ArgumentException("Parent comment not found");
                }

                if (parentComment.PostID != comment.PostID)
                {
                    throw new ArgumentException("Parent comment must belong to the same post");
                }
            }

            // Create the comment and get the new comment ID
            int commentId = await _dbService.CreateComment(comment);
            
            if (commentId > 0)
            {
                try
                {
                    // Get article details to find the article author
                    var article = await _dbService.GetNewsArticleById(comment.PostID);
                    if (article != null)
                    {
                        // Get commenter details
                        var commenter = _dbService.GetUserById(comment.UserID);
                        if (commenter != null)
                        {
                            // Create notification for article author
                            await _notificationService.CreateCommentNotificationAsync(
                                comment.PostID,
                                commentId,
                                comment.UserID,
                                article.UserID,
                                commenter.Name ?? "Unknown User"
                            );
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Log the notification error but don't fail the comment creation
                    Console.WriteLine($"Failed to create comment notification: {ex.Message}");
                }
                
                return true;
            }

            return false;
        }

        public async Task<bool> UpdateCommentAsync(int commentId, int userId, string content)
        // Update an existing comment (only by author, within 24 hours)
        {
            // Business logic validation
            if (commentId <= 0)
            {
                throw new ArgumentException("Valid Comment ID is required");
            }

            if (string.IsNullOrWhiteSpace(content))
            {
                throw new ArgumentException("Comment content is required");
            }

            // Check if comment exists
            var existingComment = await GetCommentByIdAsync(commentId);
            if (existingComment == null)
            {
                throw new ArgumentException("Comment not found");
            }

            // Business rule: Only the author can update their comment
            if (existingComment.UserID != userId)
            {
                throw new UnauthorizedAccessException("You can only update your own comments");
            }

            // Business rule: Cannot edit comments older than 24 hours
            if (existingComment.CreatedAt < DateTime.Now.AddHours(-24))
            {
                throw new InvalidOperationException("Comments cannot be edited after 24 hours");
            }

            return await Task.FromResult(true); // Implement update logic in DBservices
        }

        public async Task<bool> DeleteCommentAsync(int commentId, int userId)
        // Delete a comment (only by author)
        {
            if (commentId <= 0 || userId <= 0)
            {
                throw new ArgumentException("Valid Comment ID and User ID are required");
            }

            var comment = await GetCommentByIdAsync(commentId);
            if (comment == null)
            {
                throw new ArgumentException("Comment not found");
            }

            // Business rule: Only the author can delete their comment
            if (comment.UserID != userId)
            {
                throw new UnauthorizedAccessException("You can only delete your own comments");
            }

            return await Task.FromResult(true); // Implement delete logic in DBservices
        }

        public async Task<Comment?> GetCommentByIdAsync(int commentId)
        // Get a specific comment by its ID
        {
            if (commentId <= 0)
            {
                throw new ArgumentException("Valid Comment ID is required");
            }

            // This method would need to be implemented in DBservices
            return await Task.FromResult<Comment?>(null); // Placeholder
        }

        public async Task<int> GetCommentsCountAsync(int postId)
        // Get the number of comments for a specific post
        {
            if (postId <= 0)
            {
                throw new ArgumentException("Valid Post ID is required");
            }

            var comments = await GetCommentsByPostIdAsync(postId);
            return comments.Count;
        }

        public async Task<string> ToggleCommentLikeAsync(int commentId, int userId)
        // Like or unlike a comment, and send notification if liked
        {
            if (commentId <= 0 || userId <= 0)
            {
                throw new ArgumentException("Valid Comment ID and User ID are required");
            }

            var comment = await GetCommentByIdAsync(commentId);
            if (comment == null)
            {
                throw new ArgumentException("Comment not found");
            }

            var result = await _dbService.ToggleCommentLike(commentId, userId);
            
            // Create like notification if comment was liked (not unliked)
            if (result == "liked")
            {
                try
                {
                    // Get liker details
                    var liker = _dbService.GetUserById(userId);
                    if (liker != null)
                    {
                        // Create notification for comment author (using a custom notification for comment likes)
                        var request = new CreateNotificationRequest
                        {
                            UserID = comment.UserID,
                            Type = "CommentLike",
                            Title = "Comment Liked",
                            Message = $"{liker.Name ?? "Unknown User"} liked your comment",
                            RelatedEntityType = "Comment",
                            RelatedEntityID = commentId,
                            FromUserID = userId,
                            ActionUrl = $"/Posts/Details/{comment.PostID}#comment-{commentId}"
                        };

                        await _notificationService.CreateCustomNotificationAsync(request);
                    }
                }
                catch (Exception ex)
                {
                    // Log the notification error but don't fail the like action
                    Console.WriteLine($"Failed to create comment like notification: {ex.Message}");
                }
            }
            
            return result;
        }

        /// <summary>
        /// Creates a comment on a repost and sends notification to repost owner
        /// </summary>
        /// <param name="repostComment">Repost comment to create</param>
        /// <returns>True if comment was created successfully</returns>
        public async Task<bool> CreateRepostCommentAsync(RepostComment repostComment)
        // Create a comment on a repost and send notification to repost owner
        {
            // Business logic validation
            if (string.IsNullOrWhiteSpace(repostComment.Content))
            {
                throw new ArgumentException("Comment content is required");
            }

            if (repostComment.RepostID <= 0)
            {
                throw new ArgumentException("Valid Repost ID is required");
            }

            if (repostComment.UserID <= 0)
            {
                throw new ArgumentException("Valid User ID is required");
            }

            // Validate content length
            if (repostComment.Content.Length > 1000)
            {
                throw new ArgumentException("Comment cannot exceed 1000 characters");
            }

            // Validate parent comment if provided
            if (repostComment.ParentCommentID.HasValue)
            {
                var parentComment = await GetRepostCommentByIdAsync(repostComment.ParentCommentID.Value);
                if (parentComment == null)
                {
                    throw new ArgumentException("Parent comment not found");
                }

                if (parentComment.RepostID != repostComment.RepostID)
                {
                    throw new ArgumentException("Parent comment must belong to the same repost");
                }
            }

            // Create the comment and get the new comment ID
            var createRequest = new CreateRepostCommentRequest
            {
                RepostID = repostComment.RepostID,
                UserID = repostComment.UserID,
                Content = repostComment.Content,
                ParentCommentID = repostComment.ParentCommentID
            };
            
            int commentId = await _dbService.CreateRepostCommentAsync(createRequest);
            
            if (commentId > 0)
            {
                try
                {
                    // Get repost details to find the repost owner
                    var repost = await _dbService.GetRepostByIdAsync(repostComment.RepostID);
                    if (repost != null)
                    {
                        // Get commenter details
                        var commenter = _dbService.GetUserById(repostComment.UserID);
                        if (commenter != null)
                        {
                            // Create notification for repost owner
                            await _notificationService.CreateRepostCommentNotificationAsync(
                                repostComment.RepostID,
                                commentId,
                                repostComment.UserID,
                                repost.UserID,
                                commenter.Name ?? "Unknown User",
                                repostComment.Content
                            );
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Log the notification error but don't fail the comment creation
                    Console.WriteLine($"Failed to create repost comment notification: {ex.Message}");
                }
                
                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets comments for a specific repost
        /// </summary>
        /// <param name="repostId">ID of the repost</param>
        /// <returns>List of repost comments</returns>
        public async Task<List<RepostComment>> GetCommentsByRepostIdAsync(int repostId)
        // Get all comments for a specific repost
        {
            if (repostId <= 0)
            {
                throw new ArgumentException("Valid Repost ID is required");
            }

            return await _dbService.GetCommentsByRepostId(repostId);
        }

        /// <summary>
        /// Gets a specific repost comment by ID
        /// </summary>
        /// <param name="commentId">ID of the comment</param>
        /// <returns>RepostComment or null if not found</returns>
        public async Task<RepostComment> GetRepostCommentByIdAsync(int commentId)
        // Get a specific repost comment by its ID
        {
            if (commentId <= 0)
            {
                throw new ArgumentException("Valid Comment ID is required");
            }

            return await _dbService.GetRepostCommentById(commentId);
        }

        /// <summary>
        /// Updates a repost comment
        /// </summary>
        /// <param name="commentId">ID of the comment to update</param>
        /// <param name="userId">ID of the user making the update</param>
        /// <param name="content">New content for the comment</param>
        /// <returns>True if update was successful</returns>
        public async Task<bool> UpdateRepostCommentAsync(int commentId, int userId, string content)
        // Update a repost comment (only by author)
        {
            // Business logic validation
            if (commentId <= 0)
            {
                throw new ArgumentException("Valid Comment ID is required");
            }

            if (userId <= 0)
            {
                throw new ArgumentException("Valid User ID is required");
            }

            if (string.IsNullOrWhiteSpace(content))
            {
                throw new ArgumentException("Comment content is required");
            }

            if (content.Length > 1000)
            {
                throw new ArgumentException("Comment cannot exceed 1000 characters");
            }

            // Verify user owns the comment
            var comment = await GetRepostCommentByIdAsync(commentId);
            if (comment == null)
            {
                throw new ArgumentException("Comment not found");
            }

            if (comment.UserID != userId)
            {
                throw new UnauthorizedAccessException("You can only edit your own comments");
            }

            return await _dbService.UpdateRepostComment(commentId, content);
        }

        /// <summary>
        /// Deletes a repost comment (soft delete)
        /// </summary>
        /// <param name="commentId">ID of the comment to delete</param>
        /// <param name="userId">ID of the user requesting deletion</param>
        /// <returns>True if deletion was successful</returns>
        public async Task<bool> DeleteRepostCommentAsync(int commentId, int userId)
        // Delete a repost comment (only by author)
        {
            if (commentId <= 0)
            {
                throw new ArgumentException("Valid Comment ID is required");
            }

            if (userId <= 0)
            {
                throw new ArgumentException("Valid User ID is required");
            }

            // Verify user owns the comment
            var comment = await GetRepostCommentByIdAsync(commentId);
            if (comment == null)
            {
                throw new ArgumentException("Comment not found");
            }

            if (comment.UserID != userId)
            {
                throw new UnauthorizedAccessException("You can only delete your own comments");
            }

            return await _dbService.DeleteRepostComment(commentId);
        }
    }
}
