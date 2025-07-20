using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NewsSite.BL;

namespace NewsSite.Pages
{
    public class UserProfileModel : PageModel
    {
        private readonly DBservices _dbService;

        public UserProfile? UserProfile { get; set; }

        public UserProfileModel()
        {
            _dbService = new DBservices();
        }

        public IActionResult OnGet(int userId)
        {
            try
            {
                // Get user basic info
                var user = _dbService.GetUser(null, userId, null);
                if (user == null)
                {
                    return NotFound("User not found");
                }

                // Get user statistics
                var userStats = _dbService.GetUserStats(userId);

                // Get user's recent posts
                var recentPosts = _dbService.GetArticlesByUser(userId, 1, 10);

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

                return Page();
            }
            catch (Exception)
            {
                return RedirectToPage("/Error");
            }
        }
    }
}
