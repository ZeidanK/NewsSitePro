-- =============================================
-- SQL Server ALTER Scripts to Fix Stored Procedures
-- Run these on your SQL Server to update existing procedures
-- Fixed for myProjDB database
-- =============================================

USE myProjDB;
GO

-- 1. ALTER: Fix GetAll procedure to include repost count and proper user filtering
ALTER PROCEDURE NewsSitePro2025_sp_NewsArticles_GetAll
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
        -- Like information with proper aggregation
        COALESCE(lc.LikesCount, 0) as LikesCount,
        CASE WHEN al.UserID IS NOT NULL THEN 1 ELSE 0 END as IsLiked,
        -- Save information
        CASE WHEN sa.UserID IS NOT NULL THEN 1 ELSE 0 END as IsSaved,
        -- View count from actual views table
        COALESCE(vc.ViewsCount, 0) as ViewsCount,
        -- Repost count from reposts table
        COALESCE(rc.RepostCount, 0) as RepostCount,
        -- Check if current user reposted this article
        CASE WHEN rp.UserID IS NOT NULL THEN 1 ELSE 0 END as IsReposted
    FROM NewsSitePro2025_NewsArticles na
    INNER JOIN NewsSitePro2025_Users u ON na.UserID = u.UserID
    -- Aggregate likes count
    LEFT JOIN (
        SELECT ArticleID, COUNT(*) as LikesCount
        FROM NewsSitePro2025_ArticleLikes
        GROUP BY ArticleID
    ) lc ON na.ArticleID = lc.ArticleID
    -- Aggregate views count
    LEFT JOIN (
        SELECT ArticleID, COUNT(*) as ViewsCount
        FROM NewsSitePro2025_ArticleViews
        GROUP BY ArticleID
    ) vc ON na.ArticleID = vc.ArticleID
    -- Aggregate repost count
    LEFT JOIN (
        SELECT OriginalArticleID, COUNT(*) as RepostCount
        FROM NewsSitePro2025_Reposts
        GROUP BY OriginalArticleID
    ) rc ON na.ArticleID = rc.OriginalArticleID
    -- Check if current user liked this article
    LEFT JOIN NewsSitePro2025_ArticleLikes al ON na.ArticleID = al.ArticleID AND al.UserID = @CurrentUserID
    -- Check if current user saved this article
    LEFT JOIN NewsSitePro2025_SavedArticles sa ON na.ArticleID = sa.ArticleID AND sa.UserID = @CurrentUserID
    -- Check if current user reposted this article
    LEFT JOIN NewsSitePro2025_Reposts rp ON na.ArticleID = rp.OriginalArticleID AND rp.UserID = @CurrentUserID
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

-- 2. ALTER: Fix SavedArticles procedure to include proper interaction data
ALTER PROCEDURE NewsSitePro2025_sp_SavedArticles_GetByUser
    @UserID INT,
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
        sa.SaveDate,
        -- Aggregate likes count
        COALESCE(lc.LikesCount, 0) as LikesCount,
        -- Check if current user liked this article
        CASE WHEN al.UserID IS NOT NULL THEN 1 ELSE 0 END as IsLiked,
        -- This will always be 1 since these are saved articles
        1 as IsSaved,
        -- Actual view count from views table
        COALESCE(vc.ViewsCount, 0) as ViewsCount,
        -- Repost count from reposts table
        COALESCE(rc.RepostCount, 0) as RepostCount,
        -- Check if current user reposted this article
        CASE WHEN rp.UserID IS NOT NULL THEN 1 ELSE 0 END as IsReposted
    FROM NewsSitePro2025_SavedArticles sa
    INNER JOIN NewsSitePro2025_NewsArticles na ON sa.ArticleID = na.ArticleID
    INNER JOIN NewsSitePro2025_Users u ON na.UserID = u.UserID
    -- Aggregate likes count
    LEFT JOIN (
        SELECT ArticleID, COUNT(*) as LikesCount
        FROM NewsSitePro2025_ArticleLikes
        GROUP BY ArticleID
    ) lc ON na.ArticleID = lc.ArticleID
    -- Aggregate views count
    LEFT JOIN (
        SELECT ArticleID, COUNT(*) as ViewsCount
        FROM NewsSitePro2025_ArticleViews
        GROUP BY ArticleID
    ) vc ON na.ArticleID = vc.ArticleID
    -- Aggregate repost count
    LEFT JOIN (
        SELECT OriginalArticleID, COUNT(*) as RepostCount
        FROM NewsSitePro2025_Reposts
        GROUP BY OriginalArticleID
    ) rc ON na.ArticleID = rc.OriginalArticleID
    -- Check if current user liked this article
    LEFT JOIN NewsSitePro2025_ArticleLikes al ON na.ArticleID = al.ArticleID AND al.UserID = COALESCE(@CurrentUserID, @UserID)
    -- Check if current user reposted this article
    LEFT JOIN NewsSitePro2025_Reposts rp ON na.ArticleID = rp.OriginalArticleID AND rp.UserID = COALESCE(@CurrentUserID, @UserID)
    WHERE sa.UserID = @UserID
    ORDER BY sa.SaveDate DESC
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END
GO

-- 3. CREATE: New procedure for individual article with user context (if not exists)
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[NewsSitePro2025_sp_NewsArticles_GetByIdWithUserContext]') AND type in (N'P', N'PC'))
BEGIN
    EXEC('CREATE PROCEDURE NewsSitePro2025_sp_NewsArticles_GetByIdWithUserContext AS BEGIN SET NOCOUNT ON; END')
END
GO

ALTER PROCEDURE NewsSitePro2025_sp_NewsArticles_GetByIdWithUserContext
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
        -- Aggregate likes count
        COALESCE(lc.LikesCount, 0) as LikesCount,
        -- Check if current user liked this article
        CASE WHEN al.UserID IS NOT NULL THEN 1 ELSE 0 END as IsLiked,
        -- Check if current user saved this article
        CASE WHEN sa.UserID IS NOT NULL THEN 1 ELSE 0 END as IsSaved,
        -- Actual view count from views table
        COALESCE(vc.ViewsCount, 0) as ViewsCount,
        -- Repost count from reposts table
        COALESCE(rc.RepostCount, 0) as RepostCount,
        -- Check if current user reposted this article
        CASE WHEN rp.UserID IS NOT NULL THEN 1 ELSE 0 END as IsReposted
    FROM NewsSitePro2025_NewsArticles na
    INNER JOIN NewsSitePro2025_Users u ON na.UserID = u.UserID
    -- Aggregate likes count
    LEFT JOIN (
        SELECT ArticleID, COUNT(*) as LikesCount
        FROM NewsSitePro2025_ArticleLikes
        GROUP BY ArticleID
    ) lc ON na.ArticleID = lc.ArticleID
    -- Aggregate views count
    LEFT JOIN (
        SELECT ArticleID, COUNT(*) as ViewsCount
        FROM NewsSitePro2025_ArticleViews
        GROUP BY ArticleID
    ) vc ON na.ArticleID = vc.ArticleID
    -- Aggregate repost count
    LEFT JOIN (
        SELECT OriginalArticleID, COUNT(*) as RepostCount
        FROM NewsSitePro2025_Reposts
        GROUP BY OriginalArticleID
    ) rc ON na.ArticleID = rc.OriginalArticleID
    -- Check if current user liked this article
    LEFT JOIN NewsSitePro2025_ArticleLikes al ON na.ArticleID = al.ArticleID AND al.UserID = @CurrentUserID
    -- Check if current user saved this article
    LEFT JOIN NewsSitePro2025_SavedArticles sa ON na.ArticleID = sa.ArticleID AND sa.UserID = @CurrentUserID
    -- Check if current user reposted this article
    LEFT JOIN NewsSitePro2025_Reposts rp ON na.ArticleID = rp.OriginalArticleID AND rp.UserID = @CurrentUserID
    WHERE na.ArticleID = @ArticleID;
END
GO

-- 4. CREATE: Repost toggle procedure (if not exists)
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[NewsSitePro2025_sp_Reposts_Toggle]') AND type in (N'P', N'PC'))
BEGIN
    EXEC('CREATE PROCEDURE NewsSitePro2025_sp_Reposts_Toggle AS BEGIN SET NOCOUNT ON; END')
END
GO

ALTER PROCEDURE NewsSitePro2025_sp_Reposts_Toggle
    @OriginalArticleID INT,
    @UserID INT
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (SELECT 1 FROM NewsSitePro2025_Reposts WHERE OriginalArticleID = @OriginalArticleID AND UserID = @UserID)
    BEGIN
        DELETE FROM NewsSitePro2025_Reposts WHERE OriginalArticleID = @OriginalArticleID AND UserID = @UserID;
        SELECT 'unreposted' as Action;
    END
    ELSE
    BEGIN
        INSERT INTO NewsSitePro2025_Reposts (OriginalArticleID, UserID, CreatedAt)
        VALUES (@OriginalArticleID, @UserID, GETDATE());
        SELECT 'reposted' as Action;
    END
END
GO

PRINT 'All stored procedures have been updated successfully for myProjDB!';
