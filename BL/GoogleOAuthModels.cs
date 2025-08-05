using System.ComponentModel.DataAnnotations;

namespace NewsSite.BL
{
    /// <summary>
    /// Google OAuth response models for handling authentication flow
    /// </summary>
    
    public class GoogleOAuthConfig
    {
        public string ClientId { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;
        public string RedirectUri { get; set; } = string.Empty;
        public string RedirectUriProduction { get; set; } = string.Empty;
    }

    public class OAuthTokenResponse
    {
        public string access_token { get; set; } = string.Empty;
        public int expires_in { get; set; }
        public string refresh_token { get; set; } = string.Empty;
        public string scope { get; set; } = string.Empty;
        public string token_type { get; set; } = string.Empty;
        public string id_token { get; set; } = string.Empty;
        public string error { get; set; } = string.Empty;
        public string error_description { get; set; } = string.Empty;

        public bool IsSuccess => string.IsNullOrEmpty(error);
    }

    public class GoogleUserInfo
    {
        public string id { get; set; } = string.Empty;
        public string email { get; set; } = string.Empty;
        public string verified_email { get; set; } = string.Empty;
        public string name { get; set; } = string.Empty;
        public string given_name { get; set; } = string.Empty;
        public string family_name { get; set; } = string.Empty;
        public string picture { get; set; } = string.Empty;
        public string locale { get; set; } = string.Empty;
        public ApiError error { get; set; } = new ApiError();

        public bool IsSuccess => error == null || error.code == 0;

        public class ApiError
        {
            public int code { get; set; }
            public string message { get; set; } = string.Empty;
            public string status { get; set; } = string.Empty;
        }
    }

    public class UserSession
    {
        public int SessionID { get; set; }
        public int UserID { get; set; }
        public string SessionToken { get; set; } = string.Empty;
        public string? DeviceInfo { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public DateTime LoginTime { get; set; }
        public DateTime LastActivityTime { get; set; }
        public DateTime ExpiryTime { get; set; }
        public bool IsActive { get; set; }
        public DateTime? LogoutTime { get; set; }
        public string? LogoutReason { get; set; }
        public int SessionDurationMinutes { get; set; }

        public bool IsExpired => DateTime.Now > ExpiryTime;
        public bool IsValid => IsActive && !IsExpired;
    }

    public class OAuthToken
    {
        public int TokenID { get; set; }
        public int UserID { get; set; }
        public string Provider { get; set; } = "Google";
        public string AccessToken { get; set; } = string.Empty;
        public string? RefreshToken { get; set; }
        public string TokenType { get; set; } = "Bearer";
        public DateTime ExpiresAt { get; set; }
        public string? Scope { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsActive { get; set; }

        public bool IsExpired => DateTime.Now > ExpiresAt;
        public bool IsValid => IsActive && !IsExpired;
    }

    // Request/Response models for API endpoints
    public class GoogleOAuthRequest
    {
        [Required]
        public string AuthorizationCode { get; set; } = string.Empty;
        public string? DeviceInfo { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
    }

    public class GoogleOAuthResponse
    {
        public bool Success { get; set; }
        public string? Token { get; set; }
        public User? User { get; set; }
        public bool IsNewUser { get; set; }
        public string? Message { get; set; }
        public UserSession? Session { get; set; }
    }

    public class SessionValidationResponse
    {
        public bool IsValid { get; set; }
        public User? User { get; set; }
        public UserSession? Session { get; set; }
        public string? Message { get; set; }
    }

    public class LoginHistoryResponse
    {
        public List<UserSession> Sessions { get; set; } = new List<UserSession>();
        public int TotalSessions { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalSessions / PageSize);
    }

    // Extended User class with Google OAuth properties
    public partial class User
    {
        public string? GoogleId { get; set; }
        public string? GoogleEmail { get; set; }
        public bool IsGoogleUser { get; set; }
        public DateTime? LastLoginTime { get; set; }
        public string? GoogleRefreshToken { get; set; }

        // Navigation properties for sessions and tokens
        public List<UserSession> Sessions { get; set; } = new List<UserSession>();
        public List<OAuthToken> OAuthTokens { get; set; } = new List<OAuthToken>();

        // Google OAuth specific methods will be added to the User class
    }

    // Request models for session management
    public class SessionRequest
    {
        public string? DeviceInfo { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public int ExpiryHours { get; set; } = 24;
    }

    public class LogoutRequest
    {
        public string SessionToken { get; set; } = string.Empty;
        public string LogoutReason { get; set; } = "Manual";
    }

    // Analytics and monitoring models
    public class SessionStats
    {
        public int TotalActiveSessions { get; set; }
        public int GoogleOAuthUsers { get; set; }
        public int RegularUsers { get; set; }
        public int ExpiredSessions { get; set; }
        public double AverageSessionDurationHours { get; set; }
        public List<DeviceStats> DeviceBreakdown { get; set; } = new List<DeviceStats>();
    }

    public class DeviceStats
    {
        public string DeviceType { get; set; } = string.Empty;
        public int Count { get; set; }
        public double Percentage { get; set; }
    }
}
