using Microsoft.AspNetCore.Mvc.RazorPages;
using NewsSite.BL;
using NewsSitePro.Models;

public class IndexModel : PageModel
{
    public HeaderViewModel HeaderData { get; set; } = new HeaderViewModel();
    public List<NewsArticle> Posts { get; set; } = new List<NewsArticle>();
    public Dictionary<int, bool> FollowStatusMap { get; set; } = new Dictionary<int, bool>();
    private readonly DBservices _dbService;

    public IndexModel()
    { 
        _dbService = new DBservices();
    }

    public void OnGet() 
    {
        var jwt = Request.Cookies["jwtToken"];
        User? currentUser = null;
        int? currentUserId = null;

        if (!string.IsNullOrEmpty(jwt))
        {
            try
            {
                var user = new User().ExtractUserFromJWT(jwt);
                currentUser = _dbService.GetUserById(user.Id);
                currentUserId = user.Id;
            }
            catch
            {
                // Invalid JWT, continue as guest
            }
        }

        HeaderData = new HeaderViewModel
        {
            UserName = currentUser?.Name ?? (User?.Identity?.IsAuthenticated == true ? User?.Identity?.Name ?? "Guest" : "Guest"),
            NotificationCount = currentUser != null ? 3 : 0, // Example
            CurrentPage = "Home",
            user = currentUser
        };

        // Load initial posts (first page)
        try
        {
            Posts = _dbService.GetAllNewsArticles(1, 10, null, currentUserId);
            
            // Load follow status for all post authors if user is logged in
            if (currentUserId.HasValue && Posts.Any())
            {
                var uniqueUserIds = Posts.Select(p => p.UserID).Distinct().Where(uid => uid != currentUserId.Value);
                foreach (var userId in uniqueUserIds)
                {
                    try
                    {
                        var isFollowing = _dbService.IsUserFollowing(currentUserId.Value, userId).Result;
                        ViewData["IsFollowing_" + userId] = isFollowing;
                        FollowStatusMap[userId] = isFollowing;
                    }
                    catch
                    {
                        // If follow status check fails, default to false
                        ViewData["IsFollowing_" + userId] = false;
                        FollowStatusMap[userId] = false;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            // Log the error and continue with empty posts
            Console.WriteLine($"Error loading posts: {ex.Message}");
            Posts = new List<NewsArticle>();
        }
    }
}