-- =============================================
-- NewsSitePro2025: User Activity and Saved Articles Stored Procedures
-- =============================================

-- Get User Recent Activity (Liked and Commented Articles)
CREATE PROCEDURE NewsSitePro2025_sp_UserActivity_GetRecent
    @UserID INT,
    @PageNumber INT = 1,
    @PageSize INT = 20
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT ActivityType, ArticleID, ActivityDate, Title, Category, ImageURL, SourceName, Username
    FROM (
        -- Liked Articles
        SELECT 
            'liked' as ActivityType,
            na.ArticleID,
            al.LikeDate as ActivityDate,
            na.Title,
            na.Category,
            na.ImageURL,
            na.SourceName,
            u.Username as Username
        FROM NewsSitePro2025_ArticleLikes al
        INNER JOIN NewsSitePro2025_NewsArticles na ON al.ArticleID = na.ArticleID
        INNER JOIN NewsSitePro2025_Users u ON na.UserID = u.UserID
        WHERE al.UserID = @UserID
        
        UNION ALL
        
        -- Commented Articles
        SELECT DISTINCT
            'commented' as ActivityType,
            na.ArticleID,
            MAX(c.CreatedAt) as ActivityDate,
            na.Title,
            na.Category,
            na.ImageURL,
            na.SourceName,
            u.Username as Username
        FROM NewsSitePro2025_Comments c
        INNER JOIN NewsSitePro2025_NewsArticles na ON c.PostID = na.ArticleID
        INNER JOIN NewsSitePro2025_Users u ON na.UserID = u.UserID
        WHERE c.UserID = @UserID AND (c.IsDeleted = 0 OR c.IsDeleted IS NULL)
        GROUP BY na.ArticleID, na.Title, na.Category, na.ImageURL, na.SourceName, u.Username
    ) as Activities
    ORDER BY ActivityDate DESC
    OFFSET (@PageNumber - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END
GO

-- Search Saved Articles
CREATE PROCEDURE NewsSitePro2025_sp_SavedArticles_Search
    @UserID INT,
    @SearchTerm NVARCHAR(200) = NULL,
    @Category NVARCHAR(50) = NULL,
    @PageNumber INT = 1,
    @PageSize INT = 10
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT na.ArticleID, na.Title, na.Content, na.ImageURL, na.SourceURL, 
           na.SourceName, na.Category, na.PublishDate, na.UserID, u.Username,
           COALESCE(lc.LikesCount, 0) as LikesCount,
           COALESCE(vc.ViewsCount, 0) as ViewsCount,
           CASE WHEN al.UserID IS NOT NULL THEN 1 ELSE 0 END as IsLiked,
           1 as IsSaved, -- Always 1 since these are saved articles
           sa.SaveDate
    FROM NewsSitePro2025_SavedArticles sa
    INNER JOIN NewsSitePro2025_NewsArticles na ON sa.ArticleID = na.ArticleID
    INNER JOIN NewsSitePro2025_Users u ON na.UserID = u.UserID
    LEFT JOIN (
        SELECT ArticleID, COUNT(*) as LikesCount
        FROM NewsSitePro2025_ArticleLikes
        GROUP BY ArticleID
    ) lc ON na.ArticleID = lc.ArticleID
    LEFT JOIN (
        SELECT ArticleID, COUNT(*) as ViewsCount
        FROM NewsSitePro2025_ArticleViews
        GROUP BY ArticleID
    ) vc ON na.ArticleID = vc.ArticleID
    LEFT JOIN NewsSitePro2025_ArticleLikes al ON na.ArticleID = al.ArticleID AND al.UserID = @UserID
    WHERE sa.UserID = @UserID
      AND (@SearchTerm IS NULL OR na.Title LIKE '%' + @SearchTerm + '%' OR na.Content LIKE '%' + @SearchTerm + '%')
      AND (@Category IS NULL OR na.Category = @Category)
    ORDER BY sa.SaveDate DESC
    OFFSET (@PageNumber - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END
GO

-- Get Saved Articles Count (for pagination)
CREATE PROCEDURE NewsSitePro2025_sp_SavedArticles_GetCount
    @UserID INT,
    @SearchTerm NVARCHAR(200) = NULL,
    @Category NVARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT COUNT(*) as TotalCount
    FROM NewsSitePro2025_SavedArticles sa
    INNER JOIN NewsSitePro2025_NewsArticles na ON sa.ArticleID = na.ArticleID
    WHERE sa.UserID = @UserID
      AND (@SearchTerm IS NULL OR na.Title LIKE '%' + @SearchTerm + '%' OR na.Content LIKE '%' + @SearchTerm + '%')
      AND (@Category IS NULL OR na.Category = @Category);
END
GO

-- Get User Saved Articles with display options
CREATE PROCEDURE NewsSitePro2025_sp_SavedArticles_GetWithOptions
    @UserID INT,
    @DisplayType NVARCHAR(20) = 'grid', -- 'grid', 'list', 'compact'
    @SortBy NVARCHAR(50) = 'SavedAt', -- 'SavedAt', 'PublishDate', 'Title', 'Category'
    @SortOrder NVARCHAR(10) = 'DESC', -- 'ASC', 'DESC'
    @PageNumber INT = 1,
    @PageSize INT = 12
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @sql NVARCHAR(MAX);
    SET @sql = N'
    SELECT na.ArticleID, na.Title, na.Content, na.ImageURL, na.SourceURL, 
           na.SourceName, na.Category, na.PublishDate, na.UserID, u.Username,
           COALESCE(lc.LikesCount, 0) as LikesCount,
           COALESCE(vc.ViewsCount, 0) as ViewsCount,
           CASE WHEN al.UserID IS NOT NULL THEN 1 ELSE 0 END as IsLiked,
           1 as IsSaved,
           sa.SaveDate
    FROM NewsSitePro2025_SavedArticles sa
    INNER JOIN NewsSitePro2025_NewsArticles na ON sa.ArticleID = na.ArticleID
    INNER JOIN NewsSitePro2025_Users u ON na.UserID = u.UserID
    LEFT JOIN (
        SELECT ArticleID, COUNT(*) as LikesCount
        FROM NewsSitePro2025_ArticleLikes
        GROUP BY ArticleID
    ) lc ON na.ArticleID = lc.ArticleID
    LEFT JOIN (
        SELECT ArticleID, COUNT(*) as ViewsCount
        FROM NewsSitePro2025_ArticleViews
        GROUP BY ArticleID
    ) vc ON na.ArticleID = vc.ArticleID
    LEFT JOIN NewsSitePro2025_ArticleLikes al ON na.ArticleID = al.ArticleID AND al.UserID = @UserID
    WHERE sa.UserID = @UserID
    ORDER BY ';
    
    -- Dynamic sorting
    IF @SortBy = 'SavedAt'
        SET @sql = @sql + 'sa.SaveDate';
    ELSE IF @SortBy = 'PublishDate'
        SET @sql = @sql + 'na.PublishDate';
    ELSE IF @SortBy = 'Title'
        SET @sql = @sql + 'na.Title';
    ELSE IF @SortBy = 'Category'
        SET @sql = @sql + 'na.Category';
    ELSE
        SET @sql = @sql + 'sa.SaveDate';
    
    SET @sql = @sql + ' ' + @SortOrder + '
    OFFSET (@PageNumber - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY';
    
    EXEC sp_executesql @sql, N'@UserID INT, @PageNumber INT, @PageSize INT', 
                       @UserID, @PageNumber, @PageSize;
END
GO
