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

-- Add more procedures for comments, tags, follows, notifications, reports, etc. (see next file for more) 