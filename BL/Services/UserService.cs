using NewsSite.BL;

namespace NewsSite.BL.Services
{
    /// <summary>
    /// User Service - Business Logic Layer
    /// Implements user-related business operations and validation
    /// </summary>
    public class UserService : IUserService
    {
        private readonly DBservices _dbService;

        public UserService(DBservices dbService)
        {
            _dbService = dbService;
        }

        public async Task<User?> GetUserByIdAsync(int userId)
        {
            return await Task.FromResult(_dbService.GetUserById(userId));
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await Task.FromResult(_dbService.GetUser(email: email));
        }

        public async Task<bool> CreateUserAsync(User user)
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
        {
            // Business logic validation
            if (user.Id <= 0)
            {
                throw new ArgumentException("Valid User ID is required");
            }

            return await Task.FromResult(_dbService.UpdateUser(user));
        }

        public async Task<bool> DeleteUserAsync(int userId)
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
        {
            return await Task.FromResult(_dbService.GetUserStats(userId));
        }

        public async Task<bool> UpdateUserProfileAsync(int userId, string username, string? bio = null)
        {
            return await Task.FromResult(_dbService.UpdateUserProfile(userId, username, bio));
        }

        public async Task<bool> ChangePasswordAsync(int userId, string newPasswordHash)
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
        {
            return await _dbService.SearchUsersAsync(searchTerm, pageNumber, pageSize);
        }

        public async Task<bool> UpdateUserProfilePicAsync(int userId, string profilePicPath)
        {
            // Note: This assumes DBservice method exists or returns a default value
            return true; // Placeholder implementation
        }

        public async Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
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
        {
            // Simple implementation - assumes this functionality exists in DBservice
            return true; // Placeholder implementation
        }

        public async Task<bool> BanUserAsync(int userId, string reason, int durationDays, int adminId)
        {
            // Simple implementation - assumes this functionality exists in DBservice
            return true; // Placeholder implementation
        }

        public async Task<bool> UnbanUserAsync(int userId)
        {
            // Simple implementation - assumes this functionality exists in DBservice
            return true; // Placeholder implementation
        }
    }
}
