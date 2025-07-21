using NewsSite.BL;

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
        Task<bool> UnbanUserAsync(int userId);
    }
}
