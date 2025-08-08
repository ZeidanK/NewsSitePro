using System.ComponentModel.DataAnnotations;

// ----------------------------------------------------------------------------------
// UserBlock.cs
//
// This file defines models related to user blocking functionality in NewsSitePro.
// It includes the UserBlock class for representing block relationships, result/status
// models for block operations, statistics, mutual block checks, and request models for
// blocking/unblocking users. These models are used to manage and track user block actions
// and their outcomes throughout the application.
// ----------------------------------------------------------------------------------
namespace NewsSite.BL
{
    /// <summary>
    /// Represents a user block relationship
    /// </summary>
    public class UserBlock
    {
        // Unique identifier for the block record
        public int BlockID { get; set; }
        // ID of the user who performed the block
        public int BlockerUserID { get; set; }
        // ID of the user who was blocked
        public int BlockedUserID { get; set; }
        // Date and time when the block was created
        public DateTime BlockDate { get; set; }
        // Optional reason for blocking
        public string? Reason { get; set; }
        // Indicates if the block is currently active
        public bool IsActive { get; set; }
        // Timestamp when the block record was created
        public DateTime CreatedAt { get; set; }
        // Timestamp when the block record was last updated
        public DateTime UpdatedAt { get; set; }
        
        // Navigation properties for display
        // Blocked user's display information
        public string? BlockedUsername { get; set; }
        public string? BlockedUserEmail { get; set; }
        public string? BlockedUserProfilePicture { get; set; }
        // Blocker user's display information
        public string? BlockerUsername { get; set; }
        public string? BlockerUserEmail { get; set; }
    }

    /// <summary>
    /// Result of a block operation
    /// </summary>
    public class BlockResult
    {
        // Result status string (e.g., "success", "already_blocked", "not_found", "error")
        public string Result { get; set; } = string.Empty;
        // Message describing the outcome
        public string Message { get; set; } = string.Empty;
        // Indicates if the block operation was successful
        public bool Success => Result == "success";
        // Indicates if the user was already blocked
        public bool AlreadyBlocked => Result == "already_blocked";
        // Indicates if the target user was not found
        public bool NotFound => Result == "not_found";
        // Indicates if there was an error in the operation
        public bool HasError => Result == "error";
    }

    /// <summary>
    /// User block statistics
    /// </summary>
    public class UserBlockStats
    {
        // Number of users this user has blocked
        public int BlockedUsersCount { get; set; }
        // Number of users who have blocked this user
        public int BlockedByUsersCount { get; set; }
    }

    /// <summary>
    /// Mutual block check result
    /// </summary>
    public class MutualBlockCheck
    {
        // True if User1 has blocked User2
        public bool User1BlockedUser2 { get; set; }
        // True if User2 has blocked User1
        public bool User2BlockedUser1 { get; set; }
        // True if any block exists between the two users
        public bool AnyBlockExists { get; set; }
    }

    /// <summary>
    /// Request model for blocking a user
    /// </summary>
    public class BlockUserRequest
    {
        [Required]
        // ID of the user to be blocked
        public int BlockedUserID { get; set; }
        
        [StringLength(500)]
        // Optional reason for blocking
        public string? Reason { get; set; }
    }

    /// <summary>
    /// Request model for unblocking a user
    /// </summary>
    public class UnblockUserRequest
    {
        [Required]
        // ID of the user to be unblocked
        public int BlockedUserID { get; set; }
    }
}
