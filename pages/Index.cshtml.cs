using Microsoft.AspNetCore.Mvc.RazorPages;
using NewsSitePro.Models;

public class IndexModel : PageModel
{
    public HeaderViewModel HeaderData { get; set; }

    public void OnGet()
    {
        // Replace with your actual user/notification logic
        HeaderData = new HeaderViewModel
        {
            UserName = User.Identity.IsAuthenticated ? User.Identity.Name : "Guest",
            NotificationCount = User.Identity.IsAuthenticated ? 3 : 0 // Example
        };
    }
}