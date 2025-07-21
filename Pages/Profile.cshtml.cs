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
        
        HeaderData = new HeaderViewModel
        {
            UserName = isAuthenticated ? User?.Identity?.Name ?? "Guest" : "Guest",
            NotificationCount = isAuthenticated ? 3 : 0,
            CurrentPage = "Profile",
            user = isAuthenticated ? new User 
            { 
                Name = User?.Identity?.Name ?? "Guest",
                Email = User?.Claims?.FirstOrDefault(c => c.Type == "email")?.Value ?? "",
                IsAdmin = User?.IsInRole("Admin") == true || User?.Claims?.Any(c => c.Type == "isAdmin" && c.Value == "True") == true
            } : null
        };
        
        ViewData["HeaderData"] = HeaderData;
    }
}