/**
 * RepostController.cs
 * 
 * Controller for repost functionality - handles creating, viewing, liking, and commenting on reposts.
 * Integrates with RepostService and CommentService for business logic operations.
 * Supports notification creation for repost interactions.
 * 
 * Author: System
 * Date: 2025-01-27
 */

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using NewsSite.BL;
using NewsSite.BL.Services;
using NewsSite.BL.Interfaces;
using System.Security.Claims;

namespace NewsSite.Controllers
{
    /// <summary>
    /// Controller for repost-related operations
    /// Handles creating reposts, viewing repost details, managing likes and comments
    /// </summary>
    [Authorize]
    public class RepostController : Controller
    {
        private readonly IRepostService _repostService;
        private readonly ICommentService _commentService;
        private readonly NotificationService _notificationService;
        private readonly DBservices _dbService;

        public RepostController(
            IRepostService repostService, 
            ICommentService commentService,
            NotificationService notificationService,
            DBservices dbService)
        {
            _repostService = repostService;
            _commentService = commentService;
            _notificationService = notificationService;
            _dbService = dbService;
        }

        /// <summary>
        /// Creates a new repost
        /// </summary>
        /// <param name="request">Repost creation request</param>
        /// <returns>JSON result with success/failure status</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromBody] CreateRepostRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Json(new { success = false, message = "Invalid request data" });
                }

                var userId = GetCurrentUserId();
                if (userId == 0)
                {
                    return Json(new { success = false, message = "User not authenticated" });
                }

                request.UserID = userId;

                // Create the repost
                var result = await _repostService.CreateRepostAsync(request);
                if (result.Success)
                {
                    return Json(new { success = true, repostId = result.RepostID, message = result.Message });
                }

                return Json(new { success = false, message = "Failed to create repost" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Toggles repost on an article (repost/unrepost)
        /// </summary>
        /// <param name="articleId">Article ID to repost/unrepost</param>
        /// <returns>JSON result with action performed</returns>
        [HttpPost]
        [Route("api/repost/toggle")]
        public IActionResult Toggle([FromBody] int articleId)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId <= 0)
                {
                    return Json(new { success = false, message = "User not authenticated" });
                }

                var result = _dbService.ToggleRepost(articleId, userId);
                
                return Json(new { 
                    success = true, 
                    action = result,
                    message = result == "reposted" ? "Article reposted successfully" : "Repost removed successfully"
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Gets repost details with comments
        /// </summary>
        /// <param name="id">Repost ID</param>
        /// <returns>Repost details view</returns>
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var repost = await _dbService.GetRepostByIdAsync(id);
                if (repost == null || repost.IsDeleted)
                {
                    return NotFound("Repost not found");
                }

                var userId = GetCurrentUserId();

                // Get repost statistics
                repost.LikesCount = await _dbService.GetRepostLikeCountAsync(id);
                repost.CommentsCount = await _dbService.GetRepostCommentCountAsync(id);
                
                if (userId > 0)
                {
                    repost.IsLikedByUser = await _dbService.HasUserLikedRepostAsync(id, userId);
                }

                // Get comments
                var comments = await _dbService.GetRepostCommentsAsync(id);

                ViewBag.Comments = comments;
                ViewBag.CurrentUserId = userId;
                
                return View(repost);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = ex.Message;
                return View("Error");
            }
        }

        /// <summary>
        /// Toggles like on a repost
        /// </summary>
        /// <param name="repostId">ID of the repost</param>
        /// <returns>JSON result with like status</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleLike(int repostId)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == 0)
                {
                    return Json(new { success = false, message = "User not authenticated" });
                }

                var result = await _dbService.ToggleRepostLikeAsync(repostId, userId);
                var likeCount = await _dbService.GetRepostLikeCountAsync(repostId);

                return Json(new { 
                    success = result.Success, 
                    liked = result.IsLiked,
                    likeCount = likeCount,
                    message = result.Message
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Creates a comment on a repost
        /// </summary>
        /// <param name="repostComment">Comment data</param>
        /// <returns>JSON result with success status</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment([FromBody] CreateRepostCommentRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Json(new { success = false, message = "Invalid comment data" });
                }

                var userId = GetCurrentUserId();
                if (userId == 0)
                {
                    return Json(new { success = false, message = "User not authenticated" });
                }

                request.UserID = userId;

                var commentId = await _dbService.CreateRepostCommentAsync(request);
                if (commentId > 0)
                {
                    return Json(new { success = true, message = "Comment added successfully!" });
                }

                return Json(new { success = false, message = "Failed to add comment" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Deletes a repost (soft delete)
        /// </summary>
        /// <param name="id">Repost ID</param>
        /// <returns>JSON result with success status</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == 0)
                {
                    return Json(new { success = false, message = "User not authenticated" });
                }

                var success = await _dbService.DeleteRepostAsync(id, userId);
                if (success)
                {
                    return Json(new { success = true, message = "Repost deleted successfully!" });
                }

                return Json(new { success = false, message = "Failed to delete repost or unauthorized" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Gets user's reposts for profile page
        /// </summary>
        /// <param name="userId">User ID (optional, defaults to current user)</param>
        /// <param name="page">Page number</param>
        /// <param name="pageSize">Items per page</param>
        /// <returns>JSON result with reposts</returns>
        [HttpGet]
        public async Task<IActionResult> GetUserReposts(int? userId = null, int page = 1, int pageSize = 10)
        {
            try
            {
                var targetUserId = userId ?? GetCurrentUserId();
                if (targetUserId == 0)
                {
                    return Json(new { success = false, message = "Invalid user" });
                }

                var reposts = await _dbService.GetUserRepostsAsync(targetUserId, page, pageSize);
                
                return Json(new { 
                    success = true, 
                    reposts = reposts,
                    page = page,
                    pageSize = pageSize
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Updates a repost comment
        /// </summary>
        /// <param name="commentId">Comment ID</param>
        /// <param name="content">New content</param>
        /// <returns>JSON result with success status</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateComment(int commentId, [FromBody] string content)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == 0)
                {
                    return Json(new { success = false, message = "User not authenticated" });
                }

                var success = await _dbService.UpdateRepostComment(commentId, content);
                if (success)
                {
                    return Json(new { success = true, message = "Comment updated successfully!" });
                }

                return Json(new { success = false, message = "Failed to update comment or unauthorized" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Deletes a repost comment
        /// </summary>
        /// <param name="commentId">Comment ID</param>
        /// <returns>JSON result with success status</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteComment(int commentId)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == 0)
                {
                    return Json(new { success = false, message = "User not authenticated" });
                }

                var success = await _dbService.DeleteRepostComment(commentId);
                if (success)
                {
                    return Json(new { success = true, message = "Comment deleted successfully!" });
                }

                return Json(new { success = false, message = "Failed to delete comment or unauthorized" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Helper method to get current user ID from claims
        /// </summary>
        /// <returns>Current user ID or 0 if not found</returns>
        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst("id") ?? User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                return userId;
            }
            return 0;
        }
    }
}
