# Google OAuth Setup Guide

## Overview
This project now includes Google OAuth authentication that allows users to sign in or register using their Google accounts. The implementation includes session management with expiration tracking.

## Features Added
- Google OAuth login/registration
- Session management with automatic expiration
- Login/logout time tracking
- Device and IP address tracking
- Session statistics and analytics
- Admin session management tools

## Configuration Required

### 1. Google Console Setup
1. Go to [Google Cloud Console](https://console.cloud.google.com/)
2. Create a new project or select existing one
3. Enable the Google+ API
4. Go to "Credentials" and create "OAuth 2.0 Client IDs"
5. Set authorized redirect URIs:
   - Development: `https://localhost:7026/GoogleOAuthCallback`
   - Production: `https://yourproductionsite.com/GoogleOAuthCallback`

### 2. Update appsettings.json
Replace the placeholder values in `appsettings.json`:

```json
{
  "GoogleOAuth": {
    "ClientId": "YOUR_ACTUAL_GOOGLE_CLIENT_ID",
    "ClientSecret": "YOUR_ACTUAL_GOOGLE_CLIENT_SECRET",
    "RedirectUri": "https://localhost:7026/GoogleOAuthCallback",
    "RedirectUriProduction": "https://yourproductionsite.com/GoogleOAuthCallback"
  }
}
```

### 3. Database Setup
Run the SQL script to set up Google OAuth tables and procedures:
```bash
# Execute the google_oauth_and_sessions_setup.sql file in your database
```

## Database Changes Made

### New Tables:
1. **NewsSitePro2025_UserSessions** - Tracks user login sessions
2. **NewsSitePro2025_OAuthTokens** - Stores Google OAuth tokens

### New Columns in Users Table:
- GoogleId - Google user identifier
- GoogleEmail - User's Google email
- IsGoogleUser - Flag indicating Google user
- LastLoginTime - Last login timestamp
- GoogleRefreshToken - Google refresh token

### New Stored Procedures:
- `NewsSitePro2025_sp_Google_CreateOrGetUser` - Create or link Google user
- `NewsSitePro2025_sp_UserSessions_Create` - Create user session
- `NewsSitePro2025_sp_UserSessions_Validate` - Validate session
- `NewsSitePro2025_sp_UserSessions_Logout` - Logout session  
- `NewsSitePro2025_sp_UserSessions_Cleanup` - Clean expired sessions
- `NewsSitePro2025_sp_OAuthTokens_Store` - Store OAuth tokens
- `NewsSitePro2025_sp_OAuthTokens_Get` - Retrieve OAuth tokens
- `NewsSitePro2025_sp_UserSessions_GetHistory` - Get login history

## New API Endpoints

### GoogleOAuth Controller (`/api/GoogleOAuth/`):
- `GET auth-url` - Get Google OAuth authentication URL
- `POST callback` - Handle OAuth callback
- `POST validate-session` - Validate user session
- `POST logout` - Logout user
- `GET login-history` - Get user login history (requires auth)
- `GET session-stats` - Get session statistics (requires auth)
- `POST cleanup-sessions` - Clean expired sessions (requires auth)

## How It Works

### Login/Register Flow:
1. User clicks "Continue with Google" button
2. System redirects to Google OAuth consent screen
3. User authorizes the application
4. Google redirects to `/GoogleOAuthCallback` with authorization code
5. System exchanges code for access token and user info
6. System creates or links user account
7. Creates user session with expiration
8. Generates JWT token and redirects to feed

### Session Management:
- Sessions automatically expire after 24 hours (configurable)
- Last activity time is tracked and updated
- Multiple sessions per user are supported
- Sessions can be manually terminated
- Expired sessions are automatically cleaned up

## Session Analytics
The system tracks:
- Login/logout times
- Session duration
- Device information
- IP addresses  
- User agents
- Session statistics

## Security Features
- Unique Google ID mapping
- Session token validation
- Automatic session expiration
- OAuth token refresh capability
- Device and location tracking
- Secure JWT token generation

## Testing
1. Update the Google OAuth credentials in appsettings.json
2. Run the database setup script
3. Start the application
4. Navigate to `/Login` or `/Register`
5. Click "Continue with Google"
6. Complete OAuth flow

## Troubleshooting

### Common Issues:
1. **Redirect URI mismatch**: Ensure the redirect URI in Google Console matches exactly
2. **Invalid client credentials**: Double-check ClientId and ClientSecret
3. **Database errors**: Ensure all SQL scripts have been executed
4. **Session expiration**: Check session expiry settings in database

### Error Logs:
Check application logs for detailed error information during OAuth flow.

## Production Deployment Notes
1. Update `RedirectUriProduction` in appsettings.json
2. Configure HTTPS properly
3. Set up session cleanup background service
4. Monitor session statistics for performance
5. Consider implementing session limits per user

## Files Modified/Added:
- `BL/GoogleOAuthModels.cs` - OAuth models
- `BL/Services/GoogleOAuthService.cs` - OAuth service implementation
- `BL/Extensions/ClaimsPrincipalExtensions.cs` - Authentication extensions
- `Controllers/GoogleOAuthController.cs` - OAuth API controller
- `Pages/GoogleOAuthCallback.cshtml` - OAuth callback page
- `Pages/Login.cshtml` - Updated with Google login
- `Pages/Register.cshtml` - Updated with Google registration
- `Database2025/google_oauth_and_sessions_setup.sql` - Database setup script
- `DAL/DBservices.cs` - Additional OAuth database methods
