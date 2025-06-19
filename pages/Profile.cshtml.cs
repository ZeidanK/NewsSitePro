using Microsoft.AspNetCore.Mvc.RazorPages;
using NewsSitePro.Models;

public class ProfileModel : PageModel
{
    public void OnGet()
    {
        // Replace with actual user logic
        ViewData["HeaderData"] = new HeaderViewModel
        {
            UserName = User.Identity.IsAuthenticated ? User.Identity.Name : "Guest",
            NotificationCount = 2,
            CurrentPage = "Profile"
        };
    }
}