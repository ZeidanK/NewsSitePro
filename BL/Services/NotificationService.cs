using NewsSite.BL;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NewsSite.BL.Services
{
    /// <summary>
    /// Service for handling all notification-related operations
    /// This service provides high-level methods for creating notifications for different actions
    /// </summary>
    public class NotificationService
    {
        private readonly DBservices _dbService;

        public NotificationService(DBservices dbService)
        {
            _dbService = dbService;
        }

        /// <summary>
        /// Creates a notification when a user likes a post
        /// </summary>
        /// <param name="postId">ID of the post that was liked</param>
        /// <param name="likedByUserId">ID of the user who liked the post</param>
        /// <param name="postOwnerId">ID of the user who owns the post</param>
        /// <param name="likedByUserName">Name of the user who liked the post</param>
        /// <returns>ID of created notification or -1 if failed</returns>
        public async Task<int> CreateLikeNotificationAsync(int postId, int likedByUserId, int postOwnerId, string likedByUserName)
        {
            Console.WriteLine($"[NotificationService] CreateLikeNotificationAsync called - PostId: {postId}, LikedByUserId: {likedByUserId}, PostOwnerId: {postOwnerId}, LikedByUserName: {likedByUserName}");
            
            // Don't create notification if user likes their own post
            if (likedByUserId == postOwnerId)
            {
                Console.WriteLine($"[NotificationService] Skipping notification - user liked their own post");
                return -1;
            }

            var request = new CreateNotificationRequest
            {
                UserID = postOwnerId,
                Type = NotificationTypes.Like,
                Title = "New Like",
                Message = $"{likedByUserName} liked your post",
                RelatedEntityType = "Post",
                RelatedEntityID = postId,
                FromUserID = likedByUserId,
                ActionUrl = $"/Posts/Details/{postId}"
            };

            Console.WriteLine($"[NotificationService] Creating notification request for user {postOwnerId}: {request.Message}");
            var notificationId = await _dbService.CreateNotification(request);
            Console.WriteLine($"[NotificationService] Notification created with ID: {notificationId}");
            
            return notificationId;
        }

        /// <summary>
        /// Creates a notification when a user comments on a post
        /// </summary>
        /// <param name="postId">ID of the post that was commented on</param>
        /// <param name="commentId">ID of the created comment</param>
        /// <param name="commentByUserId">ID of the user who made the comment</param>
        /// <param name="postOwnerId">ID of the user who owns the post</param>
        /// <param name="commentByUserName">Name of the user who made the comment</param>
        /// <returns>ID of created notification or -1 if failed</returns>
        public async Task<int> CreateCommentNotificationAsync(int postId, int commentId, int commentByUserId, int postOwnerId, string commentByUserName)
        {
            // Don't create notification if user comments on their own post
            if (commentByUserId == postOwnerId)
                return -1;

            var request = new CreateNotificationRequest
            {
                UserID = postOwnerId,
                Type = NotificationTypes.Comment,
                Title = "New Comment",
                Message = $"{commentByUserName} commented on your post",
                RelatedEntityType = "Post",
                RelatedEntityID = postId,
                FromUserID = commentByUserId,
                ActionUrl = $"/Posts/Details/{postId}#comment-{commentId}"
            };

            return await _dbService.CreateNotification(request);
        }

        /// <summary>
        /// Creates a notification when a user follows another user
        /// </summary>
        /// <param name="followerId">ID of the user who followed</param>
        /// <param name="followedId">ID of the user who was followed</param>
        /// <param name="followerName">Name of the user who followed</param>
        /// <returns>ID of created notification or -1 if failed</returns>
        public async Task<int> CreateFollowNotificationAsync(int followerId, int followedId, string followerName)
        {
            var request = new CreateNotificationRequest
            {
                UserID = followedId,
                Type = NotificationTypes.Follow,
                Title = "New Follower",
                Message = $"{followerName} started following you",
                RelatedEntityType = "User",
                RelatedEntityID = followerId,
                FromUserID = followerId,
                ActionUrl = $"/User/Profile/{followerId}"
            };

            return await _dbService.CreateNotification(request);
        }

        /// <summary>
        /// Creates a notification when a user shares a post
        /// </summary>
        /// <param name="postId">ID of the post that was shared</param>
        /// <param name="sharedByUserId">ID of the user who shared the post</param>
        /// <param name="postOwnerId">ID of the user who owns the post</param>
        /// <param name="sharedByUserName">Name of the user who shared the post</param>
        /// <returns>ID of created notification or -1 if failed</returns>
        public async Task<int> CreateShareNotificationAsync(int postId, int sharedByUserId, int postOwnerId, string sharedByUserName)
        {
            // Don't create notification if user shares their own post
            if (sharedByUserId == postOwnerId)
                return -1;

            var request = new CreateNotificationRequest
            {
                UserID = postOwnerId,
                Type = NotificationTypes.PostShare,
                Title = "Post Shared",
                Message = $"{sharedByUserName} shared your post",
                RelatedEntityType = "Post",
                RelatedEntityID = postId,
                FromUserID = sharedByUserId,
                ActionUrl = $"/Posts/Details/{postId}"
            };

            return await _dbService.CreateNotification(request);
        }

        /// <summary>
        /// Creates a notification when a new post is created by someone the user follows
        /// </summary>
        /// <param name="postId">ID of the new post</param>
        /// <param name="authorId">ID of the post author</param>
        /// <param name="followerId">ID of the follower to notify</param>
        /// <param name="authorName">Name of the post author</param>
        /// <param name="postTitle">Title of the post</param>
        /// <returns>ID of created notification or -1 if failed</returns>
        public async Task<int> CreateNewPostNotificationAsync(int postId, int authorId, int followerId, string authorName, string postTitle)
        {
            var request = new CreateNotificationRequest
            {
                UserID = followerId,
                Type = NotificationTypes.NewPost,
                Title = "New Post",
                Message = $"{authorName} published a new post: {(postTitle.Length > 50 ? postTitle.Substring(0, 50) + "..." : postTitle)}",
                RelatedEntityType = "Post",
                RelatedEntityID = postId,
                FromUserID = authorId,
                ActionUrl = $"/Posts/Details/{postId}"
            };

            return await _dbService.CreateNotification(request);
        }

        /// <summary>
        /// Creates notifications for all followers when a user creates a new post
        /// </summary>
        /// <param name="postId">ID of the new post</param>
        /// <param name="authorId">ID of the post author</param>
        /// <param name="authorName">Name of the post author</param>
        /// <param name="postTitle">Title of the post</param>
        /// <returns>Number of notifications created</returns>
        public async Task<int> CreateNewPostNotificationsForFollowersAsync(int postId, int authorId, string authorName, string postTitle)
        {
            try
            {
                // Get all followers of the author
                var followers = await _dbService.GetFollowedUserIdsAsync(authorId);
                int notificationsCreated = 0;

                foreach (var followerId in followers)
                {
                    var result = await CreateNewPostNotificationAsync(postId, authorId, followerId, authorName, postTitle);
                    if (result > 0)
                        notificationsCreated++;
                }

                return notificationsCreated;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error creating new post notifications for followers: {ex.Message}");
            }
        }

        /// <summary>
        /// Creates an admin message notification
        /// </summary>
        /// <param name="userId">ID of the user to notify</param>
        /// <param name="title">Title of the admin message</param>
        /// <param name="message">Content of the admin message</param>
        /// <param name="adminId">ID of the admin sending the message</param>
        /// <param name="actionUrl">Optional URL for the notification</param>
        /// <returns>ID of created notification or -1 if failed</returns>
        public async Task<int> CreateAdminMessageNotificationAsync(int userId, string title, string message, int adminId, string? actionUrl = null)
        {
            var request = new CreateNotificationRequest
            {
                UserID = userId,
                Type = NotificationTypes.AdminMessage,
                Title = title,
                Message = message,
                RelatedEntityType = "Admin",
                RelatedEntityID = adminId,
                FromUserID = adminId,
                ActionUrl = actionUrl
            };

            return await _dbService.CreateNotification(request);
        }

        /// <summary>
        /// Creates a system update notification
        /// </summary>
        /// <param name="userId">ID of the user to notify</param>
        /// <param name="title">Title of the system update</param>
        /// <param name="message">Content of the system update</param>
        /// <param name="actionUrl">Optional URL for the notification</param>
        /// <returns>ID of created notification or -1 if failed</returns>
        public async Task<int> CreateSystemUpdateNotificationAsync(int userId, string title, string message, string? actionUrl = null)
        {
            var request = new CreateNotificationRequest
            {
                UserID = userId,
                Type = NotificationTypes.SystemUpdate,
                Title = title,
                Message = message,
                RelatedEntityType = "System",
                ActionUrl = actionUrl
            };

            return await _dbService.CreateNotification(request);
        }

        /// <summary>
        /// Creates a security alert notification
        /// </summary>
        /// <param name="userId">ID of the user to notify</param>
        /// <param name="title">Title of the security alert</param>
        /// <param name="message">Content of the security alert</param>
        /// <param name="actionUrl">Optional URL for the notification</param>
        /// <returns>ID of created notification or -1 if failed</returns>
        public async Task<int> CreateSecurityAlertNotificationAsync(int userId, string title, string message, string? actionUrl = null)
        {
            var request = new CreateNotificationRequest
            {
                UserID = userId,
                Type = NotificationTypes.SecurityAlert,
                Title = title,
                Message = message,
                RelatedEntityType = "Security",
                ActionUrl = actionUrl
            };

            return await _dbService.CreateNotification(request);
        }

        /// <summary>
        /// Creates bulk notifications for multiple users
        /// </summary>
        /// <param name="userIds">List of user IDs to notify</param>
        /// <param name="type">Type of notification</param>
        /// <param name="title">Title of the notification</param>
        /// <param name="message">Content of the notification</param>
        /// <param name="fromUserId">Optional ID of the user who triggered the notification</param>
        /// <param name="actionUrl">Optional URL for the notification</param>
        /// <returns>Number of notifications created successfully</returns>
        public async Task<int> CreateBulkNotificationsAsync(List<int> userIds, string type, string title, string message, int? fromUserId = null, string? actionUrl = null)
        {
            int successCount = 0;
            
            foreach (var userId in userIds)
            {
                try
                {
                    var request = new CreateNotificationRequest
                    {
                        UserID = userId,
                        Type = type,
                        Title = title,
                        Message = message,
                        FromUserID = fromUserId,
                        ActionUrl = actionUrl
                    };

                    var result = await _dbService.CreateNotification(request);
                    if (result > 0)
                        successCount++;
                }
                catch
                {
                    // Continue with other notifications even if one fails
                    continue;
                }
            }

            return successCount;
        }

        /// <summary>
        /// Creates a notification with custom parameters
        /// </summary>
        /// <param name="request">Custom notification request</param>
        /// <returns>ID of created notification or -1 if failed</returns>
        public async Task<int> CreateCustomNotificationAsync(CreateNotificationRequest request)
        {
            return await _dbService.CreateNotification(request);
        }

        // Wrapper methods for existing DAL functionality

        /// <summary>
        /// Gets user notifications with pagination
        /// </summary>
        /// <param name="userId">ID of the user</param>
        /// <param name="page">Page number (1-based)</param>
        /// <param name="pageSize">Number of notifications per page</param>
        /// <returns>List of notifications</returns>
        public async Task<List<Notification>> GetUserNotificationsAsync(int userId, int page = 1, int pageSize = 20)
        {
            return await _dbService.GetUserNotifications(userId, page, pageSize);
        }

        /// <summary>
        /// Gets notification summary for a user
        /// </summary>
        /// <param name="userId">ID of the user</param>
        /// <returns>Notification summary</returns>
        public async Task<NotificationSummary> GetNotificationSummaryAsync(int userId)
        {
            return await _dbService.GetNotificationSummary(userId);
        }

        /// <summary>
        /// Gets unread notification count for a user
        /// </summary>
        /// <param name="userId">ID of the user</param>
        /// <returns>Number of unread notifications</returns>
        public async Task<int> GetUnreadNotificationCountAsync(int userId)
        {
            return await _dbService.GetUnreadNotificationCount(userId);
        }

        /// <summary>
        /// Marks a notification as read
        /// </summary>
        /// <param name="notificationId">ID of the notification</param>
        /// <param name="userId">ID of the user (for security)</param>
        /// <returns>True if successful</returns>
        public async Task<bool> MarkNotificationAsReadAsync(int notificationId, int userId)
        {
            return await _dbService.MarkNotificationAsRead(notificationId, userId);
        }

        /// <summary>
        /// Marks all notifications as read for a user
        /// </summary>
        /// <param name="userId">ID of the user</param>
        /// <returns>True if successful</returns>
        public async Task<bool> MarkAllNotificationsAsReadAsync(int userId)
        {
            return await _dbService.MarkAllNotificationsAsRead(userId);
        }
    }
}
