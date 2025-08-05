using System.Text.Json;
using Microsoft.Extensions.Configuration;
using System.Text;

namespace NewsSite.BL.Services
{
    public interface IGoogleOAuthService
    {
        Task<GoogleOAuthResponse> HandleGoogleOAuthAsync(GoogleOAuthRequest request);
        Task<OAuthTokenResponse> ExchangeCodeForTokensAsync(string authorizationCode);
        Task<GoogleUserInfo> GetUserInfoAsync(string accessToken);
        Task<SessionValidationResponse> ValidateSessionAsync(string sessionToken);
        Task<bool> LogoutAsync(string sessionToken, string reason = "Manual");
        Task<LoginHistoryResponse> GetUserLoginHistoryAsync(int userId, int pageNumber = 1, int pageSize = 20);
        Task<SessionStats> GetSessionStatsAsync();
        Task CleanupExpiredSessionsAsync();
        string GenerateGoogleOAuthUrl(bool isDevelopment = true);
    }

    public class GoogleOAuthService : IGoogleOAuthService
    {
        private readonly DBservices _dbService;
        private readonly IConfiguration _config;
        private readonly HttpClient _httpClient;

        public GoogleOAuthService(DBservices dbService, IConfiguration config, HttpClient httpClient)
        {
            _dbService = dbService;
            _config = config;
            _httpClient = httpClient;
        }

        public string GenerateGoogleOAuthUrl(bool isDevelopment = true)
        {
            var clientId = _config["GoogleOAuth:ClientId"];
            var redirectUri = isDevelopment 
                ? _config["GoogleOAuth:RedirectUri"]
                : _config["GoogleOAuth:RedirectUriProduction"];

            var encodedRedirectUri = Uri.EscapeDataString(redirectUri);

            return $"https://accounts.google.com/o/oauth2/v2/auth?" +
                   $"access_type=online" +
                   $"&client_id={clientId}" +
                   $"&redirect_uri={encodedRedirectUri}" +
                   $"&response_type=code" +
                   $"&scope=email%20profile" +
                   $"&prompt=consent";
        }

        public async Task<GoogleOAuthResponse> HandleGoogleOAuthAsync(GoogleOAuthRequest request)
        {
            try
            {
                Console.WriteLine($"[DEBUG] Starting OAuth flow for authorization code: {request.AuthorizationCode?.Substring(0, Math.Min(10, request.AuthorizationCode.Length))}...");

                // Step 1: Exchange authorization code for tokens
                var tokenResponse = await ExchangeCodeForTokensAsync(request.AuthorizationCode);
                if (!tokenResponse.IsSuccess)
                {
                    Console.WriteLine($"[DEBUG] Token exchange failed: {tokenResponse.error} - {tokenResponse.error_description}");
                    return new GoogleOAuthResponse
                    {
                        Success = false,
                        Message = $"Failed to exchange code for tokens: {tokenResponse.error_description}"
                    };
                }

                Console.WriteLine($"[DEBUG] Token exchange successful, getting user info...");

                // Step 2: Get user info from Google
                var userInfo = await GetUserInfoAsync(tokenResponse.access_token);
                if (!userInfo.IsSuccess)
                {
                    Console.WriteLine($"[DEBUG] User info retrieval failed");
                    return new GoogleOAuthResponse
                    {
                        Success = false,
                        Message = "Failed to retrieve user information from Google"
                    };
                }

                Console.WriteLine($"[DEBUG] User info retrieved successfully, creating/getting user in database...");

                // Step 3: Create or get user in our database
                var userResult = await _dbService.CreateOrGetGoogleUserAsync(userInfo.id, userInfo.email, userInfo.name);
                var user = userResult.User;
                var isNewUser = userResult.IsNewUser;

                Console.WriteLine($"[DEBUG] User created/retrieved - ID: {user.Id}, IsNew: {isNewUser}");

                // Step 4: Store OAuth tokens
                await _dbService.StoreOAuthTokenAsync(
                    user.Id, 
                    "Google", 
                    tokenResponse.access_token,
                    tokenResponse.refresh_token,
                    "Bearer",
                    tokenResponse.expires_in,
                    tokenResponse.scope
                );

                Console.WriteLine($"[DEBUG] OAuth tokens stored, creating session...");

                // Step 5: Create user session
                var sessionToken = GenerateSessionToken();
                var session = await _dbService.CreateUserSessionAsync(
                    user.Id,
                    sessionToken,
                    request.DeviceInfo,
                    request.IpAddress,
                    request.UserAgent,
                    24 // 24 hours expiry
                );

                Console.WriteLine($"[DEBUG] Session created, generating JWT...");

                // Step 6: Generate JWT token directly
                var jwtToken = GenerateJwtToken(user);

                Console.WriteLine($"[DEBUG] OAuth flow completed successfully");

                return new GoogleOAuthResponse
                {
                    Success = true,
                    Token = jwtToken,
                    User = user,
                    IsNewUser = isNewUser,
                    Session = session,
                    Message = isNewUser ? "Account created successfully" : "Login successful"
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DEBUG] OAuth flow exception: {ex.Message}");
                Console.WriteLine($"[DEBUG] Stack trace: {ex.StackTrace}");
                return new GoogleOAuthResponse
                {
                    Success = false,
                    Message = $"OAuth process failed: {ex.Message}"
                };
            }
        }

        public async Task<OAuthTokenResponse> ExchangeCodeForTokensAsync(string authorizationCode)
        {
            try
            {
                var clientId = _config["GoogleOAuth:ClientId"];
                var clientSecret = _config["GoogleOAuth:ClientSecret"];
                
                // Determine which redirect URI to use based on environment
                var isDevelopment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";
                var redirectUri = isDevelopment 
                    ? _config["GoogleOAuth:RedirectUri"]
                    : _config["GoogleOAuth:RedirectUriProduction"];

                Console.WriteLine($"[DEBUG] OAuth Token Exchange:");
                Console.WriteLine($"[DEBUG] Client ID: {clientId}");
                Console.WriteLine($"[DEBUG] Redirect URI: {redirectUri}");
                Console.WriteLine($"[DEBUG] Is Development: {isDevelopment}");
                Console.WriteLine($"[DEBUG] Authorization Code: {authorizationCode?.Substring(0, Math.Min(10, authorizationCode.Length))}...");

                var requestData = new Dictionary<string, string>
                {
                    { "client_id", clientId },
                    { "client_secret", clientSecret },
                    { "code", authorizationCode },
                    { "grant_type", "authorization_code" },
                    { "redirect_uri", redirectUri },
                    { "access_type", "online" }
                };

                var content = new FormUrlEncodedContent(requestData);
                var response = await _httpClient.PostAsync("https://oauth2.googleapis.com/token", content);
                var json = await response.Content.ReadAsStringAsync();

                Console.WriteLine($"[DEBUG] Google Token Response Status: {response.StatusCode}");
                Console.WriteLine($"[DEBUG] Google Token Response: {json}");

                var tokenResponse = JsonSerializer.Deserialize<OAuthTokenResponse>(json, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                Console.WriteLine($"[DEBUG] Token Response Success: {tokenResponse?.IsSuccess}");
                if (!string.IsNullOrEmpty(tokenResponse?.error))
                {
                    Console.WriteLine($"[DEBUG] Token Error: {tokenResponse.error} - {tokenResponse.error_description}");
                }

                return tokenResponse ?? new OAuthTokenResponse { error = "Failed to deserialize response" };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DEBUG] Token Exchange Exception: {ex.Message}");
                return new OAuthTokenResponse
                {
                    error = "exchange_failed",
                    error_description = ex.Message
                };
            }
        }

        public async Task<GoogleUserInfo> GetUserInfoAsync(string accessToken)
        {
            try
            {
                Console.WriteLine($"[DEBUG] Getting user info with access token: {accessToken?.Substring(0, Math.Min(10, accessToken.Length))}...");
                
                _httpClient.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

                var response = await _httpClient.GetAsync("https://www.googleapis.com/oauth2/v2/userinfo?fields=id,email,verified_email,name,given_name,family_name,picture,locale");
                var json = await response.Content.ReadAsStringAsync();

                Console.WriteLine($"[DEBUG] Google User Info Response Status: {response.StatusCode}");
                Console.WriteLine($"[DEBUG] Google User Info Response: {json}");

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"[DEBUG] User info request failed with status: {response.StatusCode}");
                    return new GoogleUserInfo
                    {
                        error = new GoogleUserInfo.ApiError
                        {
                            code = (int)response.StatusCode,
                            message = json,
                            status = response.StatusCode.ToString()
                        }
                    };
                }

                var userInfo = JsonSerializer.Deserialize<GoogleUserInfo>(json, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                Console.WriteLine($"[DEBUG] User Info Success: {userInfo?.IsSuccess}");
                Console.WriteLine($"[DEBUG] User Info - ID: {userInfo?.id}, Email: {userInfo?.email}, Name: {userInfo?.name}");

                return userInfo ?? new GoogleUserInfo 
                { 
                    error = new GoogleUserInfo.ApiError 
                    { 
                        code = 500, 
                        message = "Failed to deserialize user info" 
                    } 
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DEBUG] User Info Exception: {ex.Message}");
                return new GoogleUserInfo
                {
                    error = new GoogleUserInfo.ApiError
                    {
                        code = 500,
                        message = ex.Message,
                        status = "INTERNAL_ERROR"
                    }
                };
            }
        }

        public async Task<SessionValidationResponse> ValidateSessionAsync(string sessionToken)
        {
            try
            {
                var result = await _dbService.ValidateUserSessionAsync(sessionToken);
                return result;
            }
            catch (Exception ex)
            {
                return new SessionValidationResponse
                {
                    IsValid = false,
                    Message = $"Session validation failed: {ex.Message}"
                };
            }
        }

        public async Task<bool> LogoutAsync(string sessionToken, string reason = "Manual")
        {
            try
            {
                return await _dbService.LogoutUserSessionAsync(sessionToken, reason);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<LoginHistoryResponse> GetUserLoginHistoryAsync(int userId, int pageNumber = 1, int pageSize = 20)
        {
            try
            {
                return await _dbService.GetUserLoginHistoryAsync(userId, pageNumber, pageSize);
            }
            catch (Exception)
            {
                return new LoginHistoryResponse
                {
                    Sessions = new List<UserSession>(),
                    TotalSessions = 0,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };
            }
        }

        public async Task<SessionStats> GetSessionStatsAsync()
        {
            try
            {
                return await _dbService.GetSessionStatsAsync();
            }
            catch (Exception)
            {
                return new SessionStats();
            }
        }

        public async Task CleanupExpiredSessionsAsync()
        {
            try
            {
                await _dbService.CleanupExpiredSessionsAsync();
            }
            catch (Exception)
            {
                // Log error but don't throw
            }
        }

        private string GenerateJwtToken(User user)
        {
            var key = _config["Jwt:Key"];
            var issuer = _config["Jwt:Issuer"];
            var audience = _config["Jwt:Audience"];

            var securityKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(key));
            var credentials = new Microsoft.IdentityModel.Tokens.SigningCredentials(securityKey, Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new System.Security.Claims.Claim(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub, user.Email ?? ""),
                new System.Security.Claims.Claim("id", user.Id.ToString()),
                new System.Security.Claims.Claim("name", user.Name ?? ""),
                new System.Security.Claims.Claim("email", user.Email ?? ""),
                new System.Security.Claims.Claim("isAdmin", user.IsAdmin.ToString()),
                new System.Security.Claims.Claim("isGoogleUser", user.IsGoogleUser.ToString()),
                new System.Security.Claims.Claim(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Jti, System.Guid.NewGuid().ToString())
            };

            var token = new System.IdentityModel.Tokens.Jwt.JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: System.DateTime.Now.AddDays(30),
                signingCredentials: credentials
            );

            return new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler().WriteToken(token);
        }

        private string GenerateSessionToken()
        {
            return Guid.NewGuid().ToString() + "_" + DateTime.UtcNow.Ticks.ToString();
        }
    }
}
