using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NewsSitePro.Models;
using Microsoft.AspNetCore.Identity;
using NewsSite.BL;
using NewsSite.Models;

namespace NewsSite.Pages
{
    // [Authorize] // Temporarily removed to fix 401 error - we'll handle auth in the page
    public class PostModel : PageModel
    {
        private readonly DBservices _dbService;

        public HeaderViewModel HeaderData { get; set; } = new HeaderViewModel();
        public NewsArticle? PostData { get; set; }
        public List<Comment> Comments { get; set; } = new List<Comment>();
        public bool IsIndividualPost { get; set; } = false;

        public PostModel()
        {
            _dbService = new DBservices();
        }

        public async Task<IActionResult> OnGet(int? id = null)
        {
            try
            {
                var jwtToken = Request.Cookies["jwtToken"];
                User? currentUser = null;
                int? currentUserId = null;
                
                // Get current user from JWT token (same pattern as other working pages)
                if (!string.IsNullOrEmpty(jwtToken))
                {
                    try
                    {
                        currentUser = new User().ExtractUserFromJWT(jwtToken);
                        currentUserId = currentUser?.Id;
                    }
                    catch
                    {
                        // Invalid token, treat as not authenticated
                        currentUser = null;
                        currentUserId = null;
                    }
                }
                
                HeaderData = new HeaderViewModel
                {
                    UserName = currentUser?.Name ?? "Guest",
                    NotificationCount = currentUser != null ? 3 : 0,
                    CurrentPage = id.HasValue ? "Post" : "NewsFeed",
                    user = currentUser
                };
                
                ViewData["HeaderData"] = HeaderData;

                // If ID is provided, load individual post
                if (id.HasValue && id.Value > 0)
                {
                    try
                    {
                        IsIndividualPost = true;
                        PostData = await _dbService.GetNewsArticleById(id.Value);
                        
                        if (PostData == null)
                        {
                            ViewData["ErrorMessage"] = "Post not found.";
                            return NotFound();
                        }

                        // Get comments for this post
                        Comments = await _dbService.GetCommentsByPostId(id.Value) ?? new List<Comment>();

                        // Check follow status for the post author if user is logged in
                        if (currentUserId.HasValue && PostData.UserID != currentUserId.Value)
                        {
                            var isFollowing = await _dbService.IsUserFollowing(currentUserId.Value, PostData.UserID);
                            ViewData["IsFollowing_" + PostData.UserID] = isFollowing;
                        }

                        // Increment view count
                        _dbService.RecordArticleView(id.Value, currentUserId);
                    }
                    catch (Exception ex)
                    {
                        // Log error in production
                        ViewData["ErrorMessage"] = "Error loading post: " + ex.Message;
                        IsIndividualPost = false;
                        return Page();
                    }
                }
                else
                {
                    IsIndividualPost = false;
                }

                return Page();
            }
            catch (Exception ex)
            {
                ViewData["ErrorMessage"] = "An unexpected error occurred: " + ex.Message;
                return Page();
            }
        }

        // API method to add a comment
        public async Task<IActionResult> OnPostAddComment([FromBody] AddCommentRequest request)
        {
            try
            {
                var jwtToken = Request.Cookies["jwtToken"];
                User? currentUser = null;
                
                // Get current user from JWT token (same pattern as OnGet)
                if (!string.IsNullOrEmpty(jwtToken))
                {
                    try
                    {
                        currentUser = new User().ExtractUserFromJWT(jwtToken);
                    }
                    catch
                    {
                        return new JsonResult(new { success = false, message = "Please log in to comment" });
                    }
                }

                if (currentUser == null || currentUser.Id == 0)
                {
                    return new JsonResult(new { success = false, message = "Please log in to comment" });
                }

                var comment = new Comment
                {
                    PostID = request.PostId,
                    UserID = currentUser.Id,
                    Content = request.Content,
                    CreatedAt = DateTime.Now
                };

                var success = await _dbService.CreateComment(comment);
                
                if (success)
                {
                    return new JsonResult(new { success = true, message = "Comment added successfully" });
                }
                else 
                {
                    return BadRequest(new { success = false, message = "Failed to add comment" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "An error occurred: " + ex.Message });
            }
        }
    }

    public class AddCommentRequest
    {
        public int PostId { get; set; }
        public string Content { get; set; } = string.Empty;
    }
}