using NewsSite.BL;

// ----------------------------------------------------------------------------------
// SearchModels.cs
//
// This file defines models for search results in NewsSitePro. It includes classes for
// representing user and article search results, with properties for displaying relevant
// information in search views. These models are used to transfer search result data
// between backend and frontend components.
// ----------------------------------------------------------------------------------
namespace NewsSite.Models
{
    public class SearchUserResult
    {
        // Unique identifier for the user
        public int Id { get; set; }
        // User's display name
        public string? Name { get; set; }
        // User's email address
        public string? Email { get; set; }
        // User's biography or description
        public string? Bio { get; set; }
        // Date the user joined the platform
        public DateTime JoinDate { get; set; }
        // Indicates if the user is an admin
        public bool IsAdmin { get; set; }
        // Indicates if the user account is locked
        public bool IsLocked { get; set; }
        // Number of posts created by the user
        public int PostsCount { get; set; }
        // Number of followers the user has
        public int FollowersCount { get; set; }
        // Number of users this user is following
        public int FollowingCount { get; set; }
        // URL or path to the user's profile picture
        public string? ProfilePicture { get; set; }
    }

    public class SearchArticleResult
    {
        // Unique identifier for the article
        public int ArticleID { get; set; }
        // Title of the article
        public string? Title { get; set; }
        // Main content of the article
        public string? Content { get; set; }
        // URL to the original source of the article
        public string? SourceURL { get; set; }
        // Name of the source or publisher
        public string? SourceName { get; set; }
        // URL or path to the article's image
        public string? ImageURL { get; set; }
        // Date the article was published
        public DateTime PublishDate { get; set; }
        // Category or topic of the article
        public string? Category { get; set; }
        // ID of the user who posted the article
        public int UserID { get; set; }
        // Name of the user who posted the article
        public string? UserName { get; set; }
        // Number of likes the article has received
        public int LikesCount { get; set; }
        // Number of views the article has received
        public int ViewsCount { get; set; }
        // Indicates if the current user has liked the article
        public bool IsLiked { get; set; }
        // Indicates if the current user has saved the article
        public bool IsSaved { get; set; }
    }
}
