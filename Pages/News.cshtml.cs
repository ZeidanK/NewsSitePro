/*
    News.cshtml.cs - Page model for public news browsing functionality
    Handles news display and user interaction for non-admin users
*/
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NewsSite.BL;

namespace NewsSite.Pages
{
    /// <summary>
    /// Page model for public news browsing system
    /// Provides news access and statistics for regular users
    /// </summary>
    public class NewsModel : PageModel
    {
        private readonly DBservices _dbService;

        // Properties for news statistics - displayed in page header
        public int TotalNewsCount { get; set; } = 0;
        public string? ErrorMessage { get; set; }

        // User context properties
        public User? CurrentUser { get; set; }

        public NewsModel()
        {
            _dbService = new DBservices();
        }

        /// <summary>
        /// Handle GET requests - load news statistics and user context
        /// Accessible to all users (authenticated and anonymous)
        /// </summary>
        public IActionResult OnGetAsync()
        {
            try
            {
                // Load current user if authenticated (optional for news page)
                var userId = NewsSite.BL.User.GetCurrentUserId(Request, User);
                if (userId.HasValue)
                {
                    CurrentUser = _dbService.GetUserById(userId.Value);
                }

                // Load news statistics for display
                LoadNewsStatistics();

                return Page();
            }
            catch (Exception ex)
            {
                // Log error but don't block access to news page
                ErrorMessage = "Unable to load some page data";
                Console.WriteLine($"Error in NewsModel.OnGetAsync: {ex.Message}");
                return Page();
            }
        }

        /// <summary>
        /// Load statistics for the news page header
        /// </summary>
        private void LoadNewsStatistics()
        {
            try
            {
                // Get total count of published news articles
                TotalNewsCount = _dbService.GetTotalNewsArticlesCount();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading news statistics: {ex.Message}");
                TotalNewsCount = 0;
            }
        }
    }
}
