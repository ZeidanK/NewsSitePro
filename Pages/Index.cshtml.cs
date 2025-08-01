using Microsoft.AspNetCore.Mvc.RazorPages;
using NewsSite.BL;
using NewsSitePro.Models;

public class IndexModel : PageModel
{
    public HeaderViewModel HeaderData { get; set; } = new HeaderViewModel();
    public List<NewsArticle> Posts { get; set; } = new List<NewsArticle>();
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
        }
        catch (Exception ex)
        {
            // Log the error and continue with empty posts
            Console.WriteLine($"Error loading posts: {ex.Message}");
            Posts = new List<NewsArticle>();
        }
    }
}