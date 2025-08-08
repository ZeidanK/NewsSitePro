-- Fix Google OAuth stored procedure to handle PasswordHash constraint
-- This script updates the stored procedure to provide a placeholder password hash for Google OAuth users

USE [NewsSitePro2025];
GO

-- Drop and recreate the stored procedure with PasswordHash handling
IF OBJECT_ID('NewsSitePro2025_sp_Google_CreateOrGetUser', 'P') IS NOT NULL
    DROP PROCEDURE NewsSitePro2025_sp_Google_CreateOrGetUser;
GO

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
                LastLoginTime = GETDATE(),
                LastUpdated = GETDATE()
            WHERE UserID = @UserID;
        END
        ELSE
        BEGIN
            -- Create new user with placeholder password hash for Google OAuth users
            SET @IsNewUser = 1;
            
            -- Generate a placeholder password hash (SHA512 of a random string)
            -- This is not a real password but satisfies the NOT NULL constraint
            DECLARE @PlaceholderPassword NVARCHAR(100) = 'GOOGLE_OAUTH_USER_' + @GoogleId;
            DECLARE @PlaceholderPasswordHash NVARCHAR(200);
            
            -- Create a SHA512-like placeholder hash (88 chars base64 encoded)
            SET @PlaceholderPasswordHash = 'GOOGLE_OAUTH_PLACEHOLDER_' + 
                CONVERT(NVARCHAR(64), HASHBYTES('SHA2_512', @PlaceholderPassword), 1);
            
            INSERT INTO NewsSitePro2025_Users (
                Username, Email, PasswordHash, GoogleId, GoogleEmail, IsGoogleUser, 
                IsActive, IsAdmin, IsLocked, IsBanned, JoinDate, LastLoginTime, LastUpdated
            )
            VALUES (
                ISNULL(@Username, LEFT(@GoogleEmail, CHARINDEX('@', @GoogleEmail) - 1)),
                @GoogleEmail, 
                @PlaceholderPasswordHash,
                @GoogleId, 
                @GoogleEmail, 
                1,
                1, 0, 0, 0, 
                GETDATE(), 
                GETDATE(),
                GETDATE()
            );
            
            SET @UserID = SCOPE_IDENTITY();
        END
    END
    ELSE
    BEGIN
        -- Update last login time for existing Google user
        UPDATE NewsSitePro2025_Users 
        SET LastLoginTime = GETDATE(),
            LastUpdated = GETDATE()
        WHERE UserID = @UserID;
    END
    
    -- Return user information
    SELECT 
        UserID,
        Username,
        Email,
        GoogleId,
        GoogleEmail,
        IsGoogleUser,
        IsActive,
        IsAdmin,
        IsLocked,
        IsBanned,
        BannedUntil,
        ProfilePicture,
        Bio,
        JoinDate,
        LastLoginTime,
        @IsNewUser AS IsNewUser
    FROM NewsSitePro2025_Users
    WHERE UserID = @UserID;
END
GO

PRINT 'Google OAuth stored procedure updated successfully with PasswordHash handling.';
