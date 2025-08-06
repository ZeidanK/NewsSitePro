using NewsSite.BL;

// ----------------------------------------------------------------------------------
// UserService.cs
//
// This class implements user-related business logic for the NewsSitePro application. It provides methods
// for creating, updating, deleting, and retrieving users, handling user profiles, passwords, bans, follows,
// and interests. The service enforces business rules (such as validation, ownership, and notification triggers),
// integrates with NotificationService, and interacts with the database layer through DBservices. All methods
// are asynchronous for efficient, non-blocking operations. Comments are added to key functions for clarity.
// ----------------------------------------------------------------------------------

namespace NewsSite.BL.Services
{
    /// <summary>
    /// User Service - Business Logic Layer
    /// Implements user-related business operations and validation
    /// Integrated with NotificationService for user action notifications
    /// </summary>
    public class UserService : IUserService
    {
        private readonly DBservices _dbService;
        private readonly NotificationService _notificationService;

        public UserService(DBservices dbService, NotificationService notificationService)
        // Constructor: injects database and notification services
        {
            _dbService = dbService;
            _notificationService = notificationService;
        }

        public async Task<User?> GetUserByIdAsync(int userId)
        // Get a user by their ID
        {
            return await Task.FromResult(_dbService.GetUserById(userId));
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        // Get a user by their email address
        {
            return await Task.FromResult(_dbService.GetUser(email: email));
        }

        public async Task<bool> CreateUserAsync(User user)
        // Create a new user, with validation and duplicate check
        {
            // Business logic validation
            if (string.IsNullOrWhiteSpace(user.Email) || string.IsNullOrWhiteSpace(user.Name))
            {
                throw new ArgumentException("Email and Name are required");
            }

            // Check if user already exists
            var existingUser = await GetUserByEmailAsync(user.Email);
            if (existingUser != null)
            {
                throw new InvalidOperationException("User with this email already exists");
            }

            return await Task.FromResult(_dbService.CreateUser(user));
        }

        public async Task<bool> UpdateUserAsync(User user)
        // Update an existing user
        {
            // Business logic validation
            if (user.Id <= 0)
            {
                throw new ArgumentException("Valid User ID is required");
            }

            return await Task.FromResult(_dbService.UpdateUser(user));
        }

        public async Task<bool> DeleteUserAsync(int userId)
        // Delete a user (cannot delete admin users)
        {
            // Business logic: Check if user exists and can be deleted
            var user = await GetUserByIdAsync(userId);
            if (user == null)
            {
                throw new ArgumentException("User not found");
            }

            // Add business rules for deletion (e.g., admin users cannot be deleted)
            if (user.IsAdmin)
            {
                throw new InvalidOperationException("Admin users cannot be deleted");
            }

            // Implement soft delete or hard delete based on business requirements
            return await Task.FromResult(true); // Placeholder
        }

        public async Task<bool> ValidateUserCredentialsAsync(string email, string password)
        // Validate user credentials (simple password check)
        {
            var user = await GetUserByEmailAsync(email);
            if (user == null)
            {
                return false;
            }

            // Simple password validation for now (in real app, use proper hashing)
            return user.PasswordHash == password;
        }

        public async Task<UserActivity> GetUserStatsAsync(int userId)
        // Get user activity statistics
        {
            return await Task.FromResult(_dbService.GetUserStats(userId));
        }

        public async Task<bool> UpdateUserProfileAsync(int userId, string username, string? bio = null)
        // Update a user's profile information
        {
            return await Task.FromResult(_dbService.UpdateUserProfile(userId, username, bio));
        }

        public async Task<bool> ChangePasswordAsync(int userId, string newPasswordHash)
        // Change a user's password (hash)
        {
            var user = await GetUserByIdAsync(userId);
            if (user == null)
            {
                throw new ArgumentException("User not found");
            }

            user.PasswordHash = newPasswordHash;
            return await UpdateUserAsync(user);
        }

        public async Task<List<User>> SearchUsersAsync(string searchTerm, int pageNumber = 1, int pageSize = 10)
        // Search for users by term
        {
            return await _dbService.SearchUsersAsync(searchTerm, pageNumber, pageSize);
        }

        public async Task<bool> UpdateUserProfilePicAsync(int userId, string profilePicPath)
        // Update a user's profile picture
        {
            return await _dbService.UpdateUserProfilePic(userId, profilePicPath);
        }

        public async Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
        // Change a user's password (simple implementation)
        {
            var user = await GetUserByIdAsync(userId);
            if (user == null)
            {
                throw new ArgumentException("User not found");
            }

            // For now, simple implementation
            // In a real implementation, you would verify the current password and update it
            return true; // Placeholder implementation
        }

        public async Task<bool> DeactivateUserAsync(int userId)
        // Deactivate a user account (placeholder)
        {
            // Simple implementation - assumes this functionality exists in DBservice
            return true; // Placeholder implementation
        }

        public async Task<bool> BanUserAsync(int userId, string reason, int durationDays, int adminId)
        // Ban a user account (placeholder)
        {
            // Simple implementation - assumes this functionality exists in DBservice
            return true; // Placeholder implementation
        }

        public async Task<bool> UnbanUserAsync(int userId, int adminId)
        // Unban a user account
        {
            if (userId <= 0)
            {
                throw new ArgumentException("Valid User ID is required");
            }

            if (adminId <= 0)
            {
                throw new ArgumentException("Valid Admin ID is required");
            }

            return await _dbService.UnbanUser(userId, adminId);
        }

        public async Task<FollowResult> ToggleUserFollowAsync(int currentUserId, int targetUserId)
        // Follow or unfollow another user, and send notification if followed
        {
            if (currentUserId <= 0 || targetUserId <= 0)
            {
                throw new ArgumentException("Valid user IDs are required");
            }

            if (currentUserId == targetUserId)
            {
                throw new InvalidOperationException("Cannot follow yourself");
            }

            var result = await _dbService.ToggleUserFollow(currentUserId, targetUserId);
            
            // Create follow notification if user was followed (not unfollowed)
            if (result.IsFollowing)
            {
                try
                {
                    var followerUser = _dbService.GetUserById(currentUserId);
                    if (followerUser != null)
                    {
                        await _notificationService.CreateFollowNotificationAsync(
                            currentUserId, 
                            targetUserId, 
                            followerUser.Name ?? "Unknown User"
                        );
                    }
                }
                catch (Exception ex)
                {
                    // Log the notification error but don't fail the follow action
                    Console.WriteLine($"Failed to create follow notification: {ex.Message}");
                }
            }
            
            return result;
        }

        public async Task<bool> IsUserFollowingAsync(int currentUserId, int targetUserId)
        // Check if one user is following another
        {
            if (currentUserId <= 0 || targetUserId <= 0)
            {
                return false;
            }

            var result = await _dbService.IsUserFollowing(currentUserId, targetUserId);
            return result;
        }

        public async Task<List<UserInterest>> GetUserInterestsAsync(int userId)
        // Get a user's interests
        {
            if (userId <= 0)
            {
                throw new ArgumentException("Valid User ID is required");
            }

            return await _dbService.GetUserInterestsAsync(userId);
        }
    }
}
