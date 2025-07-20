using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NewsSite.BL;
using NewsSite.Models;
using NewsSitePro.Models;
using System.IdentityModel.Tokens.Jwt;

namespace NewsSite.Pages
{
    public class SearchModel : PageModel
    {
        private readonly DBservices _dbService;

        public SearchModel(DBservices dbService)
        {
            _dbService = dbService;
        }

        public HeaderViewModel HeaderData { get; set; } = new HeaderViewModel();
        public List<SearchArticleResult> SearchResults { get; set; } = new List<SearchArticleResult>();
        public List<SearchUserResult> UserResults { get; set; } = new List<SearchUserResult>();
        public string SearchQuery { get; set; } = "";
        public string SearchType { get; set; } = "posts"; // posts, users, all
        public string Category { get; set; } = "";
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
        public int TotalResults { get; set; } = 0;
        public const int PageSize = 10;

        public async Task<IActionResult> OnGetAsync(string q = "", string type = "posts", string category = "", int page = 1)
        {
            SearchQuery = q ?? "";
            SearchType = type;
            Category = category;
            CurrentPage = page;

            // Set up header data
            var isAuthenticated = User?.Identity?.IsAuthenticated ?? false;
            HeaderData = new HeaderViewModel
            {
                UserName = isAuthenticated ? User?.Identity?.Name ?? "Guest" : "Guest",
                NotificationCount = isAuthenticated ? 3 : 0,
                CurrentPage = "Search",
                user = isAuthenticated ? new User 
                { 
                    Name = User?.Identity?.Name ?? "Guest",
                    Email = User?.Claims?.FirstOrDefault(c => c.Type == "email")?.Value ?? "",
                    IsAdmin = User?.IsInRole("Admin") == true || User?.Claims?.Any(c => c.Type == "isAdmin" && c.Value == "True") == true
                } : null
            };
            ViewData["HeaderData"] = HeaderData;

            if (!string.IsNullOrEmpty(SearchQuery))
            {
                try
                {
                    int? currentUserId = null;
                    if (isAuthenticated)
                    {
                        // Get current user ID from JWT token
                        var jwtToken = Request.Cookies["jwtToken"];
                        if (!string.IsNullOrEmpty(jwtToken))
                        {
                            var handler = new JwtSecurityTokenHandler();
                            var token = handler.ReadJwtToken(jwtToken);
                            var userIdClaim = token.Claims.FirstOrDefault(c => c.Type == "nameid");
                            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
                            {
                                currentUserId = userId;
                            }
                        }
                    }

                    if (SearchType == "posts" || SearchType == "all")
                    {
                        // Search for posts/articles
                        var articles = await _dbService.SearchArticlesAsync(SearchQuery, Category, CurrentPage, PageSize, currentUserId);
                        SearchResults = articles.Select(a => new SearchArticleResult
                        {
                            ArticleID = a.ArticleID,
                            Title = a.Title,
                            Content = a.Content,
                            SourceURL = a.SourceURL,
                            SourceName = a.SourceName,
                            ImageURL = a.ImageURL,
                            PublishDate = a.PublishDate,
                            Category = a.Category,
                            UserID = a.UserID,
                            UserName = a.Username,
                            LikesCount = a.LikesCount,
                            ViewsCount = a.ViewsCount,
                            IsLiked = a.IsLiked,
                            IsSaved = a.IsSaved
                        }).ToList();
                        
                        TotalResults = SearchResults.Count;
                        TotalPages = (int)Math.Ceiling((double)TotalResults / PageSize);
                    }

                    if (SearchType == "users" || SearchType == "all")
                    {
                        // Search for users
                        var users = await _dbService.SearchUsersAsync(SearchQuery, CurrentPage, PageSize);
                        UserResults = users.Select(u => new SearchUserResult
                        {
                            Id = u.Id,
                            Name = u.Name,
                            Email = u.Email,
                            Bio = u.Bio,
                            JoinDate = u.JoinDate,
                            IsAdmin = u.IsAdmin,
                            IsLocked = u.IsLocked,
                            PostsCount = 0, // Will be populated by stored procedure
                            FollowersCount = 0,
                            FollowingCount = 0
                        }).ToList();
                        
                        if (SearchType == "users")
                        {
                            TotalResults = UserResults.Count;
                            TotalPages = (int)Math.Ceiling((double)TotalResults / PageSize);
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Log error and continue with empty results
                    SearchResults = new List<SearchArticleResult>();
                    UserResults = new List<SearchUserResult>();
                }
            }

            return Page();
        }

        // API endpoint for AJAX search suggestions
        public async Task<IActionResult> OnGetSuggestionsAsync(string q = "", string type = "posts")
        {
            if (string.IsNullOrEmpty(q) || q.Length < 2)
            {
                return new JsonResult(new { suggestions = new List<object>() });
            }

            try
            {
                var suggestions = new List<object>();

                if (type == "posts" || type == "all")
                {
                    var articles = await _dbService.SearchArticlesAsync(q, "", 1, 5, null);
                    suggestions.AddRange(articles.Take(3).Select(a => new {
                        type = "post",
                        id = a.ArticleID,
                        title = a.Title,
                        content = a.Content?.Length > 100 ? a.Content.Substring(0, 100) + "..." : a.Content,
                        author = a.Username,
                        category = a.Category
                    }));
                }

                if (type == "users" || type == "all")
                {
                    var users = await _dbService.SearchUsersAsync(q, 1, 3);
                    suggestions.AddRange(users.Select(u => new {
                        type = "user",
                        id = u.Id,
                        name = u.Name,
                        email = u.Email,
                        bio = u.Bio
                    }));
                }

                return new JsonResult(new { suggestions });
            }
            catch
            {
                return new JsonResult(new { suggestions = new List<object>() });
            }
        }
    }
}
