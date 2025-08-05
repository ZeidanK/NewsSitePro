using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NewsSitePro.Models;
using NewsSite.BL;
using NewsSite.Models;

namespace NewsSite.Pages
{
    public class TestPostModel : PageModel
    {
        private readonly DBservices _dbService;

        public HeaderViewModel HeaderData { get; set; } = new HeaderViewModel();
        public NewsArticle? PostData { get; set; }
        public List<Comment> Comments { get; set; } = new List<Comment>();
        public string? ErrorMessage { get; set; }

        public TestPostModel()
        {
            _dbService = new DBservices();
        }

        public async Task<IActionResult> OnGet()
        {
            try
            {
                // Fixed post ID for testing
                int testPostId = 50;
                
                // Simplified user authentication detection
                bool isAuthenticated = false;
                int? currentUserId = null;
                User? currentUser = null;
                
                // Method 1: Check JWT token from cookies
                var jwtToken = Request.Cookies["jwtToken"];
                if (!string.IsNullOrEmpty(jwtToken))
                {
                    try
                    {
                        var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
                        var jsonToken = handler.ReadJwtToken(jwtToken);
                        var userIdClaim = jsonToken.Claims.FirstOrDefault(c => c.Type == "id" || c.Type == "userId" || c.Type == "nameid" || c.Type == System.Security.Claims.ClaimTypes.NameIdentifier);
                        if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
                        {
                            currentUserId = userId;
                            isAuthenticated = true;
                            
                            // Get user details
                            try
                            {
                                currentUser = _dbService.GetUserById(userId);
                                if (currentUser != null)
                                {
                                    HeaderData.user = currentUser;
                                    Console.WriteLine($"User authenticated: {currentUser.Name} (ID: {userId})");
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error loading user data: {ex.Message}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"JWT token parsing error: {ex.Message}");
                    }
                }
                
                // Method 2: Check User.Identity as fallback (skip session completely)
                if (!isAuthenticated && User?.Identity?.IsAuthenticated == true)
                {
                    var userIdClaim = User.FindFirst("id")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                    if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out int identityUserId))
                    {
                        currentUserId = identityUserId;
                        isAuthenticated = true;
                        
                        try
                        {
                            currentUser = _dbService.GetUserById(identityUserId);
                            if (currentUser != null)
                            {
                                HeaderData.user = currentUser;
                                Console.WriteLine($"User authenticated via Identity: {currentUser.Name} (ID: {identityUserId})");
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error loading user data from Identity: {ex.Message}");
                        }
                    }
                }
                
                // Debug logging
                Console.WriteLine($"Authentication status: {isAuthenticated}, UserId: {currentUserId}, User: {currentUser?.Name ?? "null"}");
                
                // Set authentication status in ViewData for debugging
                ViewData["IsAuthenticated"] = isAuthenticated;
                ViewData["CurrentUserId"] = currentUserId;
                ViewData["CurrentUserName"] = currentUser?.Name;

                // Load the specific post
                try
                {
                    PostData = await _dbService.GetNewsArticleById(testPostId);
                    if (PostData == null)
                    {
                        ErrorMessage = $"Post with ID {testPostId} not found in database.";
                        return Page();
                    }
                }
                catch (Exception ex)
                {
                    ErrorMessage = $"Error loading post: {ex.Message}";
                    return Page();
                }

                // Load comments for the post
                try
                {
                    Comments = await _dbService.GetCommentsByPostId(testPostId);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error loading comments: {ex.Message}");
                    Comments = new List<Comment>(); // Continue without comments
                }

                return Page();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"General error: {ex.Message}";
                return Page();
            }
        }
    }
}
