/**
 * UserBlockController.cs
 * Purpose: Handles user blocking operations and block management
 * Responsibilities: Block/unblock users, view blocked users, check block status
 * Architecture: Uses UserBlockService from BL layer for business logic and data operations
 */

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NewsSite.BL;
using NewsSite.BL.Services;

namespace NewsSite.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserBlockController : ControllerBase
    {
        private readonly IUserBlockService _userBlockService;

        public UserBlockController(IUserBlockService userBlockService)
        {
            _userBlockService = userBlockService;
        }

        // Helper method to get current user ID
        private int? GetCurrentUserId()
        {
            return BL.User.GetCurrentUserId(Request, User);
        }

        /// <summary>
        /// Block a user
        /// </summary>
        [HttpPost("block")]
        public async Task<IActionResult> BlockUser([FromBody] BlockUserRequest request)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (!currentUserId.HasValue)
                {
                    return Unauthorized(new { success = false, message = "User not authenticated" });
                }

                var result = await _userBlockService.BlockUserAsync(currentUserId.Value, request.BlockedUserID, request.Reason);
                
                if (result.Success)
                {
                    return Ok(new { success = true, message = result.Message });
                }
                else if (result.AlreadyBlocked)
                {
                    return BadRequest(new { success = false, message = result.Message });
                }
                else
                {
                    return StatusCode(500, new { success = false, message = result.Message });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error blocking user: " + ex.Message });
            }
        }

        /// <summary>
        /// Unblock a user
        /// </summary>
        [HttpPost("unblock")]
        public async Task<IActionResult> UnblockUser([FromBody] UnblockUserRequest request)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (!currentUserId.HasValue)
                {
                    return Unauthorized(new { success = false, message = "User not authenticated" });
                }

                var result = await _userBlockService.UnblockUserAsync(currentUserId.Value, request.BlockedUserID);
                
                if (result.Success)
                {
                    return Ok(new { success = true, message = result.Message });
                }
                else if (result.NotFound)
                {
                    return NotFound(new { success = false, message = result.Message });
                }
                else
                {
                    return StatusCode(500, new { success = false, message = result.Message });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error unblocking user: " + ex.Message });
            }
        }

        /// <summary>
        /// Check if a user is blocked
        /// </summary>
        [HttpGet("is-blocked/{blockedUserID}")]
        public async Task<IActionResult> IsUserBlocked(int blockedUserID)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (!currentUserId.HasValue)
                {
                    return Unauthorized(new { success = false, message = "User not authenticated" });
                }

                var isBlocked = await _userBlockService.IsUserBlockedAsync(currentUserId.Value, blockedUserID);
                
                return Ok(new { success = true, isBlocked });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error checking block status: " + ex.Message });
            }
        }

        /// <summary>
        /// Get list of blocked users
        /// </summary>
        [HttpGet("blocked-users")]
        public async Task<IActionResult> GetBlockedUsers([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (!currentUserId.HasValue)
                {
                    return Unauthorized(new { success = false, message = "User not authenticated" });
                }

                var blockedUsers = await _userBlockService.GetBlockedUsersAsync(currentUserId.Value, page, pageSize);
                
                return Ok(new { success = true, blockedUsers, page, pageSize });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error retrieving blocked users: " + ex.Message });
            }
        }

        /// <summary>
        /// Get user block statistics
        /// </summary>
        [HttpGet("stats")]
        public async Task<IActionResult> GetUserBlockStats()
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (!currentUserId.HasValue)
                {
                    return Unauthorized(new { success = false, message = "User not authenticated" });
                }

                var stats = await _userBlockService.GetUserBlockStatsAsync(currentUserId.Value);
                
                return Ok(new { success = true, stats });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error retrieving block stats: " + ex.Message });
            }
        }

        /// <summary>
        /// Check mutual block status between two users (for admin/system use)
        /// </summary>
        [HttpGet("mutual-check/{userID1}/{userID2}")]
        public async Task<IActionResult> CheckMutualBlock(int userID1, int userID2)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (!currentUserId.HasValue)
                {
                    return Unauthorized(new { success = false, message = "User not authenticated" });
                }

                // Only allow users to check their own blocks or admin users
                // For now, allow checking if current user is one of the participants
                if (currentUserId.Value != userID1 && currentUserId.Value != userID2)
                {
                    return Forbid("You can only check blocks involving yourself");
                }

                var mutualCheck = await _userBlockService.CheckMutualBlockAsync(userID1, userID2);
                
                return Ok(new { success = true, mutualCheck });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error checking mutual block: " + ex.Message });
            }
        }

        /// <summary>
        /// Block user from post (called from post report functionality)
        /// </summary>
        [HttpPost("block-from-post/{postUserID}")]
        public async Task<IActionResult> BlockUserFromPost(int postUserID, [FromBody] BlockUserFromPostRequest? request = null)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (!currentUserId.HasValue)
                {
                    return Unauthorized(new { success = false, message = "User not authenticated" });
                }

                var reason = request?.Reason ?? "Blocked from post";
                var result = await _userBlockService.BlockUserAsync(currentUserId.Value, postUserID, reason);
                
                if (result.Success)
                {
                    return Ok(new { success = true, message = "User blocked successfully. You will no longer see their posts." });
                }
                else if (result.AlreadyBlocked)
                {
                    return Ok(new { success = true, message = "User is already blocked." });
                }
                else
                {
                    return StatusCode(500, new { success = false, message = result.Message });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error blocking user: " + ex.Message });
            }
        }
    }

    /// <summary>
    /// Request model for blocking user from post
    /// </summary>
    public class BlockUserFromPostRequest
    {
        public string? Reason { get; set; }
    }
}
