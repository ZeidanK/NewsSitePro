using NewsSite.BL;

namespace NewsSite.BL.Services
{
    /// <summary>
    /// Comment Service - Business Logic Layer
    /// Implements comment-related business operations and validation
    /// </summary>
    public class CommentService : ICommentService
    {
        private readonly DBservices _dbService;

        public CommentService(DBservices dbService)
        {
            _dbService = dbService;
        }

        public async Task<List<Comment>> GetCommentsByPostIdAsync(int postId)
        {
            if (postId <= 0)
            {
                throw new ArgumentException("Valid Post ID is required");
            }

            return await _dbService.GetCommentsByPostId(postId);
        }

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

            return await _dbService.CreateComment(comment);
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

            return await Task.FromResult("liked"); // Implement like toggle logic in DBservices
        }
    }
}
