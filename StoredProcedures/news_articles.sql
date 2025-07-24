-- =============================================
-- News Articles Stored Procedures
-- =============================================

-- sp_NewsArticles_GetAll - Get all news articles with pagination
CREATE PROCEDURE sp_NewsArticles_GetAll
    @PageNumber INT = 1,
    @PageSize INT = 10,
    @Category NVARCHAR(50) = NULL
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
        u.Email,
        COUNT(l.LikeID) as LikesCount,
        COUNT(v.ViewID) as ViewsCount
    FROM NewsArticles na
    INNER JOIN Users_News u ON na.UserID = u.UserID
    LEFT JOIN ArticleLikes l ON na.ArticleID = l.ArticleID
    LEFT JOIN ArticleViews v ON na.ArticleID = v.ArticleID
    WHERE (@Category IS NULL OR na.Category = @Category)
    GROUP BY na.ArticleID, na.Title, na.Content, na.ImageURL, na.SourceURL, na.SourceName, na.Category, na.PublishDate, na.UserID, u.Username, u.Email
    ORDER BY na.PublishDate DESC
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;
    
    -- Get total count
    SELECT COUNT(*) as TotalCount
    FROM NewsArticles na
    WHERE (@Category IS NULL OR na.Category = @Category);
END
GO

-- sp_NewsArticles_Insert - Create a new news article
CREATE PROCEDURE sp_NewsArticles_Insert
    @Title NVARCHAR(100),
    @Content NVARCHAR(500),
    @ImageURL NVARCHAR(255) = NULL,
    @SourceURL NVARCHAR(500) = NULL,
    @SourceName NVARCHAR(100) = NULL,
    @Category NVARCHAR(50),
    @UserID INT
AS
BEGIN
    SET NOCOUNT ON;
    
    INSERT INTO NewsArticles (Title, Content, ImageURL, SourceURL, SourceName, Category, UserID, PublishDate)
    VALUES (@Title, @Content, @ImageURL, @SourceURL, @SourceName, @Category, @UserID, GETDATE());
    
    SELECT SCOPE_IDENTITY() as ArticleID;
END
GO

-- sp_NewsArticles_GetByUser - Get articles by specific user
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
        COUNT(l.LikeID) as LikesCount,
        COUNT(v.ViewID) as ViewsCount
    FROM NewsArticles na
    LEFT JOIN ArticleLikes l ON na.ArticleID = l.ArticleID
    LEFT JOIN ArticleViews v ON na.ArticleID = v.ArticleID
    WHERE na.UserID = @UserID
    GROUP BY na.ArticleID, na.Title, na.Content, na.ImageURL, na.SourceURL, na.SourceName, na.Category, na.PublishDate
    ORDER BY na.PublishDate DESC
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END
GO

-- =============================================
-- User Interactions Stored Procedures
-- =============================================

-- sp_ArticleLikes_Toggle - Toggle like on an article
CREATE PROCEDURE sp_ArticleLikes_Toggle
    @ArticleID INT,
    @UserID INT
AS
BEGIN
    SET NOCOUNT ON;
    
    IF EXISTS (SELECT 1 FROM ArticleLikes WHERE ArticleID = @ArticleID AND UserID = @UserID)
    BEGIN
        DELETE FROM ArticleLikes WHERE ArticleID = @ArticleID AND UserID = @UserID;
        SELECT 'unliked' as Action;
    END
    ELSE
    BEGIN
        INSERT INTO ArticleLikes (ArticleID, UserID, LikeDate)
        VALUES (@ArticleID, @UserID, GETDATE());
        SELECT 'liked' as Action;
    END
END
GO

-- sp_SavedArticles_Toggle - Toggle save on an article
CREATE PROCEDURE sp_SavedArticles_Toggle
    @ArticleID INT,
    @UserID INT
AS
BEGIN
    SET NOCOUNT ON;
    
    IF EXISTS (SELECT 1 FROM SavedArticles WHERE ArticleID = @ArticleID AND UserID = @UserID)
    BEGIN
        DELETE FROM SavedArticles WHERE ArticleID = @ArticleID AND UserID = @UserID;
        SELECT 'unsaved' as Action;
    END
    ELSE
    BEGIN
        INSERT INTO SavedArticles (ArticleID, UserID, SaveDate)
        VALUES (@ArticleID, @UserID, GETDATE());
        SELECT 'saved' as Action;
    END
END
GO

-- sp_ArticleViews_Insert - Record article view
CREATE PROCEDURE sp_ArticleViews_Insert
    @ArticleID INT,
    @UserID INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Only insert if user hasn't viewed this article today
    IF NOT EXISTS (
        SELECT 1 FROM ArticleViews 
        WHERE ArticleID = @ArticleID 
        AND UserID = @UserID 
        AND CAST(ViewDate as DATE) = CAST(GETDATE() as DATE)
    )
    BEGIN
        INSERT INTO ArticleViews (ArticleID, UserID, ViewDate)
        VALUES (@ArticleID, @UserID, GETDATE());
    END
END
GO

-- sp_Reports_Insert - Report an article
CREATE PROCEDURE sp_Reports_Insert
    @ArticleID INT,
    @UserID INT,
    @Reason NVARCHAR(255) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Only allow one report per user per article
    IF NOT EXISTS (SELECT 1 FROM Reports WHERE ArticleID = @ArticleID AND UserID = @UserID)
    BEGIN
        INSERT INTO Reports (ArticleID, UserID, Reason, ReportDate)
        VALUES (@ArticleID, @UserID, @Reason, GETDATE());
    END
END
GO

-- =============================================
-- User Preferences Stored Procedures
-- =============================================

-- sp_UserPreferences_Get - Get user preferences
CREATE PROCEDURE sp_UserPreferences_Get
    @UserID INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        up.PreferenceID,
        up.Categories,
        up.EmailNotifications,
        up.PushNotifications,
        up.WeeklyDigest,
        up.LastUpdated
    FROM UserPreferences up
    WHERE up.UserID = @UserID;
END
GO

-- sp_UserPreferences_Upsert - Insert or update user preferences
CREATE PROCEDURE sp_UserPreferences_Upsert
    @UserID INT,
    @Categories NVARCHAR(255),
    @EmailNotifications BIT,
    @PushNotifications BIT,
    @WeeklyDigest BIT
AS
BEGIN
    SET NOCOUNT ON;
    
    IF EXISTS (SELECT 1 FROM UserPreferences WHERE UserID = @UserID)
    BEGIN
        UPDATE UserPreferences 
        SET 
            Categories = @Categories,
            EmailNotifications = @EmailNotifications,
            PushNotifications = @PushNotifications,
            WeeklyDigest = @WeeklyDigest,
            LastUpdated = GETDATE()
        WHERE UserID = @UserID;
    END
    ELSE
    BEGIN
        INSERT INTO UserPreferences (UserID, Categories, EmailNotifications, PushNotifications, WeeklyDigest, LastUpdated)
        VALUES (@UserID, @Categories, @EmailNotifications, @PushNotifications, @WeeklyDigest, GETDATE());
    END
END
GO

-- =============================================
-- User Statistics Stored Procedures
-- =============================================

-- sp_UserStats_Get - Get user statistics
CREATE PROCEDURE sp_UserStats_Get
    @UserID INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        (SELECT COUNT(*) FROM NewsArticles WHERE UserID = @UserID) as PostsCount,
        (SELECT COUNT(*) FROM ArticleLikes WHERE UserID = @UserID) as LikesCount,
        (SELECT COUNT(*) FROM SavedArticles WHERE UserID = @UserID) as SavedCount,
        (SELECT COUNT(*) FROM UserFollows WHERE FollowedUserID = @UserID) as FollowersCount;
END
GO

-- sp_UserProfile_Update - Update user profile information
CREATE PROCEDURE sp_UserProfile_Update
    @UserID INT,
    @Username NVARCHAR(100),
    @Bio NVARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE Users_News 
    SET 
        Username = @Username,
        Bio = @Bio,
        LastUpdated = GETDATE()
    WHERE UserID = @UserID;
END
GO
