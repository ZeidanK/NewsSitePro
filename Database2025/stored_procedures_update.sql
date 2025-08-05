-- =============================================
-- SQL Script to Update Existing Stored Procedures
-- Run this on your SQL Server to update the stored procedures
-- =============================================

USE [NewsSitePro2025]
GO

-- Update the main GetAll stored procedure to include reposts and fix view counts
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
        -- Aggregate likes count
        COALESCE(lc.LikesCount, 0) as LikesCount,
        -- Check if current user liked this article
        CASE WHEN al.UserID IS NOT NULL THEN 1 ELSE 0 END as IsLiked,
        -- Check if current user saved this article
        CASE WHEN sa.UserID IS NOT NULL THEN 1 ELSE 0 END as IsSaved,
        -- Actual view count from views table
        COALESCE(vc.ViewsCount, 0) as ViewsCount,
        -- Repost count
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
    -- Aggregate reposts count
    LEFT JOIN (
        SELECT ArticleID, COUNT(*) as RepostCount
        FROM NewsSitePro2025_Reposts
        WHERE IsDeleted = 0
        GROUP BY ArticleID
    ) rc ON na.ArticleID = rc.ArticleID
    -- Check if current user liked this article
    LEFT JOIN NewsSitePro2025_ArticleLikes al ON na.ArticleID = al.ArticleID AND al.UserID = @CurrentUserID
    -- Check if current user saved this article
    LEFT JOIN NewsSitePro2025_SavedArticles sa ON na.ArticleID = sa.ArticleID AND sa.UserID = @CurrentUserID
    -- Check if current user reposted this article
    LEFT JOIN NewsSitePro2025_Reposts rp ON na.ArticleID = rp.ArticleID AND rp.UserID = @CurrentUserID AND rp.IsDeleted = 0
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

-- Update the Search stored procedure to include reposts and fix view counts
ALTER PROCEDURE NewsSitePro2025_sp_NewsArticles_Search
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
        -- Aggregate likes count
        COALESCE(lc.LikesCount, 0) as LikesCount,
        -- Check if current user liked this article
        CASE WHEN al.UserID IS NOT NULL THEN 1 ELSE 0 END as IsLiked,
        -- Check if current user saved this article
        CASE WHEN sa.UserID IS NOT NULL THEN 1 ELSE 0 END as IsSaved,
        -- Actual view count from views table
        COALESCE(vc.ViewsCount, 0) as ViewsCount,
        -- Repost count
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
    -- Aggregate reposts count
    LEFT JOIN (
        SELECT ArticleID, COUNT(*) as RepostCount
        FROM NewsSitePro2025_Reposts
        WHERE IsDeleted = 0
        GROUP BY ArticleID
    ) rc ON na.ArticleID = rc.ArticleID
    -- Check if current user liked this article
    LEFT JOIN NewsSitePro2025_ArticleLikes al ON na.ArticleID = al.ArticleID AND al.UserID = @CurrentUserID
    -- Check if current user saved this article
    LEFT JOIN NewsSitePro2025_SavedArticles sa ON na.ArticleID = sa.ArticleID AND sa.UserID = @CurrentUserID
    -- Check if current user reposted this article
    LEFT JOIN NewsSitePro2025_Reposts rp ON na.ArticleID = rp.ArticleID AND rp.UserID = @CurrentUserID AND rp.IsDeleted = 0
    WHERE (na.Title LIKE '%' + @SearchTerm + '%' OR na.Content LIKE '%' + @SearchTerm + '%')
      AND (@Category IS NULL OR na.Category = @Category)
    ORDER BY na.PublishDate DESC
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END
GO

-- Update the GetByIdWithUserContext stored procedure to include reposts
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
        -- Repost count
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
    -- Aggregate reposts count
    LEFT JOIN (
        SELECT ArticleID, COUNT(*) as RepostCount
        FROM NewsSitePro2025_Reposts
        WHERE IsDeleted = 0
        GROUP BY ArticleID
    ) rc ON na.ArticleID = rc.ArticleID
    -- Check if current user liked this article
    LEFT JOIN NewsSitePro2025_ArticleLikes al ON na.ArticleID = al.ArticleID AND al.UserID = @CurrentUserID
    -- Check if current user saved this article
    LEFT JOIN NewsSitePro2025_SavedArticles sa ON na.ArticleID = sa.ArticleID AND sa.UserID = @CurrentUserID
    -- Check if current user reposted this article
    LEFT JOIN NewsSitePro2025_Reposts rp ON na.ArticleID = rp.ArticleID AND rp.UserID = @CurrentUserID AND rp.IsDeleted = 0
    WHERE na.ArticleID = @ArticleID;
END
GO

-- Update the SavedArticles GetByUser stored procedure to include reposts and fix view counts
ALTER PROCEDURE NewsSitePro2025_sp_SavedArticles_GetByUser
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
        sa.SaveDate,
        -- Aggregate likes count
        COALESCE(lc.LikesCount, 0) as LikesCount,
        -- Check if current user liked this article (always check for the requesting user)
        CASE WHEN al.UserID IS NOT NULL THEN 1 ELSE 0 END as IsLiked,
        -- Always true for saved articles
        1 as IsSaved,
        -- Actual view count from views table
        COALESCE(vc.ViewsCount, 0) as ViewsCount,
        -- Repost count
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
    -- Aggregate reposts count
    LEFT JOIN (
        SELECT ArticleID, COUNT(*) as RepostCount
        FROM NewsSitePro2025_Reposts
        WHERE IsDeleted = 0
        GROUP BY ArticleID
    ) rc ON na.ArticleID = rc.ArticleID
    -- Check if current user liked this article
    LEFT JOIN NewsSitePro2025_ArticleLikes al ON na.ArticleID = al.ArticleID AND al.UserID = @UserID
    -- Check if current user reposted this article
    LEFT JOIN NewsSitePro2025_Reposts rp ON na.ArticleID = rp.ArticleID AND rp.UserID = @UserID AND rp.IsDeleted = 0
    WHERE sa.UserID = @UserID
    ORDER BY sa.SaveDate DESC
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END
GO

PRINT 'All stored procedures updated successfully with repost counts and fixed view counts!'
