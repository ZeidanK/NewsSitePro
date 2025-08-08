-- =============================================
-- Google OAuth Login and Session Management Setup - SIMPLE VERSION
-- Run this script step by step or all at once
-- =============================================

-- Step 1: Check and add Google OAuth columns to existing Users table
PRINT 'Adding Google OAuth columns to Users table...';

-- Add GoogleId column
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('NewsSitePro2025_Users') AND name = 'GoogleId')
BEGIN
    ALTER TABLE NewsSitePro2025_Users ADD GoogleId NVARCHAR(100) NULL;
    PRINT 'Added GoogleId column';
END
ELSE
    PRINT 'GoogleId column already exists';

-- Add GoogleEmail column
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('NewsSitePro2025_Users') AND name = 'GoogleEmail')
BEGIN
    ALTER TABLE NewsSitePro2025_Users ADD GoogleEmail NVARCHAR(256) NULL;
    PRINT 'Added GoogleEmail column';
END
ELSE
    PRINT 'GoogleEmail column already exists';

-- Add IsGoogleUser column
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('NewsSitePro2025_Users') AND name = 'IsGoogleUser')
BEGIN
    ALTER TABLE NewsSitePro2025_Users ADD IsGoogleUser BIT NOT NULL DEFAULT 0;
    PRINT 'Added IsGoogleUser column';
END
ELSE
    PRINT 'IsGoogleUser column already exists';

-- Add LastLoginTime column
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('NewsSitePro2025_Users') AND name = 'LastLoginTime')
BEGIN
    ALTER TABLE NewsSitePro2025_Users ADD LastLoginTime DATETIME2 NULL;
    PRINT 'Added LastLoginTime column';
END
ELSE
    PRINT 'LastLoginTime column already exists';

-- Add GoogleRefreshToken column
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('NewsSitePro2025_Users') AND name = 'GoogleRefreshToken')
BEGIN
    ALTER TABLE NewsSitePro2025_Users ADD GoogleRefreshToken NVARCHAR(500) NULL;
    PRINT 'Added GoogleRefreshToken column';
END
ELSE
    PRINT 'GoogleRefreshToken column already exists';

-- Create unique index for Google ID
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID('NewsSitePro2025_Users') AND name = 'IX_Users_GoogleId')
BEGIN
    CREATE UNIQUE INDEX IX_Users_GoogleId ON NewsSitePro2025_Users(GoogleId) WHERE GoogleId IS NOT NULL;
    PRINT 'Created unique index for GoogleId';
END
ELSE
    PRINT 'GoogleId index already exists';

GO

-- Step 2: Create UserSessions table
PRINT 'Creating UserSessions table...';

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID('NewsSitePro2025_UserSessions') AND type = 'U')
BEGIN
    CREATE TABLE NewsSitePro2025_UserSessions (
        SessionID INT IDENTITY(1,1) PRIMARY KEY,
        UserID INT NOT NULL,
        SessionToken NVARCHAR(500) NOT NULL,
        DeviceInfo NVARCHAR(500) NULL,
        IpAddress NVARCHAR(45) NULL,
        UserAgent NVARCHAR(1000) NULL,
        LoginTime DATETIME2 NOT NULL DEFAULT GETDATE(),
        LastActivityTime DATETIME2 NOT NULL DEFAULT GETDATE(),
        ExpiryTime DATETIME2 NOT NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        LogoutTime DATETIME2 NULL,
        LogoutReason NVARCHAR(100) NULL,
        CONSTRAINT FK_UserSessions_Users FOREIGN KEY (UserID) REFERENCES NewsSitePro2025_Users(UserID) ON DELETE CASCADE
    );
    PRINT 'Created NewsSitePro2025_UserSessions table';
END
ELSE
    PRINT 'NewsSitePro2025_UserSessions table already exists';

-- Create indexes for UserSessions
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID('NewsSitePro2025_UserSessions') AND name = 'IX_UserSessions_UserID')
    CREATE INDEX IX_UserSessions_UserID ON NewsSitePro2025_UserSessions(UserID);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID('NewsSitePro2025_UserSessions') AND name = 'IX_UserSessions_SessionToken')
    CREATE INDEX IX_UserSessions_SessionToken ON NewsSitePro2025_UserSessions(SessionToken);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID('NewsSitePro2025_UserSessions') AND name = 'IX_UserSessions_IsActive')
    CREATE INDEX IX_UserSessions_IsActive ON NewsSitePro2025_UserSessions(IsActive);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID('NewsSitePro2025_UserSessions') AND name = 'IX_UserSessions_ExpiryTime')
    CREATE INDEX IX_UserSessions_ExpiryTime ON NewsSitePro2025_UserSessions(ExpiryTime);

GO

-- Step 3: Create OAuthTokens table
PRINT 'Creating OAuthTokens table...';

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID('NewsSitePro2025_OAuthTokens') AND type = 'U')
BEGIN
    CREATE TABLE NewsSitePro2025_OAuthTokens (
        TokenID INT IDENTITY(1,1) PRIMARY KEY,
        UserID INT NOT NULL,
        Provider NVARCHAR(50) NOT NULL DEFAULT 'Google',
        AccessToken NVARCHAR(2000) NOT NULL,
        RefreshToken NVARCHAR(500) NULL,
        TokenType NVARCHAR(50) NOT NULL DEFAULT 'Bearer',
        ExpiresAt DATETIME2 NOT NULL,
        Scope NVARCHAR(500) NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
        UpdatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
        IsActive BIT NOT NULL DEFAULT 1,
        CONSTRAINT FK_OAuthTokens_Users FOREIGN KEY (UserID) REFERENCES NewsSitePro2025_Users(UserID) ON DELETE CASCADE
    );
    PRINT 'Created NewsSitePro2025_OAuthTokens table';
END
ELSE
    PRINT 'NewsSitePro2025_OAuthTokens table already exists';

-- Create indexes for OAuthTokens
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID('NewsSitePro2025_OAuthTokens') AND name = 'IX_OAuthTokens_UserID')
    CREATE INDEX IX_OAuthTokens_UserID ON NewsSitePro2025_OAuthTokens(UserID);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID('NewsSitePro2025_OAuthTokens') AND name = 'IX_OAuthTokens_Provider')
    CREATE INDEX IX_OAuthTokens_Provider ON NewsSitePro2025_OAuthTokens(Provider);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID('NewsSitePro2025_OAuthTokens') AND name = 'IX_OAuthTokens_IsActive')
    CREATE INDEX IX_OAuthTokens_IsActive ON NewsSitePro2025_OAuthTokens(IsActive);

GO

-- Step 4: Drop existing procedures if they exist
PRINT 'Cleaning up existing procedures...';

IF OBJECT_ID('NewsSitePro2025_sp_Google_CreateOrGetUser', 'P') IS NOT NULL
    DROP PROCEDURE NewsSitePro2025_sp_Google_CreateOrGetUser;

IF OBJECT_ID('NewsSitePro2025_sp_UserSessions_Create', 'P') IS NOT NULL
    DROP PROCEDURE NewsSitePro2025_sp_UserSessions_Create;

IF OBJECT_ID('NewsSitePro2025_sp_UserSessions_Validate', 'P') IS NOT NULL
    DROP PROCEDURE NewsSitePro2025_sp_UserSessions_Validate;

IF OBJECT_ID('NewsSitePro2025_sp_UserSessions_Logout', 'P') IS NOT NULL
    DROP PROCEDURE NewsSitePro2025_sp_UserSessions_Logout;

IF OBJECT_ID('NewsSitePro2025_sp_UserSessions_Cleanup', 'P') IS NOT NULL
    DROP PROCEDURE NewsSitePro2025_sp_UserSessions_Cleanup;

IF OBJECT_ID('NewsSitePro2025_sp_OAuthTokens_Store', 'P') IS NOT NULL
    DROP PROCEDURE NewsSitePro2025_sp_OAuthTokens_Store;

IF OBJECT_ID('NewsSitePro2025_sp_OAuthTokens_Get', 'P') IS NOT NULL
    DROP PROCEDURE NewsSitePro2025_sp_OAuthTokens_Get;

IF OBJECT_ID('NewsSitePro2025_sp_UserSessions_GetHistory', 'P') IS NOT NULL
    DROP PROCEDURE NewsSitePro2025_sp_UserSessions_GetHistory;

IF OBJECT_ID('vw_ActiveUserSessions', 'V') IS NOT NULL
    DROP VIEW vw_ActiveUserSessions;

IF OBJECT_ID('tr_UserSessions_UpdateUserActivity', 'TR') IS NOT NULL
    DROP TRIGGER tr_UserSessions_UpdateUserActivity;

GO

-- Step 5: Create all stored procedures
PRINT 'Creating stored procedures...';

-- Procedure to create or get user by Google ID
CREATE PROCEDURE NewsSitePro2025_sp_Google_CreateOrGetUser
    @GoogleId NVARCHAR(100),
    @GoogleEmail NVARCHAR(256),
    @Username NVARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @UserID INT;
    DECLARE @IsNewUser BIT = 0;
    
    -- Check if user exists with this Google ID
    SELECT @UserID = UserID 
    FROM NewsSitePro2025_Users 
    WHERE GoogleId = @GoogleId;
    
    IF @UserID IS NULL
    BEGIN
        -- Check if user exists with this email (for linking existing accounts)
        SELECT @UserID = UserID 
        FROM NewsSitePro2025_Users 
        WHERE Email = @GoogleEmail;
        
        IF @UserID IS NOT NULL
        BEGIN
            -- Link existing account to Google
            UPDATE NewsSitePro2025_Users 
            SET GoogleId = @GoogleId,
                GoogleEmail = @GoogleEmail,
                IsGoogleUser = 1,
                LastLoginTime = GETDATE()
            WHERE UserID = @UserID;
        END
        ELSE
        BEGIN
            -- Create new user
            SET @IsNewUser = 1;
            
            INSERT INTO NewsSitePro2025_Users (
                Username, Email, GoogleId, GoogleEmail, IsGoogleUser, 
                IsActive, IsAdmin, IsLocked, IsBanned, JoinDate, LastLoginTime
            )
            VALUES (
                ISNULL(@Username, LEFT(@GoogleEmail, CHARINDEX('@', @GoogleEmail) - 1)),
                @GoogleEmail, @GoogleId, @GoogleEmail, 1,
                1, 0, 0, 0, GETDATE(), GETDATE()
            );
            
            SET @UserID = SCOPE_IDENTITY();
        END
    END
    ELSE
    BEGIN
        -- Update last login time for existing Google user
        UPDATE NewsSitePro2025_Users 
        SET LastLoginTime = GETDATE()
        WHERE UserID = @UserID;
    END
    
    -- Return user information
    SELECT 
        u.UserID,
        u.Username,
        u.Email,
        u.GoogleId,
        u.GoogleEmail,
        u.IsGoogleUser,
        u.IsActive,
        u.IsAdmin,
        u.IsLocked,
        u.IsBanned,
        u.BannedUntil,
        u.ProfilePicture,
        u.Bio,
        u.JoinDate,
        u.LastLoginTime,
        @IsNewUser AS IsNewUser
    FROM NewsSitePro2025_Users u
    WHERE u.UserID = @UserID;
END
GO

-- Procedure to create user session
CREATE PROCEDURE NewsSitePro2025_sp_UserSessions_Create
    @UserID INT,
    @SessionToken NVARCHAR(500),
    @DeviceInfo NVARCHAR(500) = NULL,
    @IpAddress NVARCHAR(45) = NULL,
    @UserAgent NVARCHAR(1000) = NULL,
    @ExpiryHours INT = 24
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @ExpiryTime DATETIME2 = DATEADD(HOUR, @ExpiryHours, GETDATE());
    
    -- Deactivate any existing active sessions for this user
    UPDATE NewsSitePro2025_UserSessions 
    SET IsActive = 0, 
        LogoutTime = GETDATE(),
        LogoutReason = 'NewSession'
    WHERE UserID = @UserID AND IsActive = 1;
    
    -- Create new session
    INSERT INTO NewsSitePro2025_UserSessions (
        UserID, SessionToken, DeviceInfo, IpAddress, UserAgent, 
        LoginTime, LastActivityTime, ExpiryTime, IsActive
    )
    VALUES (
        @UserID, @SessionToken, @DeviceInfo, @IpAddress, @UserAgent,
        GETDATE(), GETDATE(), @ExpiryTime, 1
    );
    
    SELECT SCOPE_IDENTITY() AS SessionID, @ExpiryTime AS ExpiryTime;
END
GO

-- Procedure to validate and update session
CREATE PROCEDURE NewsSitePro2025_sp_UserSessions_Validate
    @SessionToken NVARCHAR(500)
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @UserID INT;
    DECLARE @SessionID INT;
    DECLARE @IsValid BIT = 0;
    
    -- Check if session exists and is valid
    SELECT 
        @SessionID = s.SessionID,
        @UserID = s.UserID,
        @IsValid = CASE 
            WHEN s.IsActive = 1 AND s.ExpiryTime > GETDATE() THEN 1 
            ELSE 0 
        END
    FROM NewsSitePro2025_UserSessions s
    WHERE s.SessionToken = @SessionToken;
    
    IF @IsValid = 1
    BEGIN
        -- Update last activity time
        UPDATE NewsSitePro2025_UserSessions 
        SET LastActivityTime = GETDATE()
        WHERE SessionID = @SessionID;
        
        -- Return user and session info
        SELECT 
            u.UserID,
            u.Username,
            u.Email,
            u.IsActive,
            u.IsAdmin,
            u.IsLocked,
            u.IsBanned,
            u.BannedUntil,
            u.ProfilePicture,
            u.Bio,
            u.IsGoogleUser,
            s.SessionID,
            s.LoginTime,
            s.LastActivityTime,
            s.ExpiryTime,
            1 AS IsValidSession
        FROM NewsSitePro2025_Users u
        INNER JOIN NewsSitePro2025_UserSessions s ON u.UserID = s.UserID
        WHERE s.SessionID = @SessionID;
    END
    ELSE
    BEGIN
        -- Invalid session
        SELECT 0 AS IsValidSession;
        
        -- Mark session as inactive if it exists
        IF @SessionID IS NOT NULL
        BEGIN
            UPDATE NewsSitePro2025_UserSessions 
            SET IsActive = 0,
                LogoutTime = GETDATE(),
                LogoutReason = 'Expired'
            WHERE SessionID = @SessionID;
        END
    END
END
GO

-- Procedure to logout (invalidate session)
CREATE PROCEDURE NewsSitePro2025_sp_UserSessions_Logout
    @SessionToken NVARCHAR(500),
    @LogoutReason NVARCHAR(100) = 'Manual'
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE NewsSitePro2025_UserSessions 
    SET IsActive = 0,
        LogoutTime = GETDATE(),
        LogoutReason = @LogoutReason
    WHERE SessionToken = @SessionToken AND IsActive = 1;
    
    SELECT @@ROWCOUNT AS SessionsLoggedOut;
END
GO

-- Procedure to cleanup expired sessions
CREATE PROCEDURE NewsSitePro2025_sp_UserSessions_Cleanup
    @DaysToKeep INT = 30
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @CutoffDate DATETIME2 = DATEADD(DAY, -@DaysToKeep, GETDATE());
    
    -- Mark expired sessions as inactive
    UPDATE NewsSitePro2025_UserSessions 
    SET IsActive = 0,
        LogoutTime = GETDATE(),
        LogoutReason = 'Expired'
    WHERE IsActive = 1 AND ExpiryTime < GETDATE();
    
    -- Delete old inactive sessions
    DELETE FROM NewsSitePro2025_UserSessions 
    WHERE IsActive = 0 AND ISNULL(LogoutTime, LoginTime) < @CutoffDate;
    
    SELECT @@ROWCOUNT AS SessionsDeleted;
END
GO

-- Procedure to store OAuth tokens
CREATE PROCEDURE NewsSitePro2025_sp_OAuthTokens_Store
    @UserID INT,
    @Provider NVARCHAR(50),
    @AccessToken NVARCHAR(2000),
    @RefreshToken NVARCHAR(500) = NULL,
    @TokenType NVARCHAR(50) = 'Bearer',
    @ExpiresInSeconds INT,
    @Scope NVARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @ExpiresAt DATETIME2 = DATEADD(SECOND, @ExpiresInSeconds, GETDATE());
    
    -- Deactivate existing tokens for this user and provider
    UPDATE NewsSitePro2025_OAuthTokens 
    SET IsActive = 0, UpdatedAt = GETDATE()
    WHERE UserID = @UserID AND Provider = @Provider AND IsActive = 1;
    
    -- Insert new token
    INSERT INTO NewsSitePro2025_OAuthTokens (
        UserID, Provider, AccessToken, RefreshToken, TokenType, 
        ExpiresAt, Scope, CreatedAt, UpdatedAt, IsActive
    )
    VALUES (
        @UserID, @Provider, @AccessToken, @RefreshToken, @TokenType,
        @ExpiresAt, @Scope, GETDATE(), GETDATE(), 1
    );
    
    SELECT SCOPE_IDENTITY() AS TokenID, @ExpiresAt AS ExpiresAt;
END
GO

-- Procedure to get active OAuth token
CREATE PROCEDURE NewsSitePro2025_sp_OAuthTokens_Get
    @UserID INT,
    @Provider NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        TokenID,
        UserID,
        Provider,
        AccessToken,
        RefreshToken,
        TokenType,
        ExpiresAt,
        Scope,
        CreatedAt,
        UpdatedAt,
        CASE WHEN ExpiresAt > GETDATE() THEN 0 ELSE 1 END AS IsExpired
    FROM NewsSitePro2025_OAuthTokens
    WHERE UserID = @UserID 
      AND Provider = @Provider 
      AND IsActive = 1
    ORDER BY CreatedAt DESC;
END
GO

-- Procedure to get user login history
CREATE PROCEDURE NewsSitePro2025_sp_UserSessions_GetHistory
    @UserID INT,
    @PageNumber INT = 1,
    @PageSize INT = 20
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;
    
    SELECT 
        SessionID,
        UserID,
        DeviceInfo,
        IpAddress,
        LEFT(UserAgent, 200) AS UserAgent,
        LoginTime,
        LastActivityTime,
        ExpiryTime,
        LogoutTime,
        LogoutReason,
        IsActive,
        DATEDIFF(MINUTE, LoginTime, ISNULL(LogoutTime, LastActivityTime)) AS SessionDurationMinutes
    FROM NewsSitePro2025_UserSessions
    WHERE UserID = @UserID
    ORDER BY LoginTime DESC
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;
    
    -- Also return total count
    SELECT COUNT(*) AS TotalSessions
    FROM NewsSitePro2025_UserSessions
    WHERE UserID = @UserID;
END
GO

-- Create a view for active user sessions
CREATE VIEW vw_ActiveUserSessions AS
SELECT 
    s.SessionID,
    s.UserID,
    u.Username,
    u.Email,
    u.IsGoogleUser,
    s.DeviceInfo,
    s.IpAddress,
    s.LoginTime,
    s.LastActivityTime,
    s.ExpiryTime,
    DATEDIFF(MINUTE, s.LastActivityTime, GETDATE()) AS MinutesSinceLastActivity
FROM NewsSitePro2025_UserSessions s
INNER JOIN NewsSitePro2025_Users u ON s.UserID = u.UserID
WHERE s.IsActive = 1 AND s.ExpiryTime > GETDATE();
GO

-- Create a trigger to automatically update user's last activity when sessions are updated
CREATE TRIGGER tr_UserSessions_UpdateUserActivity
ON NewsSitePro2025_UserSessions
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    
    IF UPDATE(LastActivityTime)
    BEGIN
        UPDATE u
        SET LastActivity = i.LastActivityTime
        FROM NewsSitePro2025_Users u
        INNER JOIN inserted i ON u.UserID = i.UserID
        WHERE i.IsActive = 1;
    END
END
GO

-- Final cleanup and setup
PRINT 'Running final cleanup...';
EXEC NewsSitePro2025_sp_UserSessions_Cleanup;

PRINT 'Google OAuth and Session Management setup completed successfully!';
PRINT 'Tables created/verified: NewsSitePro2025_UserSessions, NewsSitePro2025_OAuthTokens';
PRINT 'Columns added to NewsSitePro2025_Users: GoogleId, GoogleEmail, IsGoogleUser, LastLoginTime, GoogleRefreshToken';
PRINT 'Stored procedures created for OAuth operations and session management';
