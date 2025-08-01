using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NewsSite.BL;
using NewsSite.Models;
using NewsSitePro.Models;
using System.IdentityModel.Tokens.Jwt;

namespace NewsSite.Pages
{
    public class SavedArticlesModel : PageModel
    {
        private readonly DBservices _dbService;

        public SavedArticlesModel(DBservices dbService)
        {
            _dbService = dbService;
        }

        public HeaderViewModel HeaderData { get; set; } = new HeaderViewModel();
        public List<NewsArticle> SavedArticles { get; set; } = new List<NewsArticle>();
        public string SearchQuery { get; set; } = "";
        public string Category { get; set; } = "";
        public string SortBy { get; set; } = "recent"; // recent, category, title
        public string DisplayType { get; set; } = "grid"; // grid, list, compact
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
        public int TotalResults { get; set; } = 0;
        public const int PageSize = 12;

        public async Task<IActionResult> OnGetAsync(string q = "", string category = "", string sort = "recent", string display = "grid", int page = 1)
        {
            var isAuthenticated = User?.Identity?.IsAuthenticated ?? false;
            if (!isAuthenticated)
            {
                return RedirectToPage("/Login");
            }

            // Get current user
            User? currentUser = null;
            try
            {
                var jwtToken = Request.Cookies["jwtToken"];
                if (!string.IsNullOrEmpty(jwtToken))
                {
                    currentUser = new User().ExtractUserFromJWT(jwtToken);
                }
            }
            catch
            {
                return RedirectToPage("/Login");
            }

            if (currentUser == null)
            {
                return RedirectToPage("/Login");
            }

            // Set up page data
            SearchQuery = q ?? "";
            Category = category ?? "";
            SortBy = sort;
            DisplayType = display;
            CurrentPage = page;

            // Set up header data
            HeaderData = new HeaderViewModel
            {
                UserName = currentUser.Name ?? "Guest",
                NotificationCount = 3,
                CurrentPage = "SavedArticles",
                user = currentUser
            };
            ViewData["HeaderData"] = HeaderData;

            try
            {
                // Use the new search method with stored procedures for better performance
                if (!string.IsNullOrEmpty(SearchQuery) || !string.IsNullOrEmpty(Category))
                {
                    SavedArticles = await _dbService.SearchSavedArticlesAsync(currentUser.Id, SearchQuery, Category == "all" ? null : Category, CurrentPage, PageSize);
                    TotalResults = await _dbService.GetSavedArticlesCountAsync(currentUser.Id, SearchQuery, Category == "all" ? null : Category);
                }
                else
                {
                    // Map display type and sort options to stored procedure parameters
                    string spSortBy = SortBy switch
                    {
                        "title" => "Title",
                        "category" => "Category", 
                        "oldest" => "PublishDate",
                        _ => "SavedAt" // recent (default)
                    };
                    
                    string spSortOrder = SortBy == "oldest" ? "ASC" : "DESC";
                    
                    SavedArticles = await _dbService.GetSavedArticlesWithOptionsAsync(currentUser.Id, DisplayType, spSortBy, spSortOrder, CurrentPage, PageSize);
                    TotalResults = await _dbService.GetSavedArticlesCountAsync(currentUser.Id);
                }
                
                TotalPages = (int)Math.Ceiling((double)TotalResults / PageSize);
            }
            catch (Exception ex)
            {
                ViewData["ErrorMessage"] = "Error loading saved articles: " + ex.Message;
                SavedArticles = new List<NewsArticle>();
            }

            return Page();
        }

        // API endpoint for getting filtered saved articles (AJAX)
        public async Task<IActionResult> OnGetFilteredArticlesAsync(string q = "", string category = "", string sort = "recent", int page = 1)
        {
            var isAuthenticated = User?.Identity?.IsAuthenticated ?? false;
            if (!isAuthenticated)
            {
                return new JsonResult(new { success = false, message = "Not authenticated" });
            }

            try
            {
                var jwtToken = Request.Cookies["jwtToken"];
                if (string.IsNullOrEmpty(jwtToken))
                {
                    return new JsonResult(new { success = false, message = "Not authenticated" });
                }

                var currentUser = new User().ExtractUserFromJWT(jwtToken);
                if (currentUser == null)
                {
                    return new JsonResult(new { success = false, message = "User not found" });
                }

                // Use the new search method with stored procedures for better performance
                List<NewsArticle> filteredArticles;
                int totalResults;
                
                if (!string.IsNullOrEmpty(q) || (!string.IsNullOrEmpty(category) && category != "all"))
                {
                    filteredArticles = await _dbService.SearchSavedArticlesAsync(currentUser.Id, q, category == "all" ? null : category, page, PageSize);
                    totalResults = await _dbService.GetSavedArticlesCountAsync(currentUser.Id, q, category == "all" ? null : category);
                }
                else
                {
                    // Map sort options to stored procedure parameters
                    string spSortBy = sort switch
                    {
                        "title" => "Title",
                        "category" => "Category",
                        "oldest" => "PublishDate",
                        _ => "SavedAt" // recent (default)
                    };
                    
                    string spSortOrder = sort == "oldest" ? "ASC" : "DESC";
                    
                    filteredArticles = await _dbService.GetSavedArticlesWithOptionsAsync(currentUser.Id, "grid", spSortBy, spSortOrder, page, PageSize);
                    totalResults = await _dbService.GetSavedArticlesCountAsync(currentUser.Id);
                }

                var totalPages = (int)Math.Ceiling((double)totalResults / PageSize);
                
                var articles = filteredArticles.Select(a => new {
                        articleID = a.ArticleID,
                        title = a.Title,
                        content = a.Content,
                        imageURL = a.ImageURL,
                        sourceURL = a.SourceURL,
                        sourceName = a.SourceName,
                        category = a.Category,
                        publishDate = a.PublishDate,
                        username = a.Username,
                        likesCount = a.LikesCount,
                        viewsCount = a.ViewsCount,
                        isLiked = a.IsLiked,
                        isSaved = a.IsSaved
                    })
                    .ToList();

                return new JsonResult(new { 
                    success = true, 
                    articles = articles,
                    totalResults = totalResults,
                    totalPages = totalPages,
                    currentPage = page
                });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = "Error: " + ex.Message });
            }
        }
    }
}
