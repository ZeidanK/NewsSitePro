using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens; // namespace for SymmetricSecurityKey
using System.IdentityModel.Tokens.Jwt; //  namespace for JwtSecurityToken and JwtSecurityTokenHandler
using Microsoft.AspNetCore.Authorization; //  namespace for Claim
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace NewsSite.BL
{
    public class User
    {

        private int id;
        private string? name;
        private string? email;
        private string? passwordHash;
        private bool isAdmin;
        private bool isLocked;
        private string? bio;
        private DateTime joinDate;
        private string? profilePicture;
        private readonly object? _config;

        public User(IConfiguration config)
        {
            _config = config;
        }

        public int Id { get => id; set => id = value; }
        public string? Name { get => name; set => name = value; }
        public string? Email { get => email; set => email = value; }
        public string? PasswordHash { get => passwordHash; set => passwordHash = value; }
        public bool IsAdmin { get => isAdmin; set => isAdmin = value; }
        public bool IsLocked { get => isLocked; set => isLocked = value; }
        public string? Bio { get => bio; set => bio = value; }
        public DateTime JoinDate { get => joinDate; set => joinDate = value; }
        public string? ProfilePicture { get => profilePicture; set => profilePicture = value; }



        // Constructors

        public User() { }

        public User(int id, string? name, string? email, string? passwordHash, bool isAdmin, bool isLocked, string? bio = null, DateTime? joinDate = null)
        {
            this.id = id;
            this.name = name;
            this.email = email;
            this.passwordHash = passwordHash;
            this.isAdmin = isAdmin;
            this.isLocked = isLocked;
            this.bio = bio;
            this.joinDate = joinDate ?? DateTime.Now;
        }



        public bool Register(string name, string email, string password)
        {
            DBservices dBservices = new DBservices();
            // Check if user already exists
            var existing = dBservices.GetUser(email, null, null);
            if (existing != null) return false;

            this.Name = name;
            this.Email = email;
            this.PasswordHash = HashPassword(password);
            this.IsAdmin = false;
            this.IsLocked = false;

            // Save user to DB
            return dBservices.CreateUser(this);
        }

        public bool UpdateDetails(int id, string name, string password)
        {
            DBservices dBservices = new DBservices();
            var user = dBservices.GetUserById(id);
            if (user == null) return false;

            user.Name = name ?? user.Name;
            if (!string.IsNullOrEmpty(password))
                user.PasswordHash = HashPassword(password);

            return dBservices.UpdateUser(user);
        }

        public string LogIn(string password, string email)
        {
            DBservices dBservices = new DBservices();
            // Retrieve user by email
            var user = dBservices.GetUser(email, null, null);
            if (user == null)
                return null; // User not found

            // Check if user is locked
            if (user.IsLocked)
                return null; // User is locked

            // Hash the provided password and compare
            var hashedInput = HashPassword(password);
            if (user.PasswordHash != hashedInput)
                return null; // Password does not match

            // Set current user properties
            this.Id = user.Id;
            this.Name = user.Name;
            this.Email = user.Email;
            this.PasswordHash = user.PasswordHash;
            this.IsAdmin = user.IsAdmin;
            this.IsLocked = user.IsLocked;

            // Generate JWT
            return GenerateJwtToken();
        }

        // Static Methods

        public User ExtractUserFromJWT(string jwt)
        {
            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(jwt);

            var idClaim = token.Claims.FirstOrDefault(c => c.Type == "id");
            var nameClaim = token.Claims.FirstOrDefault(c => c.Type == "name");
            var emailClaim = token.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub);
            var isAdminClaim = token.Claims.FirstOrDefault(c => c.Type == "isAdmin");

            if (idClaim != null)
                this.Id = int.Parse(idClaim.Value);

            if (nameClaim != null)
                this.Name = nameClaim.Value;

            if (emailClaim != null)
                this.Email = emailClaim.Value;

            if (isAdminClaim != null)
                this.IsAdmin = bool.Parse(isAdminClaim.Value);

            // PasswordHash and IsLocked are not present in JWT, so they remain unchanged

            return this;
        }


        private string GenerateJwtToken()
        {
            var key = ((IConfiguration)_config)["Jwt:Key"];
            var issuer = ((IConfiguration)_config)["Jwt:Issuer"];
            var audience = ((IConfiguration)_config)["Jwt:Audience"];

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
    {
        new Claim(JwtRegisteredClaimNames.Sub, this.Email), // Standard subject claim
        new Claim("id", this.Id.ToString()), // Standard name identifier
        new Claim("name", this.Name),

        new Claim("isAdmin", this.IsAdmin.ToString()),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }





        public static string HashPassword(string password)
        {
            using (var sha512 = SHA512.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(password);
                var hash = sha512.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }

        // Proper JWT authentication method - can be used across all controllers
        public static int? GetCurrentUserId(Microsoft.AspNetCore.Http.HttpRequest request, ClaimsPrincipal? user = null)
        {
            try
            {
                // Try to get from JWT claims first (if user context is available)
                if (user != null)
                {
                    var userIdClaim = user.FindFirst("id");
                    if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
                    {
                        return userId;
                    }
                }

                // Try to get from Authorization header
                var authHeader = request.Headers["Authorization"].FirstOrDefault();
                if (authHeader != null && authHeader.StartsWith("Bearer "))
                {
                    var token = authHeader.Substring("Bearer ".Length).Trim();
                    var handler = new JwtSecurityTokenHandler();

                    if (handler.CanReadToken(token))
                    {
                        var jsonToken = handler.ReadJwtToken(token);
                        var idClaim = jsonToken.Claims.FirstOrDefault(x => x.Type == "id");

                        if (idClaim != null && int.TryParse(idClaim.Value, out int tokenUserId))
                        {
                            return tokenUserId;
                        }
                    }
                }

                // Fallback: check for JWT token in cookies
                var jwtCookie = request.Cookies["jwtToken"];
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
            catch (Exception)
            {
                return null;
            }
        }

        // public int GetUserFollowersCount(int userId)
        // {
        //     DBservices dBservices = new DBservices();
        //     return dBservices.GetUserFollowersCount(userId);
        // }
        // public int GetUserFollowingCount(int userId)
        // {
        //     DBservices dBservices = new DBservices();
        //     return dBservices.GetUserFollowingCount(userId);
        // }

        // public List<User> GetUserFollowers(int userId)
        // {
        //     DBservices dBservices = new DBservices();
        //     return dBservices.GetUserFollowers(userId);
        // }

        // public List<User> GetUserFollowing(int userId)
        // {
        //     DBservices dBservices = new DBservices();
        //     return dBservices.GetUserFollowing(userId);
        // }
        // public bool FollowUser(int followerId, int followedId)
        // {
        //     DBservices dBservices = new DBservices();
        //     return dBservices.FollowUser(followerId, followedId);
        // }
        // public bool UnfollowUser(int followerId, int followedId)
        // {
        //     DBservices dBservices = new DBservices();
        //     return dBservices.UnfollowUser(followerId, followedId);
        // }
        // public int GetUserPostsCount(int userId)
        // {
        //     DBservices dBservices = new DBservices();
        //     return dBservices.GetUserPostsCount(userId);
        // }

    
    }

    // Admin-specific user view models
    public class AdminUserView
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public string? ProfilePicture { get; set; }
    public DateTime JoinDate { get; set; }
    public DateTime? LastActivity { get; set; }
    public string Status { get; set; } = string.Empty;
    public int PostCount { get; set; }
    public int LikesReceived { get; set; }
    public bool IsAdmin { get; set; }
}

    public class AdminUserDetails : AdminUserView
    {
        public string? Bio { get; set; }
        public DateTime? BannedUntil { get; set; }
        public string? BanReason { get; set; }
        public List<ActivityLog> RecentActivity { get; set; } = new List<ActivityLog>();
        public List<Report> Reports { get; set; } = new List<Report>();
    }

    public class ActivityLog
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string IpAddress { get; set; } = string.Empty;
        public string UserAgent { get; set; } = string.Empty;
    }

    public class UserReport
    {
        public int Id { get; set; }
        public int ReporterId { get; set; }
        public string ReporterUsername { get; set; } = string.Empty;
        public int ReportedUserId { get; set; }
        public string ReportedUsername { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string Status { get; set; } = "Pending"; // Pending, Resolved, Dismissed
        public int? ResolvedBy { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public string? ResolutionNotes { get; set; }
    }

    public class AdminDashboardStats
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int BannedUsers { get; set; }
        public int TotalPosts { get; set; }
        public int TotalReports { get; set; }
        public int PendingReports { get; set; }
        public int TodayRegistrations { get; set; }
        public int TodayPosts { get; set; }
    }
}
