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

// Register DBservices for dependency injection
builder.Services.AddScoped<DBservices>();

// Register Business Layer Services
builder.Services.AddScoped<NewsSite.BL.Services.IUserService, NewsSite.BL.Services.UserService>();
builder.Services.AddScoped<NewsSite.BL.Services.INewsService, NewsSite.BL.Services.NewsService>();
builder.Services.AddScoped<NewsSite.BL.Services.ICommentService, NewsSite.BL.Services.CommentService>();
builder.Services.AddScoped<NewsSite.BL.Services.IAdminService, NewsSite.BL.Services.AdminService>();

// Register HttpClient for News API
builder.Services.AddHttpClient<INewsApiService, NewsApiService>();

// Register News API Service
builder.Services.AddScoped<INewsApiService, NewsApiService>();

// Register Recommendation Service
builder.Services.AddScoped<IRecommendationService, RecommendationService>();

// Register Background Service for automatic news fetching
builder.Services.AddHostedService<NewsApiBackgroundService>();

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

app.UseHttpsRedirection();

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
