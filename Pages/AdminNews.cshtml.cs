/*
    AdminNews.cshtml.cs - Page model for admin news management functionality
    Handles authentication, news statistics, and admin-specific data preparation
*/
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NewsSite.BL;
using System.IdentityModel.Tokens.Jwt;

namespace NewsSite.Pages
{
    /// <summary>
    /// Page model for admin news management system
    /// Provides authentication, statistics, and data preparation for news administration
    /// </summary>
    public class AdminNewsModel : PageModel
    {
        private readonly DBservices _dbService;

        // Properties for dashboard statistics - displayed in page header
        public int PendingNewsCount { get; set; } = 0;
        public int PublishedTodayCount { get; set; } = 0;
        public int TotalArticlesCount { get; set; } = 0;
        public string? ErrorMessage { get; set; }

        // User context properties for admin verification
        public User? CurrentUser { get; set; }
        public bool IsAdminUser { get; set; } = false;

        public AdminNewsModel()
        {
            _dbService = new DBservices();
        }

        /// <summary>
        /// Handle GET requests - verify admin access and load statistics
        /// Redirects non-admin users to appropriate error page
        /// </summary>
        public IActionResult OnGetAsync()
        {
            try
            {
                // Check if user is admin using same logic as Admin page
                var jwt = Request.Cookies["jwtToken"];
                
                if (string.IsNullOrEmpty(jwt))
                {
                    return RedirectToPage("/Login");
                }

                var user = new User().ExtractUserFromJWT(jwt);
                
                if (!user.IsAdmin)
                {
                    return RedirectToPage("/Index"); // Redirect non-admin users to home
                }

                // Set user context
                CurrentUser = user;
                IsAdminUser = true;

                // Load news management statistics
                LoadNewsStatistics();

                return Page();
            }
            catch (Exception ex)
            {
                // Log error and display user-friendly message
                ErrorMessage = "Failed to load admin news panel. Please try again.";
                Console.WriteLine($"AdminNews Error: {ex.Message}");
                return Page();
            }
        }

        /// <summary>
        /// Load statistics for news management dashboard
        /// Calculates pending, published, and total article counts
        /// </summary>
        private void LoadNewsStatistics()
        {
            try
            {
                // Get total articles count
                var allArticles = _dbService.GetAllNewsArticles(1, 1000); // Get large batch for counting
                TotalArticlesCount = allArticles?.Count ?? 0;

                // Calculate published today count
                var today = DateTime.Today;
                var todayArticles = allArticles?.Where(a => a.PublishDate.Date == today).ToList();
                PublishedTodayCount = todayArticles?.Count ?? 0;

                // For now, set pending count to 0 - can be enhanced with actual pending system
                PendingNewsCount = 0;

                // TODO: Implement actual pending/approval system
                // This would involve adding status columns to articles table
                // and tracking article workflow states (pending, approved, published, rejected)
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading news statistics: {ex.Message}");
                // Set default values on error
                PendingNewsCount = 0;
                PublishedTodayCount = 0;
                TotalArticlesCount = 0;
            }
        }

        /// <summary>
        /// Check if current user has admin privileges
        /// Uses same logic as existing admin pages for consistency
        /// </summary>
        private bool IsCurrentUserAdmin()
        {
            try
            {
                // Check JWT claims first
                var adminClaim = User.FindFirst("isAdmin");
                if (adminClaim != null && bool.TryParse(adminClaim.Value, out bool isAdmin))
                {
                    return isAdmin;
                }

                // Check JWT token in cookies as fallback
                var jwtCookie = Request.Cookies["jwtToken"];
                if (!string.IsNullOrEmpty(jwtCookie))
                {
                    var handler = new JwtSecurityTokenHandler();
                    if (handler.CanReadToken(jwtCookie))
                    {
                        var jsonToken = handler.ReadJwtToken(jwtCookie);
                        var adminTokenClaim = jsonToken.Claims.FirstOrDefault(x => x.Type == "isAdmin");
                        
                        if (adminTokenClaim != null && bool.TryParse(adminTokenClaim.Value, out bool tokenIsAdmin))
                        {
                            return tokenIsAdmin;
                        }
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking admin status: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Get current user ID from JWT token
        /// Returns null if user is not authenticated or token is invalid
        /// </summary>
        private int? GetCurrentUserId()
        {
            try
            {
                // Try to get from JWT claims first
                var userIdClaim = User.FindFirst("id");
                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
                {
                    return userId;
                }

                // Check cookies for JWT token
                var jwtCookie = Request.Cookies["jwtToken"];
                if (!string.IsNullOrEmpty(jwtCookie))
                {
                    var handler = new JwtSecurityTokenHandler();
                    if (handler.CanReadToken(jwtCookie))
                    {
                        var jsonToken = handler.ReadJwtToken(jwtCookie);
                        var idClaim = jsonToken.Claims.FirstOrDefault(x => x.Type == "id");
                        
                        if (idClaim != null && int.TryParse(idClaim.Value, out int cookieUserId))
                        {
                            return cookieUserId;
                        }
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting user ID: {ex.Message}");
                return null;
            }
        }

        // TODO: Future enhancement methods for expanded functionality
        // These can be implemented as the system grows

        /// <summary>
        /// AJAX endpoint for fetching news articles with filtering
        /// Future implementation for dynamic news loading
        /// </summary>
        public async Task<IActionResult> OnGetNewsArticlesAsync(string filter = "all", int page = 1, int pageSize = 20)
        {
            if (!IsCurrentUserAdmin())
            {
                return Forbid();
            }

            // TODO: Implement filtered news retrieval
            // This would support different filters like pending, approved, published, rejected
            
            return new JsonResult(new { success = true, articles = new List<object>(), totalCount = 0 });
        }

        /// <summary>
        /// AJAX endpoint for updating article status
        /// Future implementation for news approval workflow
        /// </summary>
        public async Task<IActionResult> OnPostUpdateArticleStatusAsync([FromBody] UpdateArticleStatusRequest request)
        {
            if (!IsCurrentUserAdmin())
            {
                return Forbid();
            }

            // TODO: Implement article status updates
            // This would handle approve, reject, publish, edit operations
            
            return new JsonResult(new { success = true, message = "Article status updated successfully" });
        }
    }

    /// <summary>
    /// Request model for article status updates
    /// Used by AJAX endpoints for news workflow management
    /// </summary>
    public class UpdateArticleStatusRequest
    {
        public int ArticleId { get; set; }
        public string Status { get; set; } = string.Empty; // pending, approved, published, rejected
        public string? Reason { get; set; } // Optional reason for rejection
        public string? EditedContent { get; set; } // Optional edited content
    }
}
