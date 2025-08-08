using NewsSite.BL;

namespace NewsSitePro.Models
{
    /// <summary>
    /// Factory class responsible for creating appropriate PostDisplayContext objects
    /// based on the current viewing context, user relationships, and permissions.
    /// This centralizes all the logic for determining what UI elements should be shown.
    /// </summary>
    public static class PostContextFactory
    {
        /// <summary>
        /// Creates a context for feed view (main homepage feed)
        /// </summary>
        public static async Task<PostDisplayContext> CreateFeedContextAsync(User? currentUser, NewsArticle post, string feedType = "all", Dictionary<int, bool>? followStatusMap = null, Dictionary<int, bool>? blockStatusMap = null)
        {
            var context = new PostDisplayContext
            {
                ViewMode = PostViewMode.Feed,
                Layout = PostLayout.Default,
                ShowAuthorInfo = true,
                ShowCategory = true,
                ShowImage = true,
                ShowSource = true,
                ShowStats = true,
                EnableInteractions = true,
                ShowFullContent = false,
                ContentPreviewLength = 200,
                ContainerClass = "post-card"
            };

            await ApplyUserContextAsync(context, currentUser, post, followStatusMap, blockStatusMap);
            ApplyInteractionContext(context, currentUser, post);

            // Feed-specific logic
            if (feedType == "following")
            {
                context.ShowFollowButton = false; // Already following
            }

            return context;
        }

        /// <summary>
        /// Creates a context for feed view (main homepage feed) - Synchronous version for backward compatibility
        /// </summary>
        public static PostDisplayContext CreateFeedContext(User? currentUser, NewsArticle post, string feedType = "all", Dictionary<int, bool>? followStatusMap = null)
        {
            var context = new PostDisplayContext
            {
                ViewMode = PostViewMode.Feed,
                Layout = PostLayout.Default,
                ShowAuthorInfo = true,
                ShowCategory = true,
                ShowImage = true,
                ShowSource = true,
                ShowStats = true,
                EnableInteractions = true,
                ShowFullContent = false,
                ContentPreviewLength = 200,
                ContainerClass = "post-card"
            };

            ApplyUserContext(context, currentUser, post, followStatusMap);
            ApplyInteractionContext(context, currentUser, post);

            // Feed-specific logic
            if (feedType == "following")
            {
                context.ShowFollowButton = false; // Already following
            }

            return context;
        }

        /// <summary>
        /// Creates a context for individual post view (full post with comments)
        /// </summary>
        public static PostDisplayContext CreateIndividualContext(User? currentUser, NewsArticle post, Dictionary<int, bool>? followStatusMap = null)
        {
            var context = new PostDisplayContext
            {
                ViewMode = PostViewMode.Individual,
                Layout = PostLayout.Default,
                ShowAuthorInfo = true,
                ShowCategory = true,
                ShowImage = true,
                ShowSource = true,
                ShowStats = true,
                EnableInteractions = true,
                ShowFullContent = true,
                ShowComments = true, // Enable comments for individual post view
                ContainerClass = "post-card post-individual"
            };

            ApplyUserContext(context, currentUser, post, followStatusMap);
            ApplyInteractionContext(context, currentUser, post);

            return context;
        }

        /// <summary>
        /// Creates a context for user profile view
        /// </summary>
        public static PostDisplayContext CreateProfileContext(User? currentUser, NewsArticle post, int profileUserId)
        {
            var context = new PostDisplayContext
            {
                ViewMode = PostViewMode.Profile,
                Layout = PostLayout.Default,
                ShowAuthorInfo = false, // Don't show author info on their own profile
                ShowCategory = true,
                ShowImage = true,
                ShowSource = true,
                ShowStats = true,
                EnableInteractions = true,
                ShowFullContent = false,
                ContentPreviewLength = 150,
                ContainerClass = "post-card post-profile"
            };

            ApplyUserContext(context, currentUser, post);
            ApplyInteractionContext(context, currentUser, post);

            // Profile-specific logic
            context.ShowFollowButton = false; // Not relevant on profile page
            if (currentUser?.Id == profileUserId)
            {
                // Viewing own profile
                context.CanEdit = false; // Temporarily disabled until edit functionality is implemented
                context.CanDelete = true;
            }

            return context;
        }

        /// <summary>
        /// Creates a context for admin panel view
        /// </summary>
        public static PostDisplayContext CreateAdminContext(User? currentUser, NewsArticle post)
        {
            var context = new PostDisplayContext
            {
                ViewMode = PostViewMode.Admin,
                Layout = PostLayout.List,
                ShowAuthorInfo = true,
                ShowCategory = true,
                ShowImage = false,
                ShowSource = true,
                ShowStats = true,
                EnableInteractions = false,
                ShowFullContent = false,
                ContentPreviewLength = 100,
                ContainerClass = "post-card post-admin",
                ShowAdminActions = true
            };

            ApplyUserContext(context, currentUser, post);

            // Admin-specific permissions
            if (currentUser?.IsAdmin == true)
            {
                context.CanModerate = true;
                context.CanDelete = true;
                context.CanBanAuthor = true;
                context.CanDeletePost = true;
            }

            return context;
        }

        /// <summary>
        /// Creates a compact context for smaller displays or sidebars
        /// </summary>
        public static PostDisplayContext CreateCompactContext(User? currentUser, NewsArticle post)
        {
            var context = new PostDisplayContext
            {
                ViewMode = PostViewMode.Feed,
                Layout = PostLayout.Compact,
                ShowAuthorInfo = false,
                ShowCategory = false,
                ShowImage = false,
                ShowSource = false,
                ShowStats = false,
                EnableInteractions = false,
                ShowFullContent = false,
                ContentPreviewLength = 80,
                ContainerClass = "post-card post-compact"
            };

            return context;
        }

        /// <summary>
        /// Applies user-specific context (ownership, following, blocking) - Async version
        /// </summary>
        public static async Task ApplyUserContextAsync(PostDisplayContext context, User? currentUser, NewsArticle post, Dictionary<int, bool>? followStatusMap = null, Dictionary<int, bool>? blockStatusMap = null)
        {
            if (currentUser == null)
            {
                // Guest user
                context.ShowFollowButton = false;
                context.CanEdit = false;
                context.CanDelete = false;
                context.CanReport = false;
                context.CanBlock = false;
                return;
            }

            // Check if user owns the post
            context.IsOwnPost = currentUser.Id == post.UserID;

            if (context.IsOwnPost)
            {
                context.ShowFollowButton = false;
                context.CanEdit = false; // Temporarily disabled until edit functionality is implemented
                context.CanDelete = true;
                context.CanReport = false;
                context.CanBlock = false;
            }
            else
            {
                // Use follow status from map if available
                bool isFollowing = false;
                if (followStatusMap != null && followStatusMap.ContainsKey(post.UserID))
                {
                    isFollowing = followStatusMap[post.UserID];
                }
                
                context.IsFollowingAuthor = isFollowing;
                
                // Check blocking status from map
                bool isAuthorBlocked = false;
                if (blockStatusMap != null && blockStatusMap.ContainsKey(post.UserID))
                {
                    isAuthorBlocked = blockStatusMap[post.UserID];
                }
                context.IsAuthorBlocked = isAuthorBlocked;
                
                context.ShowFollowButton = !context.IsAuthorBlocked; // Show follow button unless blocked
                context.CanReport = true;
                context.CanBlock = !context.IsAuthorBlocked; // Can block if not already blocked
            }

            // Admin permissions
            if (currentUser.IsAdmin)
            {
                context.CanModerate = true;
                context.CanDelete = true;
            }
        }

        /// <summary>
        /// Applies user-specific context (ownership, following, blocking) - Synchronous version for backward compatibility
        /// </summary>
        public static void ApplyUserContext(PostDisplayContext context, User? currentUser, NewsArticle post, Dictionary<int, bool>? followStatusMap = null)
        {
            if (currentUser == null)
            {
                // Guest user
                context.ShowFollowButton = false;
                context.CanEdit = false;
                context.CanDelete = false;
                context.CanReport = false;
                context.CanBlock = false;
                return;
            }

            // Check if user owns the post
            context.IsOwnPost = currentUser.Id == post.UserID;

            if (context.IsOwnPost)
            {
                context.ShowFollowButton = false;
                context.CanEdit = false; // Temporarily disabled until edit functionality is implemented
                context.CanDelete = true;
                context.CanReport = false;
                context.CanBlock = false;
            }
            else
            {
                // Use follow status from map if available
                bool isFollowing = false;
                if (followStatusMap != null && followStatusMap.ContainsKey(post.UserID))
                {
                    isFollowing = followStatusMap[post.UserID];
                }
                
                context.IsFollowingAuthor = isFollowing;
                context.IsAuthorBlocked = false;   // TODO: Implement blocking system in synchronous contexts
                
                context.ShowFollowButton = !context.IsAuthorBlocked; // Show follow button unless blocked
                context.CanReport = true;
                context.CanBlock = true;
            }

            // Admin permissions
            if (currentUser.IsAdmin)
            {
                context.CanModerate = true;
                context.CanDelete = true;
            }
        }

        /// <summary>
        /// Applies interaction context (likes, saves, etc.) using actual post data
        /// This ensures the context reflects the real user interaction state from the database
        /// </summary>
        public static void ApplyInteractionContext(PostDisplayContext context, User? currentUser, NewsArticle post)
        {
            if (currentUser == null)
            {
                // Guest users - show counts but no personal interaction state
                context.IsLiked = false;
                context.IsSaved = false;
                context.ShowLikeButton = false; // Guests can't interact
                context.ShowSaveButton = false;
                context.ShowShareButton = true; // Guests can still share
                context.ShowCommentButton = true; // Guests can view comments
                return;
            }

            // Apply the actual user interaction state from the NewsArticle model
            // This is populated by the database queries in DBservices with proper user context
            context.IsLiked = post.IsLiked;
            context.IsSaved = post.IsSaved;
            
            // Enable interaction buttons for authenticated users
            context.ShowLikeButton = true;
            context.ShowSaveButton = true;
            context.ShowShareButton = true;
            context.ShowCommentButton = true;
        }

        /// <summary>
        /// Updates an existing context with new interaction state
        /// </summary>
        public static void UpdateInteractionState(PostDisplayContext context, bool? isLiked = null, bool? isSaved = null)
        {
            if (isLiked.HasValue)
                context.IsLiked = isLiked.Value;
            
            if (isSaved.HasValue)
                context.IsSaved = isSaved.Value;
        }
    }
}
