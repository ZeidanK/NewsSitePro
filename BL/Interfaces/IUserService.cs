using NewsSite.BL;

// ----------------------------------------------------------------------------------
// IUserService.cs
//
// This interface defines the contract for user-related operations in the business logic layer.
// It centralizes methods for creating, updating, searching, and managing users, as well as handling
// user profiles, passwords, bans, follows, and interests. By using an interface, the codebase supports
// dependency injection, testability, and separation of concerns, allowing different implementations for
// user management. All methods are asynchronous to ensure efficient, non-blocking operations, especially
// when interacting with databases or external services.
// ----------------------------------------------------------------------------------

namespace NewsSite.BL.Services
{
    public interface IUserService
    {
        Task<User?> GetUserByIdAsync(int id);
        Task<User?> GetUserByEmailAsync(string email);
        Task<bool> CreateUserAsync(User user);
        Task<bool> UpdateUserAsync(User user);
        Task<bool> UpdateUserProfileAsync(int userId, string username, string? bio = null);
        Task<bool> UpdateUserProfilePicAsync(int userId, string profilePicPath);
        Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword);
        Task<List<User>> SearchUsersAsync(string searchTerm, int pageNumber = 1, int pageSize = 10);
        Task<UserActivity> GetUserStatsAsync(int userId);
        Task<bool> DeactivateUserAsync(int userId);
        Task<bool> BanUserAsync(int userId, string reason, int durationDays, int adminId);
        Task<bool> UnbanUserAsync(int userId, int adminId);
        Task<FollowResult> ToggleUserFollowAsync(int currentUserId, int targetUserId);
        Task<bool> IsUserFollowingAsync(int currentUserId, int targetUserId);
        Task<List<UserInterest>> GetUserInterestsAsync(int userId);
    }
}
