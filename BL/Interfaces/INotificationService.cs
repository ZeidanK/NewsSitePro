using NewsSite.BL;

// ----------------------------------------------------------------------------------
// INotificationService.cs
//
// This interface defines the contract for notification-related operations in the business logic layer.
// It centralizes all methods for creating, retrieving, and managing notifications triggered by user actions
// (such as likes, comments, follows, shares, reposts, admin messages, and system alerts). By using an interface,
// the codebase supports dependency injection, testability, and separation of concerns, allowing different
// implementations for notification delivery and storage. All methods are asynchronous to ensure efficient,
// non-blocking operations, especially when interacting with databases or external services.
// ----------------------------------------------------------------------------------

namespace NewsSite.BL.Interfaces
{
    public interface INotificationService
    {
        Task<int> CreateLikeNotificationAsync(int postId, int likedByUserId, int postOwnerId, string likedByUserName);
        Task<int> CreateCommentNotificationAsync(int postId, int commentId, int commentByUserId, int postOwnerId, string commentByUserName);
        Task<int> CreateCommentNotificationAsync(int postId, int commentId, int commentedByUserId, int postOwnerId, string commentedByUserName, string commentContent);
        Task<int> CreateFollowNotificationAsync(int followerId, int followedId, string followerName);
        Task<int> CreateShareNotificationAsync(int postId, int sharedByUserId, int postOwnerId, string sharedByUserName);
        Task<int> CreateNewPostNotificationAsync(int postId, int authorId, int followerId, string authorName, string postTitle);
        Task<int> CreateNewPostNotificationsForFollowersAsync(int postId, int authorId, string authorName, string postTitle);
        Task<int> CreateCommentReplyNotificationAsync(int postId, int commentId, int parentCommentId, int repliedByUserId, int originalCommenterId, string repliedByUserName, string replyContent);
        Task<int> CreateAdminMessageNotificationAsync(int userId, string title, string message, int adminId, string? actionUrl = null);
        Task<int> CreateSystemUpdateNotificationAsync(int userId, string title, string message, string? actionUrl = null);
        Task<int> CreateSecurityAlertNotificationAsync(int userId, string title, string message, string? actionUrl = null);
        Task<int> CreateRepostNotificationAsync(int repostId, int repostedByUserId, int originalAuthorId, string repostedByUserName, string originalArticleTitle);
        Task<int> CreateRepostLikeNotificationAsync(int repostId, int likedByUserId, int repostOwnerId, string likedByUserName);
        Task<int> CreateRepostCommentNotificationAsync(int repostId, int commentId, int commentedByUserId, int repostOwnerId, string commentedByUserName, string commentContent);
        Task<int> CreateBulkNotificationsAsync(List<int> userIds, string type, string title, string message, int? fromUserId = null, string? actionUrl = null);
        Task<int> CreateCustomNotificationAsync(CreateNotificationRequest request);
        Task<List<Notification>> GetUserNotificationsAsync(int userId, int page = 1, int pageSize = 20);
        Task<NotificationSummary> GetNotificationSummaryAsync(int userId);
        Task<int> GetUnreadNotificationCountAsync(int userId);
        Task<bool> MarkNotificationAsReadAsync(int notificationId, int userId);
        Task<bool> MarkAllNotificationsAsReadAsync(int userId);
    }
}
