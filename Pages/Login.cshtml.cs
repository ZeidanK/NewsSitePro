using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using NewsSitePro.Models;
using NewsSite.BL;
using NewsSite.Models;
//using NewsSite.Pages
namespace NewsSite.Pages;

public class LoginModel : PageModel
{
    [BindProperty]
    public string Email { get; set; }
    [BindProperty]
    public string Password { get; set; }
    public HeaderViewModel HeaderData { get; set; }
    public void OnGet()
    {
        //ViewData["HeaderData"] = new HeaderViewModel
        //{
        //    UserName = null,
        //    NotificationCount = 0,
        //    CurrentPage = "Login"
        //};
        HeaderData = new HeaderViewModel
        {
            UserName = User.Identity.IsAuthenticated ? User.Identity.Name : "Guest",
            NotificationCount = User.Identity.IsAuthenticated ? 3 : 0, // Example
            CurrentPage = "Login"
        };
    }
        public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        using var httpClient = new HttpClient();
        var loginRequest = new { Email = Email, Password = Password };
        var response = await httpClient.PostAsJsonAsync("https://localhost:5001/api/Auth/login", loginRequest);

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
            // You can set a cookie or session here with the token if needed
            // For example: HttpContext.Session.SetString("AuthToken", result.token);
            return RedirectToPage("/Index");
        }
        else
        {
            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return Page();
        }
    }

    public class LoginResponse
    {
        public string token { get; set; }
    }

}
