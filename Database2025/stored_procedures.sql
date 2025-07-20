-- =============================================
-- NewsSitePro2025: Stored Procedures
-- =============================================

-- User CRUD
-- Get User
CREATE PROCEDURE NewsSitePro2025_sp_Users_Get
    @UserID INT = NULL,
    @Username NVARCHAR(100) = NULL,
    @Email NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT TOP 1 * FROM NewsSitePro2025_Users
    WHERE (@UserID IS NULL OR UserID = @UserID)
      AND (@Username IS NULL OR Username = @Username)
      AND (@Email IS NULL OR Email = @Email)
END
GO

-- Insert User
CREATE PROCEDURE NewsSitePro2025_sp_Users_Insert
    @Username NVARCHAR(100),
    @Email NVARCHAR(100),
    @PasswordHash NVARCHAR(200),
    @IsAdmin BIT,
    @IsLocked BIT,
    @Bio NVARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO NewsSitePro2025_Users (Username, Email, PasswordHash, IsAdmin, IsLocked, Bio, JoinDate, LastUpdated)
    VALUES (@Username, @Email, @PasswordHash, @IsAdmin, @IsLocked, @Bio, GETDATE(), GETDATE())
    SELECT SCOPE_IDENTITY() as UserID;
END
GO

-- Update User
CREATE PROCEDURE NewsSitePro2025_sp_Users_Update
    @UserID INT,
    @Username NVARCHAR(100) = NULL,
    @PasswordHash NVARCHAR(200) = NULL,
    @IsAdmin BIT = NULL,
    @IsLocked BIT = NULL,
    @Bio NVARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE NewsSitePro2025_Users
    SET Username = ISNULL(@Username, Username),
        PasswordHash = ISNULL(@PasswordHash, PasswordHash),
        IsAdmin = ISNULL(@IsAdmin, IsAdmin),
        IsLocked = ISNULL(@IsLocked, IsLocked),
        Bio = ISNULL(@Bio, Bio),
        LastUpdated = GETDATE()
    WHERE UserID = @UserID
END
GO

-- Update User Profile Picture
CREATE PROCEDURE NewsSitePro2025_sp_Users_UpdateProfilePic
    @UserID INT,
    @ProfilePicture NVARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE NewsSitePro2025_Users
    SET ProfilePicture = @ProfilePicture, LastUpdated = GETDATE()
    WHERE UserID = @UserID;
END
GO

-- News Article CRUD
-- Get Article
CREATE PROCEDURE NewsSitePro2025_sp_NewsArticles_Get
    @ArticleID INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM NewsSitePro2025_NewsArticles
    WHERE (@ArticleID IS NULL OR ArticleID = @ArticleID)
END
GO

-- Get All Articles with Pagination and User Context
CREATE PROCEDURE NewsSitePro2025_sp_NewsArticles_GetAll
    @PageNumber INT = 1,
    @PageSize INT = 10,
    @Category NVARCHAR(50) = NULL,
    @CurrentUserID INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;
    
    SELECT 
        na.ArticleID,
        na.Title,
        na.Content,
        na.ImageURL,
        na.SourceURL,
        na.SourceName,
        na.Category,
        na.PublishDate,
        na.UserID,
        u.Username,
        u.ProfilePicture,
        -- Like information
        (SELECT COUNT(*) FROM NewsSitePro2025_ArticleLikes al WHERE al.ArticleID = na.ArticleID) as LikesCount,
        CASE WHEN @CurrentUserID IS NOT NULL AND EXISTS(SELECT 1 FROM NewsSitePro2025_ArticleLikes al WHERE al.ArticleID = na.ArticleID AND al.UserID = @CurrentUserID) 
             THEN 1 ELSE 0 END as IsLiked,
        -- Save information
        CASE WHEN @CurrentUserID IS NOT NULL AND EXISTS(SELECT 1 FROM NewsSitePro2025_SavedArticles sa WHERE sa.ArticleID = na.ArticleID AND sa.UserID = @CurrentUserID) 
             THEN 1 ELSE 0 END as IsSaved,
        -- View count (mock for now)
        ABS(CHECKSUM(NEWID())) % 1000 as ViewsCount
    FROM NewsSitePro2025_NewsArticles na
    INNER JOIN NewsSitePro2025_Users u ON na.UserID = u.UserID
    WHERE (@Category IS NULL OR na.Category = @Category)
    ORDER BY na.PublishDate DESC
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;
    
    -- Also return total count for pagination
    SELECT COUNT(*) as TotalCount
    FROM NewsSitePro2025_NewsArticles na
    WHERE (@Category IS NULL OR na.Category = @Category);
END
GO

-- Insert Article
CREATE PROCEDURE NewsSitePro2025_sp_NewsArticles_Insert
    @Title NVARCHAR(100),
    @Content NVARCHAR(4000),
    @ImageURL NVARCHAR(255) = NULL,
    @SourceURL NVARCHAR(500) = NULL,
    @SourceName NVARCHAR(100) = NULL,
    @Category NVARCHAR(50),
    @UserID INT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO NewsSitePro2025_NewsArticles (Title, Content, ImageURL, SourceURL, SourceName, Category, UserID, PublishDate)
    VALUES (@Title, @Content, @ImageURL, @SourceURL, @SourceName, @Category, @UserID, GETDATE());
    SELECT SCOPE_IDENTITY() as ArticleID;
END
GO

-- Update Article
CREATE PROCEDURE NewsSitePro2025_sp_NewsArticles_Update
    @ArticleID INT,
    @Title NVARCHAR(100) = NULL,
    @Content NVARCHAR(4000) = NULL,
    @ImageURL NVARCHAR(255) = NULL,
    @SourceURL NVARCHAR(500) = NULL,
    @SourceName NVARCHAR(100) = NULL,
    @Category NVARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE NewsSitePro2025_NewsArticles
    SET Title = ISNULL(@Title, Title),
        Content = ISNULL(@Content, Content),
        ImageURL = ISNULL(@ImageURL, ImageURL),
        SourceURL = ISNULL(@SourceURL, SourceURL),
        SourceName = ISNULL(@SourceName, SourceName),
        Category = ISNULL(@Category, Category)
    WHERE ArticleID = @ArticleID
END
GO

-- Delete Article
CREATE PROCEDURE NewsSitePro2025_sp_NewsArticles_Delete
    @ArticleID INT
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM NewsSitePro2025_NewsArticles WHERE ArticleID = @ArticleID
END
GO

-- Toggle Like
CREATE PROCEDURE NewsSitePro2025_sp_ArticleLikes_Toggle
    @ArticleID INT,
    @UserID INT
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (SELECT 1 FROM NewsSitePro2025_ArticleLikes WHERE ArticleID = @ArticleID AND UserID = @UserID)
    BEGIN
        DELETE FROM NewsSitePro2025_ArticleLikes WHERE ArticleID = @ArticleID AND UserID = @UserID;
        SELECT 'unliked' as Action;
    END
    ELSE
    BEGIN
        INSERT INTO NewsSitePro2025_ArticleLikes (ArticleID, UserID, LikeDate)
        VALUES (@ArticleID, @UserID, GETDATE());
        SELECT 'liked' as Action;
    END
END
GO

-- Toggle Save
CREATE PROCEDURE NewsSitePro2025_sp_SavedArticles_Toggle
    @ArticleID INT,
    @UserID INT
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (SELECT 1 FROM NewsSitePro2025_SavedArticles WHERE ArticleID = @ArticleID AND UserID = @UserID)
    BEGIN
        DELETE FROM NewsSitePro2025_SavedArticles WHERE ArticleID = @ArticleID AND UserID = @UserID;
        SELECT 'unsaved' as Action;
    END
    ELSE
    BEGIN
        INSERT INTO NewsSitePro2025_SavedArticles (ArticleID, UserID, SaveDate)
        VALUES (@ArticleID, @UserID, GETDATE());
        SELECT 'saved' as Action;
    END
END
GO

-- Add more procedures for comments, tags, follows, notifications, reports, etc.

-- User Authentication Procedures
CREATE PROCEDURE NewsSitePro2025_sp_Users_GetByEmail
    @Email NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM NewsSitePro2025_Users WHERE Email = @Email;
END
GO

CREATE PROCEDURE NewsSitePro2025_sp_Users_EmailExists
    @Email NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT CASE WHEN EXISTS(SELECT 1 FROM NewsSitePro2025_Users WHERE Email = @Email) THEN 1 ELSE 0 END as EmailExists;
END
GO

-- User Posts
CREATE PROCEDURE NewsSitePro2025_sp_NewsArticles_GetByUser
    @UserID INT,
    @PageNumber INT = 1,
    @PageSize INT = 10
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;
    
    SELECT 
        na.ArticleID,
        na.Title,
        na.Content,
        na.ImageURL,
        na.SourceURL,
        na.SourceName,
        na.Category,
        na.PublishDate,
        na.UserID,
        u.Username,
        (SELECT COUNT(*) FROM NewsSitePro2025_ArticleLikes al WHERE al.ArticleID = na.ArticleID) as LikesCount,
        ABS(CHECKSUM(NEWID())) % 1000 as ViewsCount
    FROM NewsSitePro2025_NewsArticles na
    INNER JOIN NewsSitePro2025_Users u ON na.UserID = u.UserID
    WHERE na.UserID = @UserID
    ORDER BY na.PublishDate DESC
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END
GO

-- Saved Articles
CREATE PROCEDURE NewsSitePro2025_sp_SavedArticles_GetByUser
    @UserID INT,
    @PageNumber INT = 1,
    @PageSize INT = 10
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;
    
    SELECT 
        na.ArticleID,
        na.Title,
        na.Content,
        na.ImageURL,
        na.SourceURL,
        na.SourceName,
        na.Category,
        na.PublishDate,
        na.UserID,
        u.Username,
        sa.SaveDate,
        (SELECT COUNT(*) FROM NewsSitePro2025_ArticleLikes al WHERE al.ArticleID = na.ArticleID) as LikesCount,
        ABS(CHECKSUM(NEWID())) % 1000 as ViewsCount
    FROM NewsSitePro2025_SavedArticles sa
    INNER JOIN NewsSitePro2025_NewsArticles na ON sa.ArticleID = na.ArticleID
    INNER JOIN NewsSitePro2025_Users u ON na.UserID = u.UserID
    WHERE sa.UserID = @UserID
    ORDER BY sa.SaveDate DESC
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END
GO

-- User Stats
CREATE PROCEDURE NewsSitePro2025_sp_Users_GetStats
    @UserID INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        (SELECT COUNT(*) FROM NewsSitePro2025_NewsArticles WHERE UserID = @UserID) as PostsCount,
        (SELECT COUNT(*) FROM NewsSitePro2025_ArticleLikes al 
         INNER JOIN NewsSitePro2025_NewsArticles na ON al.ArticleID = na.ArticleID 
         WHERE na.UserID = @UserID) as LikesReceived,
        (SELECT COUNT(*) FROM NewsSitePro2025_ArticleLikes WHERE UserID = @UserID) as LikesGiven,
        (SELECT COUNT(*) FROM NewsSitePro2025_SavedArticles WHERE UserID = @UserID) as SavedCount,
        0 as FollowersCount, -- Placeholder for future follow system
        0 as FollowingCount  -- Placeholder for future follow system
END
GO

-- Update User Preferences
CREATE PROCEDURE NewsSitePro2025_sp_Users_UpdatePreferences
    @UserID INT,
    @EmailNotifications BIT = NULL,
    @PushNotifications BIT = NULL,
    @WeeklyDigest BIT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    -- This would require adding preference columns to Users table
    -- For now, just return success
    SELECT 'Preferences updated successfully' as Message;
END
GO

-- Update User Profile
CREATE PROCEDURE NewsSitePro2025_sp_Users_UpdateProfile
    @UserID INT,
    @Username NVARCHAR(100) = NULL,
    @Bio NVARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE NewsSitePro2025_Users
    SET Username = ISNULL(@Username, Username),
        Bio = ISNULL(@Bio, Bio),
        LastUpdated = GETDATE()
    WHERE UserID = @UserID;
    
    SELECT 'Profile updated successfully' as Message;
END
GO

-- Change Password
CREATE PROCEDURE NewsSitePro2025_sp_Users_ChangePassword
    @UserID INT,
    @NewPasswordHash NVARCHAR(200)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE NewsSitePro2025_Users
    SET PasswordHash = @NewPasswordHash,
        LastUpdated = GETDATE()
    WHERE UserID = @UserID;
    
    SELECT 'Password updated successfully' as Message;
END
GO

-- Article Views (for tracking)
CREATE PROCEDURE NewsSitePro2025_sp_ArticleViews_Add
    @ArticleID INT,
    @UserID INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    -- This would require an ArticleViews table
    -- For now, just return success
    SELECT 'View recorded' as Message;
END
GO

-- Get Article with User Context
CREATE PROCEDURE NewsSitePro2025_sp_NewsArticles_GetWithContext
    @ArticleID INT,
    @CurrentUserID INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        na.ArticleID,
        na.Title,
        na.Content,
        na.ImageURL,
        na.SourceURL,
        na.SourceName,
        na.Category,
        na.PublishDate,
        na.UserID,
        u.Username,
        u.ProfilePicture,
        -- Like information
        (SELECT COUNT(*) FROM NewsSitePro2025_ArticleLikes al WHERE al.ArticleID = na.ArticleID) as LikesCount,
        CASE WHEN @CurrentUserID IS NOT NULL AND EXISTS(SELECT 1 FROM NewsSitePro2025_ArticleLikes al WHERE al.ArticleID = na.ArticleID AND al.UserID = @CurrentUserID) 
             THEN 1 ELSE 0 END as IsLiked,
        -- Save information
        CASE WHEN @CurrentUserID IS NOT NULL AND EXISTS(SELECT 1 FROM NewsSitePro2025_SavedArticles sa WHERE sa.ArticleID = na.ArticleID AND sa.UserID = @CurrentUserID) 
             THEN 1 ELSE 0 END as IsSaved,
        -- View count (mock for now)
        ABS(CHECKSUM(NEWID())) % 1000 as ViewsCount
    FROM NewsSitePro2025_NewsArticles na
    INNER JOIN NewsSitePro2025_Users u ON na.UserID = u.UserID
    WHERE na.ArticleID = @ArticleID;
END
GO

-- Search Articles
CREATE PROCEDURE NewsSitePro2025_sp_NewsArticles_Search
    @SearchTerm NVARCHAR(100),
    @Category NVARCHAR(50) = NULL,
    @PageNumber INT = 1,
    @PageSize INT = 10,
    @CurrentUserID INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;
    
    SELECT 
        na.ArticleID,
        na.Title,
        na.Content,
        na.ImageURL,
        na.SourceURL,
        na.SourceName,
        na.Category,
        na.PublishDate,
        na.UserID,
        u.Username,
        u.ProfilePicture,
        (SELECT COUNT(*) FROM NewsSitePro2025_ArticleLikes al WHERE al.ArticleID = na.ArticleID) as LikesCount,
        CASE WHEN @CurrentUserID IS NOT NULL AND EXISTS(SELECT 1 FROM NewsSitePro2025_ArticleLikes al WHERE al.ArticleID = na.ArticleID AND al.UserID = @CurrentUserID) 
             THEN 1 ELSE 0 END as IsLiked,
        CASE WHEN @CurrentUserID IS NOT NULL AND EXISTS(SELECT 1 FROM NewsSitePro2025_SavedArticles sa WHERE sa.ArticleID = na.ArticleID AND sa.UserID = @CurrentUserID) 
             THEN 1 ELSE 0 END as IsSaved,
        ABS(CHECKSUM(NEWID())) % 1000 as ViewsCount
    FROM NewsSitePro2025_NewsArticles na
    INNER JOIN NewsSitePro2025_Users u ON na.UserID = u.UserID
    WHERE (na.Title LIKE '%' + @SearchTerm + '%' OR na.Content LIKE '%' + @SearchTerm + '%')
      AND (@Category IS NULL OR na.Category = @Category)
    ORDER BY na.PublishDate DESC
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END
GO

-- =============================================
-- Additional Stored Procedures with Short Names
-- (to match what DBservices.cs expects)
-- =============================================

-- Get All Articles (short name for compatibility)
CREATE PROCEDURE sp_NewsArticles_GetAll
    @PageNumber INT = 1,
    @PageSize INT = 10,
    @Category NVARCHAR(50) = NULL,
    @CurrentUserID INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;
    
    SELECT 
        na.ArticleID,
        na.Title,
        na.Content,
        na.ImageURL,
        na.SourceURL,
        na.SourceName,
        na.Category,
        na.PublishDate,
        na.UserID,
        u.Username,
        u.ProfilePicture,
        -- Like information
        (SELECT COUNT(*) FROM NewsSitePro2025_ArticleLikes al WHERE al.ArticleID = na.ArticleID) as LikesCount,
        CASE WHEN @CurrentUserID IS NOT NULL AND EXISTS(SELECT 1 FROM NewsSitePro2025_ArticleLikes al WHERE al.ArticleID = na.ArticleID AND al.UserID = @CurrentUserID) 
             THEN 1 ELSE 0 END as IsLiked,
        -- Save information
        CASE WHEN @CurrentUserID IS NOT NULL AND EXISTS(SELECT 1 FROM NewsSitePro2025_SavedArticles sa WHERE sa.ArticleID = na.ArticleID AND sa.UserID = @CurrentUserID) 
             THEN 1 ELSE 0 END as IsSaved,
        -- View count (mock for now)
        ABS(CHECKSUM(NEWID())) % 1000 as ViewsCount
    FROM NewsSitePro2025_NewsArticles na
    INNER JOIN NewsSitePro2025_Users u ON na.UserID = u.UserID
    WHERE (@Category IS NULL OR na.Category = @Category)
    ORDER BY na.PublishDate DESC
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END
GO

-- Insert Article (short name)
CREATE PROCEDURE sp_NewsArticles_Insert
    @Title NVARCHAR(100),
    @Content NVARCHAR(4000),
    @ImageURL NVARCHAR(255) = NULL,
    @SourceURL NVARCHAR(500) = NULL,
    @SourceName NVARCHAR(100) = NULL,
    @Category NVARCHAR(50),
    @UserID INT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO NewsSitePro2025_NewsArticles (Title, Content, ImageURL, SourceURL, SourceName, Category, UserID, PublishDate)
    VALUES (@Title, @Content, @ImageURL, @SourceURL, @SourceName, @Category, @UserID, GETDATE());
    SELECT SCOPE_IDENTITY() as ArticleID;
END
GO

-- Get Articles by User (short name)
CREATE PROCEDURE sp_NewsArticles_GetByUser
    @UserID INT,
    @PageNumber INT = 1,
    @PageSize INT = 10
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;
    
    SELECT 
        na.ArticleID,
        na.Title,
        na.Content,
        na.ImageURL,
        na.SourceURL,
        na.SourceName,
        na.Category,
        na.PublishDate,
        na.UserID,
        u.Username,
        u.ProfilePicture,
        (SELECT COUNT(*) FROM NewsSitePro2025_ArticleLikes al WHERE al.ArticleID = na.ArticleID) as LikesCount,
        ABS(CHECKSUM(NEWID())) % 1000 as ViewsCount
    FROM NewsSitePro2025_NewsArticles na
    INNER JOIN NewsSitePro2025_Users u ON na.UserID = u.UserID
    WHERE na.UserID = @UserID
    ORDER BY na.PublishDate DESC
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END
GO

-- Toggle Article Likes (short name)
CREATE PROCEDURE sp_ArticleLikes_Toggle
    @ArticleID INT,
    @UserID INT
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (SELECT 1 FROM NewsSitePro2025_ArticleLikes WHERE ArticleID = @ArticleID AND UserID = @UserID)
    BEGIN
        DELETE FROM NewsSitePro2025_ArticleLikes WHERE ArticleID = @ArticleID AND UserID = @UserID;
        SELECT 'unliked' as Action;
    END
    ELSE
    BEGIN
        INSERT INTO NewsSitePro2025_ArticleLikes (ArticleID, UserID, LikeDate)
        VALUES (@ArticleID, @UserID, GETDATE());
        SELECT 'liked' as Action;
    END
END
GO

-- Toggle Saved Articles (short name)
CREATE PROCEDURE sp_SavedArticles_Toggle
    @ArticleID INT,
    @UserID INT
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (SELECT 1 FROM NewsSitePro2025_SavedArticles WHERE ArticleID = @ArticleID AND UserID = @UserID)
    BEGIN
        DELETE FROM NewsSitePro2025_SavedArticles WHERE ArticleID = @ArticleID AND UserID = @UserID;
        SELECT 'unsaved' as Action;
    END
    ELSE
    BEGIN
        INSERT INTO NewsSitePro2025_SavedArticles (ArticleID, UserID, SaveDate)
        VALUES (@ArticleID, @UserID, GETDATE());
        SELECT 'saved' as Action;
    END
END
GO

-- Article Views Insert (short name)
CREATE PROCEDURE sp_ArticleViews_Insert
    @ArticleID INT,
    @UserID INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    -- This would require an ArticleViews table
    -- For now, just return success
    SELECT 'View recorded' as Message;
END
GO

-- Reports Insert (short name)
CREATE PROCEDURE sp_Reports_Insert
    @ArticleID INT,
    @UserID INT,
    @Reason NVARCHAR(500),
    @Description NVARCHAR(1000) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    -- This would require a Reports table
    -- For now, just return success
    SELECT 'Report submitted' as Message;
END
GO

-- User Stats Get (short name)
CREATE PROCEDURE sp_UserStats_Get
    @UserID INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        (SELECT COUNT(*) FROM NewsSitePro2025_NewsArticles WHERE UserID = @UserID) as PostsCount,
        (SELECT COUNT(*) FROM NewsSitePro2025_ArticleLikes al 
         INNER JOIN NewsSitePro2025_NewsArticles na ON al.ArticleID = na.ArticleID 
         WHERE na.UserID = @UserID) as LikesReceived,
        (SELECT COUNT(*) FROM NewsSitePro2025_ArticleLikes WHERE UserID = @UserID) as LikesGiven,
        (SELECT COUNT(*) FROM NewsSitePro2025_SavedArticles WHERE UserID = @UserID) as SavedCount,
        0 as FollowersCount, -- Placeholder for future follow system
        0 as FollowingCount  -- Placeholder for future follow system
END
GO

-- User Profile Update (short name)
CREATE PROCEDURE sp_UserProfile_Update
    @UserID INT,
    @Username NVARCHAR(100) = NULL,
    @Bio NVARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE NewsSitePro2025_Users
    SET Username = ISNULL(@Username, Username),
        Bio = ISNULL(@Bio, Bio),
        LastUpdated = GETDATE()
    WHERE UserID = @UserID;
    
    SELECT 'Profile updated successfully' as Message;
END
GO 