using System.ComponentModel.DataAnnotations;

namespace NewsSite.BL
{
    public class Notification
    {
        public int ID { get; set; }
        // Add NotificationID as an alias for ID for compatibility
        public int NotificationID { get => ID; set => ID = value; }
        public int UserID { get; set; }
        public string Type { get; set; } = string.Empty; // Like, Comment, Follow, NewPost, AdminMessage
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? RelatedEntityType { get; set; } // Post, User, etc.
        public int? RelatedEntityID { get; set; }
        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string? FromUserName { get; set; }
        public int? FromUserID { get; set; }
        public string? ActionUrl { get; set; } // URL to navigate when notification is clicked
    }

    public class NotificationPreference
    {
        public int ID { get; set; }
        public int UserID { get; set; }
        public string NotificationType { get; set; } = string.Empty;
        public bool IsEnabled { get; set; } = true;
        public bool EmailNotification { get; set; } = false;
        public bool PushNotification { get; set; } = true;
    }

    public class NotificationSummary
    {
        public int TotalUnread { get; set; }
        public int TotalCount { get; set; } // Add this property for total notifications
        public int UnreadCount { get => TotalUnread; set => TotalUnread = value; } // Alias for compatibility
        public List<Notification> RecentNotifications { get; set; } = new List<Notification>();
        public Dictionary<string, int> UnreadByType { get; set; } = new Dictionary<string, int>();
    }

    public class CreateNotificationRequest
    {
        [Required]
        public int UserID { get; set; }
        
        [Required]
        public string Type { get; set; } = string.Empty;
        
        [Required]
        public string Title { get; set; } = string.Empty;
        
        [Required]
        public string Message { get; set; } = string.Empty;
        
        public string? RelatedEntityType { get; set; }
        public int? RelatedEntityID { get; set; }
        public int? FromUserID { get; set; }
        public string? ActionUrl { get; set; }
    }

    public class UpdateNotificationPreferencesRequest
    {
        public int UserID { get; set; }
        public Dictionary<string, NotificationPreferenceSettings> Preferences { get; set; } = new Dictionary<string, NotificationPreferenceSettings>();
    }

    public class NotificationPreferenceSettings
    {
        public bool IsEnabled { get; set; } = true;
        public bool EmailNotification { get; set; } = false;
        public bool PushNotification { get; set; } = true;
    }

    // Notification types enum for consistency
    public static class NotificationTypes
    {
        public const string Like = "Like";
        public const string Comment = "Comment";
        public const string Follow = "Follow";
        public const string NewPost = "NewPost";
        public const string PostShare = "PostShare";
        public const string AdminMessage = "AdminMessage";
        public const string SystemUpdate = "SystemUpdate";
        public const string AccountUpdate = "AccountUpdate";
        public const string SecurityAlert = "SecurityAlert";
    }
}
