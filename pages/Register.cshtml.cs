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
    public void OnGet()
    {
        HeaderData = new HeaderViewModel
        {
            UserName = User.Identity.IsAuthenticated ? User.Identity.Name : "Guest",
            NotificationCount = User.Identity.IsAuthenticated ? 3 : 0 // Example
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
            Name = Email, // Or add a Name property to your form/model
            Email = Email,
            Password = Password
        };

        var response = await client.PostAsJsonAsync("https://localhost:5001/api/Auth/register", registerRequest);

        if (response.IsSuccessStatusCode)
        {
            // Registration successful, redirect or show success
            return RedirectToPage("/Login");
        }
        else
        {
            // Handle error (e.g., user already exists)
            ModelState.AddModelError(string.Empty, "Registration failed: " + await response.Content.ReadAsStringAsync());
            return Page();
        }
    }
}