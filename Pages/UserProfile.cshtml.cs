using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NewsSite.BL;
using NewsSitePro.Models;
using System.IdentityModel.Tokens.Jwt;

namespace NewsSite.Pages
{
    public class UserProfileModel : PageModel
    {
        private readonly DBservices _dbService;

        public UserProfile? UserProfile { get; set; }
        public HeaderViewModel HeaderData { get; set; } = new HeaderViewModel();
        public bool IsOwnProfile { get; set; } = false;
        public bool IsFollowing { get; set; } = false;

        public UserProfileModel(DBservices dbService)
        {
            _dbService = dbService;
        }

        public IActionResult OnGet(int? userId = null)
        {
            try
            {
                var jwtToken = Request.Cookies["jwtToken"];
                User? currentUser = null;
                int? currentUserId = null;
                
                // Get current user from JWT token
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

                // If no userId provided, show current user's profile
                int targetUserId = userId ?? currentUserId ?? 0;
                if (targetUserId == 0)
                {
                    return RedirectToPage("/Login");
                }

                IsOwnProfile = currentUserId == targetUserId;

                // Set up header data
                HeaderData = new HeaderViewModel
                {
                    UserName = currentUser?.Name ?? "Guest",
                    NotificationCount = currentUser != null ? 3 : 0,
                    CurrentPage = "UserProfile",
                    user = currentUser
                };
                ViewData["HeaderData"] = HeaderData;

                // Get user basic info
                var user = _dbService.GetUser("", targetUserId, "");
                if (user == null)
                {
                    ViewData["ErrorMessage"] = "User not found";
                    return Page();
                }

                // Get user statistics
                UserActivity userStats;
                try
                {
                    userStats = _dbService.GetUserStats(targetUserId);
                }
                catch
                {
                    userStats = new UserActivity(); // Default empty stats
                }

                // Get user's recent posts
                List<NewsArticle> recentPosts;
                try
                {
                    recentPosts = _dbService.GetArticlesByUser(targetUserId, 1, 10);
                }
                catch
                {
                    recentPosts = new List<NewsArticle>(); // Default empty list
                }

                // Combine into UserProfile object
                UserProfile = new UserProfile
                {
                    UserID = user.Id,
                    Username = user.Name,
                    Email = user.Email,
                    Bio = user.Bio ?? "",
                    JoinDate = user.JoinDate,
                    IsAdmin = user.IsAdmin,
                    Activity = userStats,
                    RecentPosts = recentPosts
                };

                // Check if current user is following this user (placeholder)
                IsFollowing = false; // TODO: Implement follow system

                return Page();
            }
            catch (Exception ex)
            {
                // Log the exception (in production, you'd want proper logging)
                ViewData["ErrorMessage"] = "An error occurred while loading the profile. Please try again.";
                ViewData["ExceptionMessage"] = ex.Message; // For debugging, remove in production
                
                // Set up minimal header data for error display
                HeaderData = new HeaderViewModel
                {
                    UserName = "Guest",
                    NotificationCount = 0,
                    CurrentPage = "UserProfile",
                    user = null
                };
                ViewData["HeaderData"] = HeaderData;
                
                return Page();
            }
        }

        // API endpoint for follow/unfollow
        public IActionResult OnPostToggleFollow(int userId)
        {
            try
            {
                var isAuthenticated = User?.Identity?.IsAuthenticated ?? false;
                if (!isAuthenticated)
                {
                    return new JsonResult(new { success = false, message = "Not authenticated" });
                }

                // TODO: Implement actual follow/unfollow logic
                var isFollowing = !IsFollowing; // Toggle state

                return new JsonResult(new { 
                    success = true, 
                    isFollowing = isFollowing,
                    message = isFollowing ? "Now following user" : "Unfollowed user"
                });
            }
            catch (Exception)
            {
                return new JsonResult(new { success = false, message = "An error occurred" });
            }
        }

        // API endpoint for getting liked posts
        public async Task<IActionResult> OnGetGetLikedPosts()
        {
            try
            {
                var jwtToken = Request.Cookies["jwtToken"];
                if (string.IsNullOrEmpty(jwtToken))
                {
                    return new JsonResult(new { success = false, message = "Not authenticated" });
                }

                User? currentUser = null;
                try
                {
                    currentUser = new User().ExtractUserFromJWT(jwtToken);
                }
                catch
                {
                    return new JsonResult(new { success = false, message = "Invalid authentication" });
                }

                if (currentUser == null)
                {
                    return new JsonResult(new { success = false, message = "User not found" });
                }

                // Get liked posts for current user
                var likedPosts = await _dbService.GetLikedArticlesByUser(currentUser.Id, 1, 20);
                
                return new JsonResult(new { 
                    success = true, 
                    posts = likedPosts.Select(p => new {
                        articleID = p.ArticleID,
                        title = p.Title,
                        content = p.Content,
                        imageURL = p.ImageURL,
                        sourceURL = p.SourceURL,
                        sourceName = p.SourceName,
                        category = p.Category,
                        publishDate = p.PublishDate,
                        username = p.Username,
                        likesCount = p.LikesCount,
                        viewsCount = p.ViewsCount
                    })
                });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = "Error loading liked posts: " + ex.Message });
            }
        }

        // API endpoint for getting saved posts
        public async Task<IActionResult> OnGetGetSavedPosts()
        {
            try
            {
                var jwtToken = Request.Cookies["jwtToken"];
                if (string.IsNullOrEmpty(jwtToken))
                {
                    return new JsonResult(new { success = false, message = "Not authenticated" });
                }

                User? currentUser = null;
                try
                {
                    currentUser = new User().ExtractUserFromJWT(jwtToken);
                }
                catch
                {
                    return new JsonResult(new { success = false, message = "Invalid authentication" });
                }

                if (currentUser == null)
                {
                    return new JsonResult(new { success = false, message = "User not found" });
                }

                // Get saved posts for current user
                var savedPosts = await _dbService.GetSavedArticlesByUser(currentUser.Id, 1, 20);
                
                return new JsonResult(new { 
                    success = true, 
                    posts = savedPosts.Select(p => new {
                        articleID = p.ArticleID,
                        title = p.Title,
                        content = p.Content,
                        imageURL = p.ImageURL,
                        sourceURL = p.SourceURL,
                        sourceName = p.SourceName,
                        category = p.Category,
                        publishDate = p.PublishDate,
                        username = p.Username,
                        likesCount = p.LikesCount,
                        viewsCount = p.ViewsCount
                    })
                });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = "Error loading saved posts: " + ex.Message });
            }
        }
    }
}
