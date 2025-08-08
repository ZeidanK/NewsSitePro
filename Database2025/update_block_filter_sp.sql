-- =============================================
-- Update Block Filter Stored Procedure
-- Run this to update the database with the fixed stored procedure
-- =============================================

USE [igroup104_test2]
GO

-- Drop and recreate the stored procedure with the fix
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'NewsSitePro2025_sp_NewsArticles_GetWithBlockFilter')
    DROP PROCEDURE NewsSitePro2025_sp_NewsArticles_GetWithBlockFilter
GO

CREATE PROCEDURE [dbo].[NewsSitePro2025_sp_NewsArticles_GetWithBlockFilter]
    @PageNumber INT = 1,
    @PageSize INT = 10,
    @Category NVARCHAR(50) = NULL,
    @CurrentUserID INT = NULL,
    @SortBy NVARCHAR(20) = 'recent'
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;
    
    -- EXACT COPY OF WORKING GetAll WITH ONLY BLOCK FILTER ADDED
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
        u.ProfilePicture as UserProfilePicture,
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
    WHERE (@Category IS NULL OR LOWER(na.Category) = LOWER(@Category))
        AND (@SortBy != 'trending' OR na.PublishDate >= DATEADD(day, -7, GETDATE()))
        -- ONLY ADDITION: Block filter
        AND (@CurrentUserID IS NULL OR na.UserID NOT IN (
            SELECT BlockedUserID FROM NewsSitePro2025_UserBlocks 
            WHERE BlockerUserID = @CurrentUserID AND IsActive = 1
        ))
    ORDER BY 
        CASE 
            WHEN @SortBy = 'trending' THEN (COALESCE(lc.LikesCount, 0) * 3 + COALESCE(vc.ViewsCount, 0) + COALESCE(rc.RepostCount, 0) * 5)
            ELSE 0
        END DESC,
        na.PublishDate DESC
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;
    
    -- Also return total count for pagination
    SELECT COUNT(*) as TotalCount
    FROM NewsSitePro2025_NewsArticles na
    WHERE (@Category IS NULL OR LOWER(na.Category) = LOWER(@Category))
        AND (@SortBy != 'trending' OR na.PublishDate >= DATEADD(day, -7, GETDATE()))
        -- ONLY ADDITION: Block filter
        AND (@CurrentUserID IS NULL OR na.UserID NOT IN (
            SELECT BlockedUserID FROM NewsSitePro2025_UserBlocks 
            WHERE BlockerUserID = @CurrentUserID AND IsActive = 1
        ));
END
GO

PRINT 'Block filter stored procedure updated successfully!';
