using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NewsSite.BL;
using NewsSite.Models;
using System.IdentityModel.Tokens.Jwt;

namespace NewsSite.Pages
{
    public class UserProfileModel : PageModel
    {
        private readonly DBservices _dbService;

        public UserProfile? UserProfile { get; set; }
        public NewsSitePro.Models.HeaderViewModel HeaderData { get; set; } = new NewsSitePro.Models.HeaderViewModel();
        public bool IsOwnProfile { get; set; } = false;
        public bool IsFollowing { get; set; } = false;

        public UserProfileModel(DBservices dbService)
        {
            _dbService = dbService;
        }

        public async Task<IActionResult> OnGet(int? userId = null)
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
                HeaderData = new NewsSitePro.Models.HeaderViewModel
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
                    ProfilePicture = user.ProfilePicture,
                    JoinDate = user.JoinDate,
                    IsAdmin = user.IsAdmin,
                    Activity = userStats,
                    RecentPosts = recentPosts
                };

                // Check if current user is following this user
                if (currentUserId.HasValue && !IsOwnProfile)
                {
                    try
                    {
                        IsFollowing = await _dbService.IsUserFollowing(currentUserId.Value, targetUserId);
                    }
                    catch
                    {
                        IsFollowing = false;
                    }
                }
                else
                {
                    IsFollowing = false;
                }

                return Page();
            }
            catch (Exception ex)
            {
                // Log the exception (in production, you'd want proper logging)
                ViewData["ErrorMessage"] = "An error occurred while loading the profile. Please try again.";
                ViewData["ExceptionMessage"] = ex.Message; // For debugging, remove in production
                
                // Set up minimal header data for error display
                HeaderData = new NewsSitePro.Models.HeaderViewModel
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

        // API endpoint for getting user activity (recent liked and commented posts)
        public async Task<IActionResult> OnGetGetUserActivity(int? userId = null)
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

                // Get the target user ID - if not provided, use current user
                int targetUserId = userId ?? currentUser.Id;

                // Get user activity (liked and commented posts, most recent first)
                var userActivity = await _dbService.GetUserRecentActivityAsync(targetUserId, 1, 20);
                
                return new JsonResult(new { 
                    success = true, 
                    activities = userActivity.Select(a => new {
                        activityType = a.ActivityType,
                        articleID = a.ArticleID,
                        activityDate = a.ActivityDate,
                        title = a.Title,
                        category = a.Category,
                        imageURL = a.ImageURL,
                        sourceName = a.SourceName,
                        username = a.Username
                    })
                });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = "Error loading user activity: " + ex.Message });
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

        // API endpoint for getting followers
        public async Task<IActionResult> OnGetGetFollowers(int userId)
        {
            try
            {
                // Get followers list for the specified user
                var followers = await _dbService.GetUserFollowersAsync(userId);
                
                return new JsonResult(new { 
                    success = true, 
                    followers = followers.Select(user => new {
                        userId = user.Id,
                        username = user.Name,
                        bio = user.Bio,
                        profilePicture = user.ProfilePicture,
                        joinDate = user.JoinDate,
                        isAdmin = user.IsAdmin
                    })
                });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = "Error loading followers: " + ex.Message });
            }
        }

        // API endpoint for getting following
        public async Task<IActionResult> OnGetGetFollowing(int userId)
        {
            try
            {
                // Get following list for the specified user
                var following = await _dbService.GetUserFollowingAsync(userId);
                
                return new JsonResult(new { 
                    success = true, 
                    following = following.Select(user => new {
                        userId = user.Id,
                        username = user.Name,
                        bio = user.Bio,
                        profilePicture = user.ProfilePicture,
                        joinDate = user.JoinDate,
                        isAdmin = user.IsAdmin
                    })
                });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = "Error loading following: " + ex.Message });
            }
        }
    }
}
