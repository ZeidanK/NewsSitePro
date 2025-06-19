using Microsoft.AspNetCore.Mvc.RazorPages;
using NewsSitePro.Models;

public class RegisterModel : PageModel
{
    public void OnGet()
    {
        ViewData["HeaderData"] = new HeaderViewModel
        {
            UserName = null,
            NotificationCount = 0,
            CurrentPage = "Register"
        };
    }
}