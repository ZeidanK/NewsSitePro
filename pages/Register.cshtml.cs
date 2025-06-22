// using Microsoft.AspNetCore.Mvc.RazorPages;
// using NewsSitePro.Models;
// namespace NewsSite.Pages;

using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using NewsSitePro.Models;
using NewsSite.BL;
using NewsSite.Models;
//using NewsSite.Pages
namespace NewsSite.Pages;

public class RegisterModel : PageModel
{
    public HeaderViewModel HeaderData { get; set; }

    [BindProperty]
    public string Email { get; set; }
    [BindProperty]
    public string Password { get; set; }
    [BindProperty]
    public string Name { get; set; }
    public void OnGet()
    {
        HeaderData = new HeaderViewModel
        {
            UserName = User.Identity.IsAuthenticated ? User.Identity.Name : "Guest",
            NotificationCount = User.Identity.IsAuthenticated ? 3 : 0 ,// Example,
            CurrentPage = "Register"
        };
    }

    public async Task<IActionResult> OnPostAsync([FromServices] IHttpClientFactory httpClientFactory)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var client = httpClientFactory.CreateClient();
        var registerRequest = new
        {
            Name = Name,
            Email = Email,
            Password = Password
        };

        var response = await client.PostAsJsonAsync("https://localhost:5001/api/Auth/register", registerRequest);
        var responseContent = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            TempData["AlertMessage"] = "Registration successful!";
            return RedirectToPage("/Login");
        }
        else
        {
            TempData["AlertMessage"] = "Registration failed: " + responseContent;
            return RedirectToPage("/Register");
        }
    }
}