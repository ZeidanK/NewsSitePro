using System.ComponentModel.DataAnnotations;

namespace NewsSite.BL
{
    public class Comment
    {
        public int ID { get; set; }
        public int PostID { get; set; }
        public int UserID { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
        public bool IsDeleted { get; set; } = false;
        public int? ParentCommentID { get; set; } // For nested comments/replies
        
        // Navigation properties (not stored in DB)
        public string? UserName { get; set; }
        public string? UserAvatar { get; set; }
        public string? UserProfileImageURL { get; set; }
        public DateTime CommentDate => CreatedAt; // Alias for CreatedAt
        public List<Comment> Replies { get; set; } = new List<Comment>();
        public int LikesCount { get; set; } = 0;
        public bool IsLikedByCurrentUser { get; set; } = false;
    }

    public class CreateCommentRequest
    {
        [Required]
        public int PostID { get; set; }
        
        [Required]
        [StringLength(1000, MinimumLength = 1)]
        public string Content { get; set; } = string.Empty;
        
        public int? ParentCommentID { get; set; } // For replies
    }

    public class UpdateCommentRequest
    {
        [Required]
        public int CommentID { get; set; }
        
        [Required]
        [StringLength(1000, MinimumLength = 1)]
        public string Content { get; set; } = string.Empty;
    }
}
