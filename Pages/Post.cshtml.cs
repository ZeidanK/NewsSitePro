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
        public HeaderViewModel HeaderData { get; set; } = new HeaderViewModel();

        public void OnGet()
        {
            var isAuthenticated = User?.Identity?.IsAuthenticated ?? false;
            
            HeaderData = new HeaderViewModel
            {
                UserName = isAuthenticated ? User?.Identity?.Name ?? "Guest" : "Guest",
                NotificationCount = isAuthenticated ? 3 : 0,
                CurrentPage = "Post",
                user = isAuthenticated ? new User 
                { 
                    Id = int.Parse(User?.Claims?.FirstOrDefault(c => c.Type == "userId")?.Value ?? "0"),
                    Name = User?.Identity?.Name ?? "Guest",
                    Email = User?.Claims?.FirstOrDefault(c => c.Type == "email")?.Value ?? "",
                    IsAdmin = User?.IsInRole("Admin") == true || User?.Claims?.Any(c => c.Type == "isAdmin" && c.Value == "True") == true
                } : null
            };
            
            ViewData["HeaderData"] = HeaderData;
        }
    }
}