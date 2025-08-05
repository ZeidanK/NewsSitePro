using NewsSite.BL;
using NewsSitePro.Services;

namespace NewsSitePro.Models
{
    /// <summary>
    /// Enhanced version of PostContextFactory that integrates with UserRelationshipService
    /// for real relationship data instead of placeholder values.
    /// This provides the most accurate context-aware rendering.
    /// </summary>
    public static class PostContextFactoryEnhanced
    {
        private static IUserRelationshipService? _relationshipService;

        /// <summary>
        /// Initialize the factory with relationship service for enhanced context creation
        /// </summary>
        public static void Initialize(IUserRelationshipService relationshipService)
        {
            _relationshipService = relationshipService;
        }

        /// <summary>
        /// Creates an enhanced context with real relationship data
        /// </summary>
        public static async Task<PostDisplayContext> CreateEnhancedFeedContextAsync(User? currentUser, NewsArticle post, string feedType = "all")
        {
            var context = PostContextFactory.CreateFeedContext(currentUser, post, feedType);
            
            if (currentUser != null && _relationshipService != null)
            {
                await ApplyRealUserContextAsync(context, currentUser, post);
            }

            return context;
        }

        /// <summary>
        /// Creates an enhanced individual post context with real relationship data
        /// </summary>
        public static async Task<PostDisplayContext> CreateEnhancedIndividualContextAsync(User? currentUser, NewsArticle post)
        {
            var context = PostContextFactory.CreateIndividualContext(currentUser, post);
            
            if (currentUser != null && _relationshipService != null)
            {
                await ApplyRealUserContextAsync(context, currentUser, post);
            }

            return context;
        }

        /// <summary>
        /// Creates an enhanced profile context with real relationship data
        /// </summary>
        public static async Task<PostDisplayContext> CreateEnhancedProfileContextAsync(User? currentUser, NewsArticle post, int profileUserId)
        {
            var context = PostContextFactory.CreateProfileContext(currentUser, post, profileUserId);
            
            if (currentUser != null && _relationshipService != null)
            {
                await ApplyRealUserContextAsync(context, currentUser, post);
            }

            return context;
        }

        /// <summary>
        /// Applies real user relationship context from the database
        /// </summary>
        private static async Task ApplyRealUserContextAsync(PostDisplayContext context, User currentUser, NewsArticle post)
        {
            if (_relationshipService == null || currentUser.Id == post.UserID)
                return;

            try
            {
                var relationship = await _relationshipService.GetRelationshipStatusAsync(currentUser.Id, post.UserID);
                
                context.IsFollowingAuthor = relationship.IsFollowing;
                context.IsAuthorBlocked = relationship.IsBlocked;
                context.ShowFollowButton = relationship.ShowFollowButton;
                
                // If user is blocked, hide interactions
                if (relationship.IsBlocked)
                {
                    context.ShowLikeButton = false;
                    context.ShowSaveButton = false;
                    context.ShowShareButton = false;
                    context.ShowCommentButton = false;
                }
            }
            catch
            {
                // Fall back to basic context if relationship service fails
            }
        }

        /// <summary>
        /// Updates context based on interaction changes (like, save, follow)
        /// </summary>
        public static PostDisplayContext UpdateContextAfterInteraction(
            PostDisplayContext existingContext, 
            User currentUser, 
            NewsArticle post, 
            string interactionType, 
            bool newState)
        {
            switch (interactionType.ToLower())
            {
                case "like":
                    existingContext.IsLiked = newState;
                    break;
                    
                case "save":
                    existingContext.IsSaved = newState;
                    break;
                    
                case "follow":
                    existingContext.IsFollowingAuthor = newState;
                    existingContext.ShowFollowButton = !newState;
                    break;
                    
                case "block":
                    existingContext.IsAuthorBlocked = newState;
                    if (newState)
                    {
                        // Blocking should hide interactions and follow button
                        existingContext.ShowFollowButton = false;
                        existingContext.IsFollowingAuthor = false;
                        existingContext.ShowLikeButton = false;
                        existingContext.ShowSaveButton = false;
                        existingContext.ShowShareButton = false;
                        existingContext.ShowCommentButton = false;
                    }
                    else
                    {
                        // Unblocking should restore interactions
                        existingContext.ShowLikeButton = true;
                        existingContext.ShowSaveButton = true;
                        existingContext.ShowShareButton = true;
                        existingContext.ShowCommentButton = true;
                        existingContext.ShowFollowButton = true;
                    }
                    break;
            }

            return existingContext;
        }
    }
}
