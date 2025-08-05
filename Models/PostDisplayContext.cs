using NewsSite.BL;

namespace NewsSitePro.Models
{
    /// <summary>
    /// Enhanced context system for post rendering that replaces the basic PostDisplayConfig.
    /// This context contains all the information needed to render a post appropriately
    /// based on the current user, page context, and user relationships.
    /// </summary>
    public class PostDisplayContext
    {
        // Basic display configuration (from existing PostDisplayConfig)
        public PostLayout Layout { get; set; } = PostLayout.Default;
        public bool ShowAuthorInfo { get; set; } = true;
        public bool ShowCategory { get; set; } = true;
        public bool ShowImage { get; set; } = true;
        public bool ShowSource { get; set; } = true;
        public bool ShowStats { get; set; } = true;
        public bool EnableInteractions { get; set; } = true;
        public bool ShowFullContent { get; set; } = false;
        public int ContentPreviewLength { get; set; } = 200;
        public string CssClass { get; set; } = "";
        public string ContainerClass { get; set; } = "post-card";

        // Enhanced context-aware properties
        public PostViewMode ViewMode { get; set; } = PostViewMode.Feed;
        
        // User relationship context
        public bool IsOwnPost { get; set; } = false;
        public bool IsFollowingAuthor { get; set; } = false;
        public bool IsAuthorBlocked { get; set; } = false;
        public bool ShowFollowButton { get; set; } = false;

        // Permission context
        public bool CanEdit { get; set; } = false;
        public bool CanDelete { get; set; } = false;
        public bool CanModerate { get; set; } = false;
        public bool CanReport { get; set; } = false;
        public bool CanBlock { get; set; } = false;

        // Interaction context - These should reflect the actual NewsArticle properties
        public bool IsLiked { get; set; } = false;
        public bool IsSaved { get; set; } = false;
        public bool ShowLikeButton { get; set; } = true;
        public bool ShowSaveButton { get; set; } = true;
        public bool ShowShareButton { get; set; } = true;
        public bool ShowCommentButton { get; set; } = true;
        public bool ShowComments { get; set; } = false;

        // Admin context
        public bool ShowAdminActions { get; set; } = false;
        public bool CanBanAuthor { get; set; } = false;
        public bool CanDeletePost { get; set; } = false;

        // Dynamic state
        public bool IsExpanded { get; set; } = false;
        public bool IsEditMode { get; set; } = false;
        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }
    }

    /// <summary>
    /// Defines different contexts where posts can be displayed
    /// </summary>
    public enum PostViewMode
    {
        Feed,           // Main feed view
        Profile,        // User profile page
        Individual,     // Single post view
        Admin,          // Admin panel
        Search,         // Search results
        Trending,       // Trending posts
        Following,      // Following feed
        Saved           // Saved posts
    }
}
