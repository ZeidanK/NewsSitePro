using NewsSite.BL;

namespace NewsSite.BL.Services
{
    public interface IUserBlockService
    {
        Task<BlockResult> BlockUserAsync(int blockerUserID, int blockedUserID, string? reason = null);
        Task<BlockResult> UnblockUserAsync(int blockerUserID, int blockedUserID);
        Task<bool> IsUserBlockedAsync(int blockerUserID, int blockedUserID);
        Task<List<UserBlock>> GetBlockedUsersAsync(int blockerUserID, int pageNumber = 1, int pageSize = 20);
        Task<UserBlockStats> GetUserBlockStatsAsync(int userID);
        Task<MutualBlockCheck> CheckMutualBlockAsync(int userID1, int userID2);
    }

    public class UserBlockService : IUserBlockService
    {
        private readonly DBservices _dbService;

        public UserBlockService(DBservices dbService)
        {
            _dbService = dbService;
        }

        public async Task<BlockResult> BlockUserAsync(int blockerUserID, int blockedUserID, string? reason = null)
        {
            // Business logic validation
            if (blockerUserID <= 0)
            {
                return new BlockResult { Result = "error", Message = "Invalid blocker user ID" };
            }

            if (blockedUserID <= 0)
            {
                return new BlockResult { Result = "error", Message = "Invalid blocked user ID" };
            }

            if (blockerUserID == blockedUserID)
            {
                return new BlockResult { Result = "error", Message = "Cannot block yourself" };
            }

            // Validate reason length
            if (!string.IsNullOrEmpty(reason) && reason.Length > 500)
            {
                return new BlockResult { Result = "error", Message = "Reason cannot exceed 500 characters" };
            }

            try
            {
                return await _dbService.BlockUserAsync(blockerUserID, blockedUserID, reason);
            }
            catch (Exception ex)
            {
                return new BlockResult { Result = "error", Message = $"Failed to block user: {ex.Message}" };
            }
        }

        public async Task<BlockResult> UnblockUserAsync(int blockerUserID, int blockedUserID)
        {
            // Business logic validation
            if (blockerUserID <= 0)
            {
                return new BlockResult { Result = "error", Message = "Invalid blocker user ID" };
            }

            if (blockedUserID <= 0)
            {
                return new BlockResult { Result = "error", Message = "Invalid blocked user ID" };
            }

            try
            {
                return await _dbService.UnblockUserAsync(blockerUserID, blockedUserID);
            }
            catch (Exception ex)
            {
                return new BlockResult { Result = "error", Message = $"Failed to unblock user: {ex.Message}" };
            }
        }

        public async Task<bool> IsUserBlockedAsync(int blockerUserID, int blockedUserID)
        {
            if (blockerUserID <= 0 || blockedUserID <= 0)
            {
                return false;
            }

            try
            {
                return await _dbService.IsUserBlockedAsync(blockerUserID, blockedUserID);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<List<UserBlock>> GetBlockedUsersAsync(int blockerUserID, int pageNumber = 1, int pageSize = 20)
        {
            // Business logic validation
            if (blockerUserID <= 0)
            {
                return new List<UserBlock>();
            }

            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 20;

            try
            {
                return await _dbService.GetBlockedUsersAsync(blockerUserID, pageNumber, pageSize);
            }
            catch (Exception)
            {
                return new List<UserBlock>();
            }
        }

        public async Task<UserBlockStats> GetUserBlockStatsAsync(int userID)
        {
            if (userID <= 0)
            {
                return new UserBlockStats { BlockedUsersCount = 0, BlockedByUsersCount = 0 };
            }

            try
            {
                return await _dbService.GetUserBlockStatsAsync(userID);
            }
            catch (Exception)
            {
                return new UserBlockStats { BlockedUsersCount = 0, BlockedByUsersCount = 0 };
            }
        }

        public async Task<MutualBlockCheck> CheckMutualBlockAsync(int userID1, int userID2)
        {
            if (userID1 <= 0 || userID2 <= 0)
            {
                return new MutualBlockCheck { User1BlockedUser2 = false, User2BlockedUser1 = false, AnyBlockExists = false };
            }

            try
            {
                return await _dbService.CheckMutualBlockAsync(userID1, userID2);
            }
            catch (Exception)
            {
                return new MutualBlockCheck { User1BlockedUser2 = false, User2BlockedUser1 = false, AnyBlockExists = false };
            }
        }
    }
}
