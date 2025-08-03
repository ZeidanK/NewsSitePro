/**
 * Repost.cs
 * 
 * Business Logic model for repost functionality.
 * Represents a user's repost of an original article with optional additional text.
 * Supports likes and comments on reposts independently from original articles.
 * 
 * Author: System
 * Date: 2025-08-03
 */

using System.ComponentModel.DataAnnotations;

namespace NewsSite.BL
{
    /// <summary>
    /// Represents a repost of an original article by a user
    /// </summary>
    public class Repost
    {
        /// <summary>
        /// Unique identifier for the repost
        /// </summary>
        public int RepostID { get; set; }

        /// <summary>
        /// ID of the original article being reposted
        /// </summary>
        [Required]
        public int OriginalArticleID { get; set; }

        /// <summary>
        /// ID of the user who created the repost
        /// </summary>
        [Required]
        public int UserID { get; set; }

        /// <summary>
        /// Optional text added by the user when reposting
        /// </summary>
        [StringLength(500, ErrorMessage = "Repost text cannot exceed 500 characters")]
        public string? RepostText { get; set; }

        /// <summary>
        /// When the repost was created
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// When the repost was last updated
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Soft delete flag
        /// </summary>
        public bool IsDeleted { get; set; }

        /// <summary>
        /// When the repost was deleted
        /// </summary>
        public DateTime? DeletedAt { get; set; }

        // Navigation properties for display
        /// <summary>
        /// The original article being reposted
        /// </summary>
        public NewsArticle? OriginalArticle { get; set; }

        /// <summary>
        /// Username of the person who reposted
        /// </summary>
        public string? RepostAuthor { get; set; }

        /// <summary>
        /// Display name of the person who reposted
        /// </summary>
        public string? RepostAuthorName { get; set; }

        /// <summary>
        /// Profile image of the person who reposted
        /// </summary>
        public string? RepostAuthorImage { get; set; }

        /// <summary>
        /// Number of likes on this repost
        /// </summary>
        public int LikesCount { get; set; }

        /// <summary>
        /// Number of comments on this repost
        /// </summary>
        public int CommentsCount { get; set; }

        /// <summary>
        /// Whether current user has liked this repost
        /// </summary>
        public bool IsLikedByUser { get; set; }
    }

    /// <summary>
    /// Request model for creating a new repost
    /// </summary>
    public class CreateRepostRequest
    {
        /// <summary>
        /// ID of the article to repost
        /// </summary>
        [Required]
        public int OriginalArticleID { get; set; }

        /// <summary>
        /// ID of the user creating the repost
        /// </summary>
        [Required]
        public int UserID { get; set; }

        /// <summary>
        /// Optional text to add when reposting
        /// </summary>
        [StringLength(500, ErrorMessage = "Repost text cannot exceed 500 characters")]
        public string? RepostText { get; set; }
    }

    /// <summary>
    /// Like on a repost
    /// </summary>
    public class RepostLike
    {
        /// <summary>
        /// Unique identifier for the repost like
        /// </summary>
        public int RepostLikeID { get; set; }

        /// <summary>
        /// ID of the repost being liked
        /// </summary>
        public int RepostID { get; set; }

        /// <summary>
        /// ID of the user who liked the repost
        /// </summary>
        public int UserID { get; set; }

        /// <summary>
        /// When the like was created
        /// </summary>
        public DateTime LikedAt { get; set; }

        /// <summary>
        /// Soft delete flag
        /// </summary>
        public bool IsDeleted { get; set; }
    }

    /// <summary>
    /// Comment on a repost
    /// </summary>
    public class RepostComment
    {
        /// <summary>
        /// Unique identifier for the repost comment
        /// </summary>
        public int RepostCommentID { get; set; }

        /// <summary>
        /// ID of the repost being commented on
        /// </summary>
        public int RepostID { get; set; }

        /// <summary>
        /// ID of the user who made the comment
        /// </summary>
        public int UserID { get; set; }

        /// <summary>
        /// Content of the comment
        /// </summary>
        [Required]
        [StringLength(1000, ErrorMessage = "Comment cannot exceed 1000 characters")]
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// When the comment was created
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// When the comment was last updated
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Soft delete flag
        /// </summary>
        public bool IsDeleted { get; set; }

        /// <summary>
        /// When the comment was deleted
        /// </summary>
        public DateTime? DeletedAt { get; set; }

        /// <summary>
        /// Parent comment ID for nested comments
        /// </summary>
        public int? ParentCommentID { get; set; }

        // Navigation properties for display
        /// <summary>
        /// Username of the commenter
        /// </summary>
        public string? Username { get; set; }

        /// <summary>
        /// Display name of the commenter
        /// </summary>
        public string? UserName { get; set; }

        /// <summary>
        /// Profile image of the commenter
        /// </summary>
        public string? UserProfileImage { get; set; }

        /// <summary>
        /// Nested replies to this comment
        /// </summary>
        public List<RepostComment>? Replies { get; set; }
    }

    /// <summary>
    /// Request model for creating a repost comment
    /// </summary>
    public class CreateRepostCommentRequest
    {
        /// <summary>
        /// ID of the repost to comment on
        /// </summary>
        [Required]
        public int RepostID { get; set; }

        /// <summary>
        /// ID of the user making the comment
        /// </summary>
        [Required]
        public int UserID { get; set; }

        /// <summary>
        /// Content of the comment
        /// </summary>
        [Required]
        [StringLength(1000, ErrorMessage = "Comment cannot exceed 1000 characters")]
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// Parent comment ID for replies
        /// </summary>
        public int? ParentCommentID { get; set; }
    }

    /// <summary>
    /// Response model for repost operations
    /// </summary>
    public class RepostResult
    {
        /// <summary>
        /// Whether the operation was successful
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Message describing the result
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// ID of the created/affected repost
        /// </summary>
        public int? RepostID { get; set; }

        /// <summary>
        /// Current like count (for like operations)
        /// </summary>
        public int? LikeCount { get; set; }

        /// <summary>
        /// Whether user liked the repost (for like operations)
        /// </summary>
        public bool? IsLiked { get; set; }
    }
}
