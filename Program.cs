using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using NewsSite.Services;
using NewsSite.BackgroundServices;

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

// Register DBservices for dependency injection
builder.Services.AddScoped<DBservices>();

// Register Business Layer Services
builder.Services.AddScoped<NewsSite.BL.Services.NotificationService>();
builder.Services.AddScoped<NewsSite.BL.Services.IUserService, NewsSite.BL.Services.UserService>();
builder.Services.AddScoped<NewsSite.BL.Services.INewsService, NewsSite.BL.Services.NewsService>();
builder.Services.AddScoped<NewsSite.BL.Services.ICommentService, NewsSite.BL.Services.CommentService>();
builder.Services.AddScoped<NewsSite.BL.Services.IAdminService, NewsSite.BL.Services.AdminService>();
// PostService removed - using NewsService directly for article operations

// Register HttpClient for News API
builder.Services.AddHttpClient<INewsApiService, NewsApiService>();

// Register News API Service
builder.Services.AddScoped<INewsApiService, NewsApiService>();

// Register Recommendation Service
builder.Services.AddScoped<IRecommendationService, RecommendationService>();

// Register Background Service for automatic news fetching
builder.Services.AddHostedService<NewsApiBackgroundService>();

// Register Background Service for trending topics calculation
builder.Services.AddHostedService<TrendingTopicsBackgroundService>();

// Configure authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = "YourIssuer",
            ValidAudience = "YourAudience",
            IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                System.Text.Encoding.UTF8.GetBytes("YourSecretKey"))
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
            }
        };
    });


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    // Configure Swagger for production with proper base path
    app.UseSwagger(c =>
    {
        c.RouteTemplate = "swagger/{documentName}/swagger.json";
        c.PreSerializeFilters.Add((swaggerDoc, httpReq) =>
        {
            var pathBase = httpReq.PathBase.ToString();
            if (!string.IsNullOrEmpty(pathBase))
            {
                swaggerDoc.Servers = new List<Microsoft.OpenApi.Models.OpenApiServer>
                {
                    new Microsoft.OpenApi.Models.OpenApiServer { Url = pathBase }
                };
            }
        });
    });
    
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("./swagger/v1/swagger.json", "NewsSitePro API V1");
        c.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();

// Configure for subdirectory deployment
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

// Order is important!
app.UseAuthentication(); // Add this before UseAuthorization
app.UseAuthorization();


app.UseCors(policy => policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());

app.UseStaticFiles();
app.MapControllers();
app.MapRazorPages();

app.Run();
