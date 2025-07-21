using System;
using System.Collections.Generic;

namespace NewsSite.BL
{
    public class Post
    {
        public int PostID { get; set; }
        
        // The article being shared/posted
        public NewsArticle Article { get; set; }
        public int ArticleID { get; set; }
        
        // User who posted/shared this content
        public int UserID { get; set; }
        public string? UserName { get; set; }
        public string? UserProfileImageURL { get; set; }
        
        // Original post info (if this is a share/repost)
        public int? OriginalPostID { get; set; }
        public string? OriginalPosterName { get; set; }
        public string? OriginalPosterProfileImageURL { get; set; }
        public bool IsShared => OriginalPostID.HasValue;
        
        // Post metadata
        public DateTime PostedDate { get; set; }
        public DateTime? LastEditedDate { get; set; }
        
        // Interaction counts
        public int LikeCount { get; set; }
        public int CommentCount { get; set; }
        public int ShareCount { get; set; }
        
        // User interaction status (for the current viewer)
        public bool IsLikedByCurrentUser { get; set; }
        
        // Optional collections for detailed view
        public List<Comment>? Comments { get; set; }
        public List<Like>? Likes { get; set; }
    }

    // Supporting classes
    public class PostComment
    {
        public int CommentID { get; set; }
        public int PostID { get; set; }
        public int UserID { get; set; }
        public string? UserName { get; set; }
        public string? UserProfileImageURL { get; set; }
        public string? Content { get; set; }
        public DateTime CommentDate { get; set; }
        public int LikeCount { get; set; }
    }

    public class Like
    {
        public int LikeID { get; set; }
        public int PostID { get; set; }
        public int UserID { get; set; }
        public string? UserName { get; set; }
        public DateTime LikeDate { get; set; }
    }
}