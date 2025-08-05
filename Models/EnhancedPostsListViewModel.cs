using NewsSite.BL;

namespace NewsSitePro.Models
{
    /// <summary>
    /// Enhanced ViewModel for posts list that supports advanced context-aware rendering.
    /// This extends the basic PostsListViewModel to support the new enhanced context system
    /// with real-time relationship data and dynamic context switching.
    /// 
    /// Used by:
    /// - Enhanced posts API endpoints
    /// - Context-aware partial views
    /// - Dynamic feed switching
    /// </summary>
    public class EnhancedPostsListViewModel : PostsListViewModel
    {
        /// <summary>
        /// The specific context type for rendering (feed, profile, admin, etc.)
        /// </summary>
        public string ContextType { get; set; } = "feed";

        /// <summary>
        /// Whether to use enhanced context with real relationship data
        /// </summary>
        public bool UseEnhancedContext { get; set; } = false;

        /// <summary>
        /// Profile user ID when viewing a user's profile
        /// </summary>
        public int? ProfileUserId { get; set; }

        /// <summary>
        /// Additional parameters for context creation
        /// </summary>
        public object? ContextParameters { get; set; }

        /// <summary>
        /// Whether this is a paginated request (affects loading behavior)
        /// </summary>
        public bool IsPaginated { get; set; } = false;

        /// <summary>
        /// Current page number for pagination
        /// </summary>
        public int CurrentPage { get; set; } = 1;

        /// <summary>
        /// Whether there are more posts available
        /// </summary>
        public bool HasMorePosts { get; set; } = true;

        /// <summary>
        /// CSS classes to apply to the posts container
        /// </summary>
        public string ContainerCssClass { get; set; } = "posts-container";

        /// <summary>
        /// Gets the appropriate ViewMode enum based on ContextType
        /// </summary>
        public PostViewMode PostViewMode
        {
            get
            {
                return ContextType.ToLower() switch
                {
                    "feed" => PostViewMode.Feed,
                    "profile" => PostViewMode.Profile,
                    "individual" => PostViewMode.Individual,
                    "admin" => PostViewMode.Admin,
                    "search" => PostViewMode.Search,
                    "trending" => PostViewMode.Trending,
                    "following" => PostViewMode.Following,
                    "saved" => PostViewMode.Saved,
                    _ => PostViewMode.Feed
                };
            }
        }
    }
}
