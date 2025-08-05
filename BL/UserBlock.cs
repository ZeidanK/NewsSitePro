using System.ComponentModel.DataAnnotations;

namespace NewsSite.BL
{
    /// <summary>
    /// Represents a user block relationship
    /// </summary>
    public class UserBlock
    {
        public int BlockID { get; set; }
        public int BlockerUserID { get; set; }
        public int BlockedUserID { get; set; }
        public DateTime BlockDate { get; set; }
        public string? Reason { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        // Navigation properties for display
        public string? BlockedUsername { get; set; }
        public string? BlockedUserEmail { get; set; }
        public string? BlockedUserProfilePicture { get; set; }
        public string? BlockerUsername { get; set; }
        public string? BlockerUserEmail { get; set; }
    }

    /// <summary>
    /// Result of a block operation
    /// </summary>
    public class BlockResult
    {
        public string Result { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public bool Success => Result == "success";
        public bool AlreadyBlocked => Result == "already_blocked";
        public bool NotFound => Result == "not_found";
        public bool HasError => Result == "error";
    }

    /// <summary>
    /// User block statistics
    /// </summary>
    public class UserBlockStats
    {
        public int BlockedUsersCount { get; set; }
        public int BlockedByUsersCount { get; set; }
    }

    /// <summary>
    /// Mutual block check result
    /// </summary>
    public class MutualBlockCheck
    {
        public bool User1BlockedUser2 { get; set; }
        public bool User2BlockedUser1 { get; set; }
        public bool AnyBlockExists { get; set; }
    }

    /// <summary>
    /// Request model for blocking a user
    /// </summary>
    public class BlockUserRequest
    {
        [Required]
        public int BlockedUserID { get; set; }
        
        [StringLength(500)]
        public string? Reason { get; set; }
    }

    /// <summary>
    /// Request model for unblocking a user
    /// </summary>
    public class UnblockUserRequest
    {
        [Required]
        public int BlockedUserID { get; set; }
    }
}
