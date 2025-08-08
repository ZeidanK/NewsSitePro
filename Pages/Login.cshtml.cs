using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using NewsSitePro.Models;
using NewsSite.BL;
using NewsSite.Models;
using NewsSite.Services;
//using NewsSite.Pages
namespace NewsSite.Pages;

public class LoginModel : PageModel
{
    private readonly IApiConfigurationService _apiConfigurationService;

    public LoginModel(IApiConfigurationService apiConfigurationService)
    {
        _apiConfigurationService = apiConfigurationService;
    }

    [BindProperty]
    public string Email { get; set; } = string.Empty;
    [BindProperty]
    public string Password { get; set; } = string.Empty;
    public HeaderViewModel HeaderData { get; set; } = new HeaderViewModel();
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
            UserName = User.Identity?.IsAuthenticated == true ? User.Identity.Name ?? "Guest" : "Guest",
            NotificationCount = User.Identity?.IsAuthenticated == true ? 3 : 0, // Example
            CurrentPage = "Login"
        };
    }
        public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        using var httpClient = new HttpClient();
        var loginRequest = new { Email = Email, Password = Password };
        var apiUrl = _apiConfigurationService.GetApiUrl("api/Auth/login");
        var response = await httpClient.PostAsJsonAsync(apiUrl, loginRequest);

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
        public string token { get; set; } = string.Empty;
    }

}
