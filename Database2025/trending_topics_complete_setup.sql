-- =============================================
-- NewsSitePro2025: Trending Topics Setup
-- This script creates the trending topics table and all related stored procedures
-- =============================================

-- First, create the TrendingTopics table if it doesn't exist
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='NewsSitePro2025_TrendingTopics' AND xtype='U')
BEGIN
    CREATE TABLE NewsSitePro2025_TrendingTopics (
        TrendID int IDENTITY(1,1) PRIMARY KEY,
        Topic nvarchar(200) NOT NULL,
        Category nvarchar(100) NOT NULL,
        TrendScore float NOT NULL DEFAULT 0.0,
        TotalInteractions int NOT NULL DEFAULT 0,
        LastUpdated datetime NOT NULL DEFAULT GETDATE(),
        RelatedKeywords nvarchar(500) NULL, -- JSON array
        GeographicRegions nvarchar(500) NULL -- JSON array of regions where trending
    );
    
    -- Create indexes for better performance
    CREATE NONCLUSTERED INDEX IX_NewsSitePro2025_TrendingTopics_Score 
        ON NewsSitePro2025_TrendingTopics (TrendScore DESC);
    
    CREATE NONCLUSTERED INDEX IX_NewsSitePro2025_TrendingTopics_Updated 
        ON NewsSitePro2025_TrendingTopics (LastUpdated DESC);
    
    CREATE NONCLUSTERED INDEX IX_NewsSitePro2025_TrendingTopics_Category 
        ON NewsSitePro2025_TrendingTopics (Category);
        
    PRINT 'NewsSitePro2025_TrendingTopics table created successfully';
END
ELSE
BEGIN
    PRINT 'NewsSitePro2025_TrendingTopics table already exists';
END
GO

-- =============================================
-- Calculate and Update Trending Topics
-- Description: Calculates trending topics based on likes, comments, and views
-- with time decay and weighted scoring (Updated to use existing tables)
-- =============================================
CREATE OR ALTER PROCEDURE NewsSitePro2025_sp_TrendingTopics_Calculate
    @TimeWindowHours INT = 24,
    @MaxTopics INT = 20
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- Clear old trending topics
        DELETE FROM NewsSitePro2025_TrendingTopics 
        WHERE LastUpdated < DATEADD(hour, -@TimeWindowHours, GETDATE());
        
        -- Calculate trending scores based on engagement metrics
        WITH EngagementMetrics AS (
            SELECT 
                na.Category,
                na.Title,
                -- Count engagement within time window
                COUNT(DISTINCT al.LikeID) as LikesCount,
                COUNT(DISTINCT c.CommentID) as CommentsCount,
                COUNT(DISTINCT av.ViewID) as ViewsCount,
                
                -- Calculate time decay factor (newer = higher weight)
                AVG(
                    CASE 
                        WHEN al.LikeDate IS NOT NULL THEN 
                            POWER(0.9, DATEDIFF(hour, al.LikeDate, GETDATE()))
                        ELSE 0
                    END +
                    CASE 
                        WHEN c.CreatedAt IS NOT NULL THEN 
                            POWER(0.9, DATEDIFF(hour, c.CreatedAt, GETDATE()))
                        ELSE 0
                    END +
                    CASE 
                        WHEN av.ViewDate IS NOT NULL THEN 
                            POWER(0.9, DATEDIFF(hour, av.ViewDate, GETDATE()))
                        ELSE 0
                    END
                ) as TimeDecayFactor,
                
                -- Calculate weighted engagement score
                (
                    COUNT(DISTINCT al.LikeID) * 3.0 +         -- Likes weight: 3
                    COUNT(DISTINCT c.CommentID) * 5.0 +       -- Comments weight: 5
                    COUNT(DISTINCT av.ViewID) * 1.0           -- Views weight: 1
                ) as BaseEngagementScore
                
            FROM NewsSitePro2025_NewsArticles na
            LEFT JOIN NewsSitePro2025_ArticleLikes al ON na.ArticleID = al.ArticleID 
                AND al.LikeDate >= DATEADD(hour, -@TimeWindowHours, GETDATE())
            LEFT JOIN NewsSitePro2025_Comments c ON na.ArticleID = c.ArticleID 
                AND c.CreatedAt >= DATEADD(hour, -@TimeWindowHours, GETDATE())
            LEFT JOIN NewsSitePro2025_ArticleViews av ON na.ArticleID = av.ArticleID 
                AND av.ViewDate >= DATEADD(hour, -@TimeWindowHours, GETDATE())
            WHERE na.PublishDate >= DATEADD(day, -7, GETDATE()) -- Only recent articles
            GROUP BY na.Category, na.Title
            HAVING COUNT(DISTINCT al.LikeID) + COUNT(DISTINCT c.CommentID) + COUNT(DISTINCT av.ViewID) > 0
        ),
        TrendingCalculation AS (
            SELECT 
                Category,
                Title as Topic,
                LikesCount + CommentsCount + ViewsCount as TotalInteractions,
                BaseEngagementScore * ISNULL(TimeDecayFactor, 1.0) as TrendScore,
                ROW_NUMBER() OVER (ORDER BY BaseEngagementScore * ISNULL(TimeDecayFactor, 1.0) DESC) as Rank
            FROM EngagementMetrics
        )
        
        -- Insert/Update trending topics
        MERGE NewsSitePro2025_TrendingTopics AS Target
        USING (
            SELECT TOP (@MaxTopics)
                Topic,
                Category,
                TrendScore,
                TotalInteractions
            FROM TrendingCalculation
            WHERE Rank <= @MaxTopics
        ) AS Source ON Target.Topic = Source.Topic AND Target.Category = Source.Category
        
        WHEN MATCHED THEN
            UPDATE SET 
                TrendScore = Source.TrendScore,
                TotalInteractions = Source.TotalInteractions,
                LastUpdated = GETDATE()
        
        WHEN NOT MATCHED THEN
            INSERT (Topic, Category, TrendScore, TotalInteractions, LastUpdated)
            VALUES (Source.Topic, Source.Category, Source.TrendScore, Source.TotalInteractions, GETDATE());
        
        COMMIT TRANSACTION;
        
        -- Return success message
        SELECT 'SUCCESS' as Status, 'Trending topics calculated successfully' as Message;
        
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
            
        SELECT 'ERROR' as Status, ERROR_MESSAGE() as Message;
    END CATCH
END
GO

-- =============================================
-- Get Trending Topics
-- Description: Retrieves current trending topics with optional filtering
-- =============================================
CREATE OR ALTER PROCEDURE NewsSitePro2025_sp_TrendingTopics_Get
    @Count INT = 10,
    @Category NVARCHAR(100) = NULL,
    @MinScore FLOAT = 0.0
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT TOP (@Count)
        TrendID,
        Topic,
        Category,
        TrendScore,
        TotalInteractions,
        LastUpdated,
        RelatedKeywords,
        GeographicRegions
    FROM NewsSitePro2025_TrendingTopics
    WHERE 
        (@Category IS NULL OR Category = @Category)
        AND TrendScore >= @MinScore
        AND LastUpdated >= DATEADD(hour, -2, GETDATE()) -- Only recent calculations
    ORDER BY TrendScore DESC, TotalInteractions DESC;
END
GO

-- =============================================
-- Get Category-Based Trending Topics
-- Description: Gets trending topics grouped by categories
-- =============================================
CREATE OR ALTER PROCEDURE NewsSitePro2025_sp_TrendingTopics_GetByCategory
    @TopicsPerCategory INT = 3
AS
BEGIN
    SET NOCOUNT ON;
    
    WITH RankedTopics AS (
        SELECT 
            TrendID,
            Topic,
            Category,
            TrendScore,
            TotalInteractions,
            LastUpdated,
            ROW_NUMBER() OVER (PARTITION BY Category ORDER BY TrendScore DESC) as CategoryRank
        FROM NewsSitePro2025_TrendingTopics
        WHERE LastUpdated >= DATEADD(hour, -2, GETDATE())
    )
    SELECT 
        TrendID,
        Topic,
        Category,
        TrendScore,
        TotalInteractions,
        LastUpdated
    FROM RankedTopics
    WHERE CategoryRank <= @TopicsPerCategory
    ORDER BY Category, TrendScore DESC;
END
GO

-- =============================================
-- Get Trending Articles by Topic
-- Description: Gets articles related to a specific trending topic (Updated to use existing tables)
-- =============================================
CREATE OR ALTER PROCEDURE NewsSitePro2025_sp_TrendingTopics_GetRelatedArticles
    @Topic NVARCHAR(200),
    @Category NVARCHAR(100),
    @Count INT = 10,
    @CurrentUserID INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT TOP (@Count)
        na.ArticleID,
        na.Title,
        na.Content,
        na.ImageURL,
        na.SourceURL,
        na.SourceName,
        na.PublishDate,
        na.Category,
        u.UserID,
        u.Username,
        u.ProfilePicture as UserProfilePicture,
        
        -- Engagement metrics
        ISNULL(likes.LikesCount, 0) as LikesCount,
        ISNULL(comments.CommentsCount, 0) as CommentsCount,
        ISNULL(views.ViewsCount, 0) as ViewsCount,
        
        -- User context
        CASE WHEN userLikes.LikeID IS NOT NULL THEN 1 ELSE 0 END as IsLiked,
        CASE WHEN userSaved.SaveID IS NOT NULL THEN 1 ELSE 0 END as IsSaved
        
    FROM NewsSitePro2025_NewsArticles na
    INNER JOIN NewsSitePro2025_Users u ON na.UserID = u.UserID
    
    -- Engagement counts
    LEFT JOIN (
        SELECT ArticleID, COUNT(*) as LikesCount
        FROM NewsSitePro2025_ArticleLikes
        GROUP BY ArticleID
    ) likes ON na.ArticleID = likes.ArticleID
    
    LEFT JOIN (
        SELECT ArticleID, COUNT(*) as CommentsCount
        FROM NewsSitePro2025_Comments
        GROUP BY ArticleID
    ) comments ON na.ArticleID = comments.ArticleID
    
    LEFT JOIN (
        SELECT ArticleID, COUNT(*) as ViewsCount
        FROM NewsSitePro2025_ArticleViews
        GROUP BY ArticleID
    ) views ON na.ArticleID = views.ArticleID
    
    -- User interactions (if user is logged in)
    LEFT JOIN NewsSitePro2025_ArticleLikes userLikes ON na.ArticleID = userLikes.ArticleID 
        AND userLikes.UserID = @CurrentUserID
    LEFT JOIN NewsSitePro2025_SavedArticles userSaved ON na.ArticleID = userSaved.ArticleID 
        AND userSaved.UserID = @CurrentUserID
    
    WHERE 
        (na.Title LIKE '%' + @Topic + '%' OR na.Content LIKE '%' + @Topic + '%')
        AND na.Category = @Category
        AND na.PublishDate >= DATEADD(day, -7, GETDATE())
    
    ORDER BY 
        (ISNULL(likes.LikesCount, 0) * 3 + ISNULL(comments.CommentsCount, 0) * 5 + ISNULL(views.ViewsCount, 0)) DESC,
        na.PublishDate DESC;
END
GO

-- =============================================
-- Update Trending Topic Keywords
-- Description: Updates related keywords for a trending topic
-- =============================================
CREATE OR ALTER PROCEDURE NewsSitePro2025_sp_TrendingTopics_UpdateKeywords
    @TrendID INT,
    @Keywords NVARCHAR(500)
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE NewsSitePro2025_TrendingTopics
    SET 
        RelatedKeywords = @Keywords,
        LastUpdated = GETDATE()
    WHERE TrendID = @TrendID;
    
    SELECT @@ROWCOUNT as RowsAffected;
END
GO

-- =============================================
-- Clean Old Trending Topics
-- Description: Removes outdated trending topics
-- =============================================
CREATE OR ALTER PROCEDURE NewsSitePro2025_sp_TrendingTopics_Cleanup
    @MaxAgeHours INT = 24
AS
BEGIN
    SET NOCOUNT ON;
    
    DELETE FROM NewsSitePro2025_TrendingTopics
    WHERE LastUpdated < DATEADD(hour, -@MaxAgeHours, GETDATE());
    
    SELECT @@ROWCOUNT as DeletedCount, 'Old trending topics cleaned up' as Message;
END
GO

-- =============================================
-- Insert some sample data for testing
-- =============================================
IF NOT EXISTS (SELECT 1 FROM NewsSitePro2025_TrendingTopics)
BEGIN
    INSERT INTO NewsSitePro2025_TrendingTopics (Topic, Category, TrendScore, TotalInteractions, LastUpdated)
    VALUES 
        ('AI Technology Breakthrough', 'Technology', 85.5, 150, GETDATE()),
        ('Climate Change Summit', 'Environment', 78.2, 120, GETDATE()),
        ('World Cup 2025 Updates', 'Sports', 92.8, 200, GETDATE()),
        ('Election Coverage', 'Politics', 67.4, 98, GETDATE()),
        ('Health & Wellness Tips', 'Health', 54.7, 75, GETDATE());
    
    PRINT 'Sample trending topics data inserted';
END
ELSE
BEGIN
    PRINT 'Trending topics table already contains data';
END
GO

PRINT 'Trending Topics setup completed successfully!';
PRINT 'You can now test the API endpoints to see trending topics in action.';
