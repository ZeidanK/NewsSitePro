using Microsoft.AspNetCore.Mvc.RazorPages;
using NewsSitePro.Models;

public class PostModel : PageModel
{
    public void OnGet()
    {
        // Replace with actual user logic
        ViewData["HeaderData"] = new HeaderViewModel
        {
            UserName = User.Identity.IsAuthenticated ? User.Identity.Name : "Guest",
            NotificationCount = 1,
            CurrentPage = "Post"
        };
    }
}