using NewsSite.BL;
using NewsSite.BL.Services;

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
        {
            if (commentId <= 0)
            {
                throw new ArgumentException("Valid Comment ID is required");
            }

            // This method would need to be implemented in DBservices
            return await Task.FromResult<Comment?>(null); // Placeholder
        }

        public async Task<int> GetCommentsCountAsync(int postId)
        {
            if (postId <= 0)
            {
                throw new ArgumentException("Valid Post ID is required");
            }

            var comments = await GetCommentsByPostIdAsync(postId);
            return comments.Count;
        }

        public async Task<string> ToggleCommentLikeAsync(int commentId, int userId)
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
    }
}
