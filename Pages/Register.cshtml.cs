// using Microsoft.AspNetCore.Mvc.RazorPages;
// using NewsSitePro.Models;
// namespace NewsSite.Pages;

using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using NewsSitePro.Models;
using NewsSite.BL;
using NewsSite.Models;
using NewsSite.Services;
//using NewsSite.Pages
namespace NewsSite.Pages;

public class RegisterModel : PageModel
{
    private readonly IApiConfigurationService _apiConfigurationService;

    public RegisterModel(IApiConfigurationService apiConfigurationService)
    {
        _apiConfigurationService = apiConfigurationService;
    }

    public HeaderViewModel HeaderData { get; set; } = new HeaderViewModel();

    [BindProperty]
    public string Email { get; set; } = string.Empty;
    [BindProperty]
    public string Password { get; set; } = string.Empty;
    [BindProperty]
    public string Name { get; set; } = string.Empty;
    public void OnGet()
    {
        HeaderData = new HeaderViewModel
        {
            UserName = User.Identity?.IsAuthenticated == true ? User.Identity.Name ?? "Guest" : "Guest",
            NotificationCount = User.Identity?.IsAuthenticated == true ? 3 : 0 ,// Example,
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

        var apiUrl = _apiConfigurationService.GetApiUrl("api/Auth/register");
        var response = await client.PostAsJsonAsync(apiUrl, registerRequest);
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