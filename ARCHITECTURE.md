# Project Architecture Documentation

## 3-Layer Architecture Overview

This project implements a clean 3-layer architecture pattern following enterprise-level best practices.

## Layer 1: Data Access Layer (DAL)

### Location: `/DAL/`

**Primary Components:**
- `DBservices.cs` - Central data access class
- Database connection management
- Stored procedure execution
- Data mapping and transformation

**Responsibilities:**
- Execute SQL commands and stored procedures
- Handle database connections and transactions
- Map database results to business objects
- Implement CRUD operations
- Handle database-specific error handling

**Key Methods:**
```csharp
// User Management
public User? GetUser(string? email = null, int? id = null, string? name = null)
public bool CreateUser(User user)
public bool UpdateUser(User user)

// News Articles
public List<NewsArticle> GetAllNewsArticles(int pageNumber, int pageSize, string? category, int? currentUserId)
public NewsArticle? GetNewsArticleById(int articleId, int? currentUserId = null)
public int CreateNewsArticle(NewsArticle article)

// Comments
public async Task<List<Comment>> GetCommentsByPostId(int postId)
public async Task<bool> CreateComment(Comment comment)

// Admin Operations
public async Task<AdminDashboardStats> GetAdminDashboardStats()
public async Task<List<AdminUserView>> GetAllUsersForAdmin(int page, int pageSize)
```

## Layer 2: Business Logic Layer (BL)

### Location: `/BL/`

**Structure:**
```
BL/
├── Interfaces/           # Service contracts
│   ├── IUserService.cs
│   ├── INewsService.cs
│   ├── ICommentService.cs
│   └── IAdminService.cs
├── Services/            # Service implementations
│   ├── UserService.cs
│   ├── NewsService.cs
│   ├── CommentService.cs
│   └── AdminService.cs
└── *.cs                # Models and DTOs
```

**Responsibilities:**
- Implement business rules and validation
- Coordinate between controllers and data access
- Handle complex business operations
- Manage transactions and data consistency
- Provide abstraction over data access

**Service Interfaces:**

### IUserService
```csharp
public interface IUserService
{
    Task<User?> GetUserByIdAsync(int id);
    Task<User?> GetUserByEmailAsync(string email);
    Task<bool> CreateUserAsync(User user);
    Task<bool> UpdateUserAsync(User user);
    Task<List<User>> SearchUsersAsync(string searchTerm, int pageNumber, int pageSize);
    Task<UserActivity> GetUserStatsAsync(int userId);
}
```

### INewsService
```csharp
public interface INewsService
{
    Task<List<NewsArticle>> GetAllNewsArticlesAsync(int pageNumber, int pageSize, string? category, int? currentUserId);
    Task<NewsArticle?> GetNewsArticleByIdAsync(int articleId, int? currentUserId);
    Task<int> CreateNewsArticleAsync(NewsArticle article);
    Task<List<NewsArticle>> SearchArticlesAsync(string searchTerm, string category, int pageNumber, int pageSize, int? currentUserId);
    Task<string> ToggleArticleLikeAsync(int articleId, int userId);
}
```

### ICommentService
```csharp
public interface ICommentService
{
    Task<List<Comment>> GetCommentsByPostIdAsync(int postId);
    Task<bool> CreateCommentAsync(Comment comment);
    Task<bool> UpdateCommentAsync(int commentId, int userId, string content);
    Task<bool> DeleteCommentAsync(int commentId, int userId);
    Task<int> GetCommentsCountAsync(int postId);
}
```

### IAdminService
```csharp
public interface IAdminService
{
    Task<AdminDashboardStats> GetAdminDashboardStatsAsync();
    Task<List<AdminUserView>> GetAllUsersForAdminAsync(int page, int pageSize);
    Task<List<UserReport>> GetAllReportsAsync();
    Task<List<ActivityLog>> GetActivityLogsAsync(int page, int pageSize);
}
```

**Business Logic Examples:**
```csharp
public async Task<bool> CreateUserAsync(User user)
{
    // Business validation
    if (string.IsNullOrWhiteSpace(user.Email) || string.IsNullOrWhiteSpace(user.Name))
    {
        throw new ArgumentException("Email and Name are required");
    }

    // Check if user already exists
    var existingUser = await GetUserByEmailAsync(user.Email);
    if (existingUser != null)
    {
        throw new InvalidOperationException("User with this email already exists");
    }

    // Delegate to DAL
    return await Task.FromResult(_dbService.CreateUser(user));
}
```

## Layer 3: Presentation Layer

### Location: `/Controllers/` and `/Pages/`

**Controllers (API Layer):**
- `AuthController.cs` - Authentication endpoints
- `PostsController.cs` - News article operations
- `UserController.cs` - User management
- `CommentsController.cs` - Comment operations
- `AdminController.cs` - Administrative functions

**Razor Pages (UI Layer):**
- `Index.cshtml` - Homepage with news feed
- `Post.cshtml` - Individual post view with comments
- `Profile.cshtml` - User profile pages
- `Admin.cshtml` - Administrative dashboard
- `Login.cshtml` / `Register.cshtml` - Authentication pages

**Responsibilities:**
- Handle HTTP requests and responses
- Validate input and model binding
- Call appropriate business services
- Return formatted responses (JSON for API, HTML for pages)
- Manage authentication and authorization

**Controller Example:**
```csharp
[Route("api/[controller]")]
[ApiController]
public class PostsController : ControllerBase
{
    private readonly INewsService _newsService;
    private readonly DBservices _dbService;

    public PostsController(INewsService newsService, DBservices dbService)
    {
        _newsService = newsService;
        _dbService = dbService;
    }

    [HttpGet]
    public async Task<IActionResult> GetPosts([FromQuery] int page = 1, [FromQuery] int limit = 10)
    {
        try
        {
            var articles = await _newsService.GetAllNewsArticlesAsync(page, limit);
            return Ok(new { posts = articles });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error loading posts", error = ex.Message });
        }
    }
}
```

## Dependency Injection Configuration

### Program.cs
```csharp
// Register Data Access Layer
builder.Services.AddScoped<DBservices>();

// Register Business Layer Services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<INewsService, NewsService>();
builder.Services.AddScoped<ICommentService, CommentService>();
builder.Services.AddScoped<IAdminService, AdminService>();

// External Services
builder.Services.AddScoped<INewsApiService, NewsApiService>();
builder.Services.AddScoped<IRecommendationService, RecommendationService>();

// Background Services
builder.Services.AddHostedService<NewsApiBackgroundService>();
```

## Data Flow Example

### User Registration Flow
1. **Presentation Layer**: `AuthController.Register()` receives HTTP POST
2. **Business Layer**: `UserService.CreateUserAsync()` validates business rules
3. **Data Access Layer**: `DBservices.CreateUser()` executes stored procedure
4. **Database**: `sp_Users_Insert` creates user record
5. **Response**: Success/failure flows back through all layers

### News Article Display Flow
1. **Presentation Layer**: `PostsController.GetPosts()` receives GET request
2. **Business Layer**: `NewsService.GetAllNewsArticlesAsync()` applies filters
3. **Data Access Layer**: `DBservices.GetAllNewsArticles()` queries database
4. **Database**: `sp_NewsArticles_GetAll` returns paginated results
5. **Response**: JSON array of articles returned to client

## Database Integration

### Stored Procedures Location: `/Database2025/` and `/StoredProcedures/`

**Key Stored Procedures:**
- `sp_Users_Insert`, `sp_Users_Update`, `sp_Users_Get`
- `sp_NewsArticles_GetAll`, `sp_NewsArticles_GetById`, `sp_NewsArticles_Insert`
- `sp_Comments_GetByPostId`, `sp_Comments_Insert`
- `sp_Admin_GetDashboardStats`, `sp_Admin_GetUsers`

**Benefits of Stored Procedures:**
- Better performance through pre-compilation
- Enhanced security (reduced SQL injection risk)
- Centralized business logic for database operations
- Easier maintenance and version control

## Security Implementation

### Authentication Flow
1. User submits credentials to `AuthController.Login()`
2. `UserService` validates credentials against database
3. JWT token generated and returned
4. Subsequent requests include JWT in Authorization header
5. Controllers validate JWT using middleware

### Authorization Levels
- **Public**: News viewing, user registration
- **Authenticated**: Commenting, liking, saving articles
- **Admin**: User management, system administration

## Error Handling Strategy

### Layer-Specific Error Handling
- **DAL**: Database exceptions, connection issues
- **BL**: Business rule violations, validation errors
- **Presentation**: HTTP status codes, user-friendly messages

### Example Error Flow
```csharp
try
{
    var result = await _userService.CreateUserAsync(user);
    return Ok(result);
}
catch (ArgumentException ex)
{
    return BadRequest(ex.Message);
}
catch (InvalidOperationException ex)
{
    return Conflict(ex.Message);
}
catch (Exception ex)
{
    return StatusCode(500, "Internal server error");
}
```

## Best Practices Implemented

1. **Separation of Concerns**: Each layer has distinct responsibilities
2. **Dependency Injection**: Loose coupling between components
3. **Interface-Based Design**: Testable and maintainable code
4. **Async/Await Pattern**: Non-blocking operations
5. **Input Validation**: Multiple validation layers
6. **Error Handling**: Comprehensive exception management
7. **Configuration Management**: Environment-specific settings

## Testing Strategy

### Unit Testing (Recommended)
- **Business Layer**: Test service logic with mocked dependencies
- **Data Layer**: Test database operations with test database
- **Controllers**: Test API endpoints with mocked services

### Integration Testing
- End-to-end testing of complete user workflows
- Database integration testing
- API testing with real HTTP requests

This architecture provides a solid foundation for enterprise-level applications with clear separation of concerns, maintainability, and scalability.
