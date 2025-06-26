using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NewsSitePro.Models;
using Microsoft.AspNetCore.Identity;
using NewsSite.BL;
using NewsSite.Models;
//using NewsSite.Pages

namespace NewsSite.Pages
{
    [Authorize] // Ensure only logged-in users can access this page
    public class PostModel : PageModel
    {
        public HeaderViewModel HeaderData { get; set; }

        public void OnGet()
        {
            // ViewData["HeaderData"] = new HeaderViewModel
            // {
            //     UserName = User.Identity.IsAuthenticated ? User.Identity.Name : "Guest",
            //     NotificationCount = 1,
            //     CurrentPage = "Post"
            // };
            HeaderData = new HeaderViewModel
            {
                UserName = User?.Identity?.IsAuthenticated == true ? User.Identity.Name ?? "Guest" : "Guest",
                NotificationCount = User.Identity.IsAuthenticated ? 3 : 0, // Example
                CurrentPage = "Login"
            };
        }
    }
}