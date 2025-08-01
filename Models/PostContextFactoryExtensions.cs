using NewsSite.BL;

namespace NewsSitePro.Models
{
    /// <summary>
    /// Enhanced configuration factory that creates contexts for different page types and scenarios.
    /// This centralizes all logic for determining what UI elements should be shown based on
    /// the current page, user permissions, and relationships.
    /// 
    /// Usage Examples:
    /// - Feed: PostContextFactory.CreateFeedContext(user, post, "all")
    /// - Profile: PostContextFactory.CreateProfileContext(user, post, profileUserId)
    /// - Search: PostContextFactory.CreateSearchContext(user, post, searchQuery)
    /// </summary>
    public static class PostContextFactoryExtensions
    {
        /// <summary>
        /// Creates a context for search results view
        /// Optimized for scanning multiple posts quickly
        /// </summary>
        public static PostDisplayContext CreateSearchContext(User? currentUser, NewsArticle post, string? searchQuery = null)
        {
            var context = new PostDisplayContext
            {
                ViewMode = PostViewMode.Search,
                Layout = PostLayout.List,
                ShowAuthorInfo = true,
                ShowCategory = true,
                ShowImage = false, // Compact view for search
                ShowSource = false,
                ShowStats = false,
                EnableInteractions = true,
                ShowFullContent = false,
                ContentPreviewLength = 120,
                ContainerClass = "post-card post-search"
            };

            PostContextFactory.ApplyUserContext(context, currentUser, post);
            PostContextFactory.ApplyInteractionContext(context, currentUser, post);

            return context;
        }

        /// <summary>
        /// Creates a context for trending posts view
        /// Emphasizes popularity and engagement metrics
        /// </summary>
        public static PostDisplayContext CreateTrendingContext(User? currentUser, NewsArticle post)
        {
            var context = new PostDisplayContext
            {
                ViewMode = PostViewMode.Trending,
                Layout = PostLayout.Default,
                ShowAuthorInfo = true,
                ShowCategory = true,
                ShowImage = true,
                ShowSource = true,
                ShowStats = true, // Emphasize stats for trending
                EnableInteractions = true,
                ShowFullContent = false,
                ContentPreviewLength = 180,
                ContainerClass = "post-card post-trending"
            };

            PostContextFactory.ApplyUserContext(context, currentUser, post);
            PostContextFactory.ApplyInteractionContext(context, currentUser, post);

            return context;
        }

        /// <summary>
        /// Creates a context for saved posts view
        /// Shows when the post was saved and allows unsaving
        /// </summary>
        public static PostDisplayContext CreateSavedContext(User? currentUser, NewsArticle post)
        {
            var context = new PostDisplayContext
            {
                ViewMode = PostViewMode.Saved,
                Layout = PostLayout.Default,
                ShowAuthorInfo = true,
                ShowCategory = true,
                ShowImage = true,
                ShowSource = true,
                ShowStats = true,
                EnableInteractions = true,
                ShowFullContent = false,
                ContentPreviewLength = 200,
                ContainerClass = "post-card post-saved",
                IsSaved = true // Always true in saved context
            };

            PostContextFactory.ApplyUserContext(context, currentUser, post);
            PostContextFactory.ApplyInteractionContext(context, currentUser, post);

            return context;
        }

        /// <summary>
        /// Creates a context for mobile/responsive view
        /// Optimized for smaller screens and touch interactions
        /// </summary>
        public static PostDisplayContext CreateMobileContext(User? currentUser, NewsArticle post)
        {
            var context = new PostDisplayContext
            {
                ViewMode = PostViewMode.Feed,
                Layout = PostLayout.Default,
                ShowAuthorInfo = true,
                ShowCategory = false, // Less space on mobile
                ShowImage = true,
                ShowSource = false,
                ShowStats = true,
                EnableInteractions = true,
                ShowFullContent = false,
                ContentPreviewLength = 150,
                ContainerClass = "post-card post-mobile"
            };

            PostContextFactory.ApplyUserContext(context, currentUser, post);
            PostContextFactory.ApplyInteractionContext(context, currentUser, post);

            return context;
        }

        /// <summary>
        /// Creates a context based on string parameters for API flexibility
        /// </summary>
        public static PostDisplayContext CreateContextByType(string contextType, User? currentUser, NewsArticle post, object? additionalParams = null)
        {
            return contextType.ToLower() switch
            {
                "feed" => PostContextFactory.CreateFeedContext(currentUser, post, additionalParams?.ToString() ?? "all"),
                "individual" => PostContextFactory.CreateIndividualContext(currentUser, post),
                "profile" => PostContextFactory.CreateProfileContext(currentUser, post, (int)(additionalParams ?? 0)),
                "admin" => PostContextFactory.CreateAdminContext(currentUser, post),
                "compact" => PostContextFactory.CreateCompactContext(currentUser, post),
                "search" => CreateSearchContext(currentUser, post, additionalParams?.ToString()),
                "trending" => CreateTrendingContext(currentUser, post),
                "saved" => CreateSavedContext(currentUser, post),
                "mobile" => CreateMobileContext(currentUser, post),
                _ => PostContextFactory.CreateFeedContext(currentUser, post)
            };
        }
    }
}
