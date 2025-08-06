using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using NewsSite.Services;
using NewsSite.BackgroundServices;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddRazorPages();
builder.Services.AddAuthorization();

// Add Memory Cache for background service control
builder.Services.AddMemoryCache();

// Configure System Settings Options
builder.Services.Configure<NewsSitePro.Models.SystemSettingsOptions>(
    builder.Configuration.GetSection(NewsSitePro.Models.SystemSettingsOptions.SectionName));

// Register DBservices for dependency injection
builder.Services.AddScoped<DBservices>();

// ---------------- What is Dependency Injection? ----------------
// Dependency Injection (DI) is a way for the code to get the objects it needs (like services or helpers)
// without creating them directly. Instead, you "ask" for what you need, and ASP.NET Core gives it to you.
// This makes the code easier to change, test, and reuse. For example, if you need to use a news service,
// you just ask for INewsService, and the system provides the right version for you.
//
// In the lines below, we tell ASP.NET Core which classes to use for each interface or service.
// When the controllers or other classes need something, DI will automatically provide it.
// ---------------------------------------------------------------
// Register Business Layer Services
builder.Services.AddScoped<NewsSite.BL.Services.NotificationService>();
builder.Services.AddScoped<NewsSite.BL.Services.IUserService, NewsSite.BL.Services.UserService>();
builder.Services.AddScoped<NewsSite.BL.Services.INewsService, NewsSite.BL.Services.NewsService>();
builder.Services.AddScoped<NewsSite.BL.Services.ICommentService, NewsSite.BL.Services.CommentService>();
builder.Services.AddScoped<NewsSite.BL.Services.IAdminService, NewsSite.BL.Services.AdminService>();
builder.Services.AddScoped<NewsSite.BL.Interfaces.IRepostService, NewsSite.BL.Services.RepostService>();
builder.Services.AddScoped<NewsSite.BL.Services.IUserBlockService, NewsSite.BL.Services.UserBlockService>();
builder.Services.AddScoped<NewsSite.BL.Services.IGoogleOAuthService, NewsSite.BL.Services.GoogleOAuthService>();
// PostService removed - using NewsService directly for article operations

// Register HttpClient for News API
builder.Services.AddHttpClient<INewsApiService, NewsApiService>();

// Register News API Service
builder.Services.AddScoped<INewsApiService, NewsApiService>();

// Register Recommendation Service
builder.Services.AddScoped<IRecommendationService, RecommendationService>();

builder.Services.AddHostedService<NewsApiBackgroundService>();


// Register background service that runs in the background and fetches news articles automatically
builder.Services.AddHostedService<NewsApiBackgroundService>();

// Register background service that calculates trending topics periodically
builder.Services.AddHostedService<TrendingTopicsBackgroundService>();

// Register API Configuration Service for accessing API settings and context
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<NewsSite.Services.IApiConfigurationService, NewsSite.Services.ApiConfigurationService>();

// Configure authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(options =>
    {
        var jwtSettings = builder.Configuration.GetSection("Jwt");
        var key = jwtSettings["Key"];
        var issuer = jwtSettings["Issuer"];
        var audience = jwtSettings["Audience"];

        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                System.Text.Encoding.UTF8.GetBytes(key)),
            ClockSkew = TimeSpan.Zero // Reduce clock skew to zero
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var token = context.Request.Cookies["jwtToken"];
                if (!string.IsNullOrEmpty(token))
                {
                    context.Token = token;
                }
                return Task.CompletedTask;
            },
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine($"JWT Authentication failed: {context.Exception.Message}");
                return Task.CompletedTask;
            }
        };
    });


var app = builder.Build();

// Configure the HTTP request pipeline.
if (true)
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{

}

app.UseHttpsRedirection();

// If deploying to a subdirectory, set the base path for all routes
var pathBase = builder.Configuration["PathBase"];
if (!string.IsNullOrEmpty(pathBase))
{
    app.UsePathBase(pathBase);
}

// Enable Cookie Policy middleware
app.UseCookiePolicy(new CookiePolicyOptions
{
    MinimumSameSitePolicy = SameSiteMode.Lax,
    Secure = CookieSecurePolicy.Always,
    
});


// Order is important! Authentication must come before Authorization
app.UseAuthentication();
app.UseAuthorization();


app.UseCors(policy => policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());


// Serve static files (CSS, JS, images, etc.)
app.UseStaticFiles();

// Map controller routes (API endpoints)
app.MapControllers();

// Map Razor Pages routes (for server-rendered pages)
app.MapRazorPages();

app.Run();
