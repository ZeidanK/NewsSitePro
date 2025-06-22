using Microsoft.AspNetCore.Mvc.RazorPages;
using NewsSite.BL;
using NewsSitePro.Models;

public class IndexModel : PageModel
{
    public HeaderViewModel HeaderData { get; set; }

    public void OnGet()
    {
        var jwt = Request.Cookies["jwtToken"];
        if (!string.IsNullOrEmpty(jwt))
        {
            var user = new User().ExtractUserFromJWT(jwt);
            HeaderData = new HeaderViewModel
            {
                UserName = user.Name,
                NotificationCount = 3, // Example
                CurrentPage = "Home",
                user = user
            };
        }
        else
        {
            // Replace with your actual user/notification logic
            HeaderData = new HeaderViewModel
            {
                UserName = User.Identity.IsAuthenticated ? User.Identity.Name : "Guest",
                NotificationCount = User.Identity.IsAuthenticated ? 3 : 0, // Example
                CurrentPage = "Home"
            };
        }
    }
}