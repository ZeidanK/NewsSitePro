using Microsoft.AspNetCore.Mvc.RazorPages;
using NewsSitePro.Models;
using NewsSite.BL;

namespace NewsSite.Pages;
public class ProfileModel : PageModel
{
    public HeaderViewModel HeaderData { get; set; } = new HeaderViewModel();

    public void OnGet()
    {
        var isAuthenticated = User?.Identity?.IsAuthenticated ?? false;
        var dbService = new DBservices();
        User? currentUser = null;
        
        if (isAuthenticated)
        {
            // Try to get user ID from JWT token or claims
            var userIdClaim = User?.Claims?.FirstOrDefault(c => c.Type == "id" || c.Type == "userId" || c.Type == System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                currentUser = dbService.GetUserById(userId);
            }
        }
        
        HeaderData = new HeaderViewModel
        {
            UserName = isAuthenticated ? User?.Identity?.Name ?? "Guest" : "Guest",
            NotificationCount = isAuthenticated ? 3 : 0,
            CurrentPage = "Profile",
            user = currentUser
        };
        
        ViewData["HeaderData"] = HeaderData;
    }
}