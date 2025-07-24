using Microsoft.AspNetCore.Mvc;
using NewsSite.BL;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace NewsSite.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CommentsController : ControllerBase
    {
        private readonly DBservices dbService;

        public CommentsController(DBservices dbService)
        {
            this.dbService = dbService;
        }

        [HttpGet("post/{postId}")]
        public async Task<IActionResult> GetCommentsByPost(int postId)
        {
            try
            {
                var comments = await dbService.GetCommentsByPostId(postId);
                return Ok(new { success = true, comments = comments });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateComment([FromBody] CreateCommentRequest request)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized(new { success = false, message = "Authentication required" });
            }

            try
            {
                var comment = new NewsSite.BL.Comment
                {
                    PostID = request.PostID,
                    UserID = userId.Value,
                    Content = request.Content,
                    ParentCommentID = request.ParentCommentID
                };

                bool success = await dbService.CreateComment(comment);
                if (success)
                {
                    return Ok(new { success = true, message = "Comment posted successfully" });
                }
                else
                {
                    return BadRequest(new { success = false, message = "Failed to post comment" });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPut("{commentId}")]
        public async Task<IActionResult> UpdateComment(int commentId, [FromBody] UpdateCommentRequest request)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized(new { success = false, message = "Authentication required" });
            }

            try
            {
                bool success = await dbService.UpdateComment(commentId, userId.Value, request.Content);
                if (success)
                {
                    return Ok(new { success = true, message = "Comment updated successfully" });
                }
                else
                {
                    return BadRequest(new { success = false, message = "Failed to update comment or unauthorized" });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpDelete("{commentId}")]
        public async Task<IActionResult> DeleteComment(int commentId)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized(new { success = false, message = "Authentication required" });
            }

            try
            {
                bool success = await dbService.DeleteComment(commentId, userId.Value);
                if (success)
                {
                    return Ok(new { success = true, message = "Comment deleted successfully" });
                }
                else
                {
                    return BadRequest(new { success = false, message = "Failed to delete comment or unauthorized" });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("count/{postId}")]
        public async Task<IActionResult> GetCommentsCount(int postId)
        {
            try
            {
                int count = await dbService.GetCommentsCount(postId);
                return Ok(new { success = true, count = count });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPost("like/{commentId}")]
        public IActionResult ToggleCommentLike(int commentId)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized(new { success = false, message = "Authentication required" });
            }

            try
            {
                // For now, we'll implement a simple toggle mechanism
                // You would need to implement ToggleCommentLike in DBservices
                // This is a placeholder implementation
                return Ok(new { success = true, action = "liked" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        private int? GetCurrentUserId()
        {
            try
            {
                // Try to get from JWT token in Authorization header first
                var authHeader = Request.Headers["Authorization"].FirstOrDefault();
                if (authHeader != null && authHeader.StartsWith("Bearer "))
                {
                    var token = authHeader.Substring("Bearer ".Length).Trim();
                    var handler = new JwtSecurityTokenHandler();
                    var jsonToken = handler.ReadJwtToken(token);
                    var userIdClaim = jsonToken.Claims.FirstOrDefault(c => c.Type == "userId" || c.Type == ClaimTypes.NameIdentifier);
                    if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int jwtUserId))
                    {
                        return jwtUserId;
                    }
                }

                // Fallback to cookie
                if (Request.Cookies.TryGetValue("jwtToken", out string? cookieToken) && !string.IsNullOrEmpty(cookieToken))
                {
                    var handler = new JwtSecurityTokenHandler();
                    var jsonToken = handler.ReadJwtToken(cookieToken);
                    var userIdClaim = jsonToken.Claims.FirstOrDefault(c => c.Type == "userId" || c.Type == ClaimTypes.NameIdentifier);
                    if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int cookieUserId))
                    {
                        return cookieUserId;
                    }
                }

                return null;
            }
            catch
            {
                return null;
            }
        }
    }
}
