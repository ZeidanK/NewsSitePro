using Microsoft.AspNetCore.Mvc.RazorPages;

namespace NewsSite.Pages
{
    public class GoogleOAuthCallbackModel : PageModel
    {
        public void OnGet()
        {
            // This page handles the OAuth callback via JavaScript
            // No server-side processing needed in the page model
        }
    }
}
