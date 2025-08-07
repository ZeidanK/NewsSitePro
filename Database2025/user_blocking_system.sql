-- =============================================
-- NewsSitePro2025: User Blocking System
-- Creates tables and stored procedures for user blocking functionality
-- Author: System
-- Date: 2025-08-04
-- =============================================

USE [NewsSitePro2025]
GO

-- =============================================
-- Create User Blocking Table
-- =============================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='NewsSitePro2025_UserBlocks' AND xtype='U')
BEGIN
    CREATE TABLE [dbo].[NewsSitePro2025_UserBlocks] (
        BlockID INT IDENTITY(1,1) PRIMARY KEY,
        BlockerUserID INT NOT NULL,
        BlockedUserID INT NOT NULL,
        BlockDate DATETIME2 NOT NULL DEFAULT GETDATE(),
        Reason NVARCHAR(500) NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
        UpdatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
        
        -- Foreign Key Constraints
        FOREIGN KEY (BlockerUserID) REFERENCES NewsSitePro2025_Users(UserID) ON DELETE NO ACTION,
        FOREIGN KEY (BlockedUserID) REFERENCES NewsSitePro2025_Users(UserID) ON DELETE NO ACTION,
        
        -- Unique Constraint to prevent duplicate blocks
        CONSTRAINT UQ_UserBlock_2025 UNIQUE(BlockerUserID, BlockedUserID),
        
        -- Check Constraint to prevent self-blocking
        CONSTRAINT CK_UserBlock_NoSelfBlock_2025 CHECK (BlockerUserID != BlockedUserID)
    );
    
    PRINT 'NewsSitePro2025_UserBlocks table created successfully';
END
ELSE
BEGIN
    PRINT 'NewsSitePro2025_UserBlocks table already exists';
END
GO

-- Create indexes for better performance
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_NewsSitePro2025_UserBlocks_BlockerUserID')
BEGIN
    CREATE INDEX IX_NewsSitePro2025_UserBlocks_BlockerUserID ON NewsSitePro2025_UserBlocks(BlockerUserID);
    PRINT 'Index IX_NewsSitePro2025_UserBlocks_BlockerUserID created';
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_NewsSitePro2025_UserBlocks_BlockedUserID')
BEGIN
    CREATE INDEX IX_NewsSitePro2025_UserBlocks_BlockedUserID ON NewsSitePro2025_UserBlocks(BlockedUserID);
    PRINT 'Index IX_NewsSitePro2025_UserBlocks_BlockedUserID created';
END
GO

-- =============================================
-- STORED PROCEDURES
-- =============================================

-- =============================================
-- SP: Block a user
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'NewsSitePro2025_sp_UserBlocks_Block')
    DROP PROCEDURE NewsSitePro2025_sp_UserBlocks_Block
GO

CREATE PROCEDURE [dbo].[NewsSitePro2025_sp_UserBlocks_Block]
    @BlockerUserID INT,
    @BlockedUserID INT,
    @Reason NVARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        -- Check if users exist
        IF NOT EXISTS (SELECT 1 FROM NewsSitePro2025_Users WHERE UserID = @BlockerUserID)
        BEGIN
            RAISERROR('Blocker user does not exist', 16, 1);
            RETURN;
        END
        
        IF NOT EXISTS (SELECT 1 FROM NewsSitePro2025_Users WHERE UserID = @BlockedUserID)
        BEGIN
            RAISERROR('Blocked user does not exist', 16, 1);
            RETURN;
        END
        
        -- Check if already blocked
        IF EXISTS (SELECT 1 FROM NewsSitePro2025_UserBlocks WHERE BlockerUserID = @BlockerUserID AND BlockedUserID = @BlockedUserID AND IsActive = 1)
        BEGIN
            SELECT 'already_blocked' as Result, 'User is already blocked' as Message;
            RETURN;
        END
        
        -- Insert or update block record
        MERGE NewsSitePro2025_UserBlocks AS target
        USING (SELECT @BlockerUserID as BlockerUserID, @BlockedUserID as BlockedUserID) AS source
        ON (target.BlockerUserID = source.BlockerUserID AND target.BlockedUserID = source.BlockedUserID)
        WHEN MATCHED THEN
            UPDATE SET 
                IsActive = 1,
                BlockDate = GETDATE(),
                Reason = @Reason,
                UpdatedAt = GETDATE()
        WHEN NOT MATCHED THEN
            INSERT (BlockerUserID, BlockedUserID, BlockDate, Reason, IsActive, CreatedAt, UpdatedAt)
            VALUES (@BlockerUserID, @BlockedUserID, GETDATE(), @Reason, 1, GETDATE(), GETDATE());
        
        -- Remove any existing follow relationship
        DELETE FROM NewsSitePro2025_UserFollows 
        WHERE (FollowerUserID = @BlockerUserID AND FollowedUserID = @BlockedUserID)
           OR (FollowerUserID = @BlockedUserID AND FollowedUserID = @BlockerUserID);
        
        SELECT 'success' as Result, 'User blocked successfully' as Message;
        
    END TRY
    BEGIN CATCH
        SELECT 'error' as Result, ERROR_MESSAGE() as Message;
    END CATCH
END
GO

-- =============================================
-- SP: Unblock a user
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'NewsSitePro2025_sp_UserBlocks_Unblock')
    DROP PROCEDURE NewsSitePro2025_sp_UserBlocks_Unblock
GO

CREATE PROCEDURE [dbo].[NewsSitePro2025_sp_UserBlocks_Unblock]
    @BlockerUserID INT,
    @BlockedUserID INT
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        UPDATE NewsSitePro2025_UserBlocks 
        SET IsActive = 0, UpdatedAt = GETDATE()
        WHERE BlockerUserID = @BlockerUserID 
          AND BlockedUserID = @BlockedUserID 
          AND IsActive = 1;
        
        IF @@ROWCOUNT > 0
        BEGIN
            SELECT 'success' as Result, 'User unblocked successfully' as Message;
        END
        ELSE
        BEGIN
            SELECT 'not_found' as Result, 'Block relationship not found' as Message;
        END
        
    END TRY
    BEGIN CATCH
        SELECT 'error' as Result, ERROR_MESSAGE() as Message;
    END CATCH
END
GO

-- =============================================
-- SP: Check if user is blocked
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'NewsSitePro2025_sp_UserBlocks_IsBlocked')
    DROP PROCEDURE NewsSitePro2025_sp_UserBlocks_IsBlocked
GO

CREATE PROCEDURE [dbo].[NewsSitePro2025_sp_UserBlocks_IsBlocked]
    @BlockerUserID INT,
    @BlockedUserID INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT CASE 
        WHEN EXISTS (
            SELECT 1 FROM NewsSitePro2025_UserBlocks 
            WHERE BlockerUserID = @BlockerUserID 
              AND BlockedUserID = @BlockedUserID 
              AND IsActive = 1
        ) THEN 1 
        ELSE 0 
    END as IsBlocked;
END
GO

-- =============================================
-- SP: Get user's blocked users list
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'NewsSitePro2025_sp_UserBlocks_GetBlockedUsers')
    DROP PROCEDURE NewsSitePro2025_sp_UserBlocks_GetBlockedUsers
GO

CREATE PROCEDURE [dbo].[NewsSitePro2025_sp_UserBlocks_GetBlockedUsers]
    @BlockerUserID INT,
    @PageNumber INT = 1,
    @PageSize INT = 20
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;
    
    SELECT 
        ub.BlockID,
        ub.BlockedUserID,
        u.Username as BlockedUsername,
        u.Email as BlockedUserEmail,
        u.ProfilePicture as BlockedUserProfilePicture,
        ub.BlockDate,
        ub.Reason
    FROM NewsSitePro2025_UserBlocks ub
    INNER JOIN NewsSitePro2025_Users u ON ub.BlockedUserID = u.UserID
    WHERE ub.BlockerUserID = @BlockerUserID 
      AND ub.IsActive = 1
    ORDER BY ub.BlockDate DESC
    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
END
GO

-- =============================================
-- SP: Get users who blocked this user
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'NewsSitePro2025_sp_UserBlocks_GetBlockedByUsers')
    DROP PROCEDURE NewsSitePro2025_sp_UserBlocks_GetBlockedByUsers
GO

CREATE PROCEDURE [dbo].[NewsSitePro2025_sp_UserBlocks_GetBlockedByUsers]
    @BlockedUserID INT,
    @PageNumber INT = 1,
    @PageSize INT = 20
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;
    
    SELECT 
        ub.BlockID,
        ub.BlockerUserID,
        u.Username as BlockerUsername,
        u.Email as BlockerUserEmail,
        ub.BlockDate,
        ub.Reason
    FROM NewsSitePro2025_UserBlocks ub
    INNER JOIN NewsSitePro2025_Users u ON ub.BlockerUserID = u.UserID
    WHERE ub.BlockedUserID = @BlockedUserID 
      AND ub.IsActive = 1
    ORDER BY ub.BlockDate DESC
    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
END
GO

-- =============================================
-- SP: Get block statistics for a user
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'NewsSitePro2025_sp_UserBlocks_GetStats')
    DROP PROCEDURE NewsSitePro2025_sp_UserBlocks_GetStats
GO

CREATE PROCEDURE [dbo].[NewsSitePro2025_sp_UserBlocks_GetStats]
    @UserID INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        (SELECT COUNT(*) FROM NewsSitePro2025_UserBlocks WHERE BlockerUserID = @UserID AND IsActive = 1) as BlockedUsersCount,
        (SELECT COUNT(*) FROM NewsSitePro2025_UserBlocks WHERE BlockedUserID = @UserID AND IsActive = 1) as BlockedByUsersCount;
END
GO

-- =============================================
-- SP: Filter posts excluding blocked users
-- =============================================
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
    
    -- EXACT COPY OF WORKING GetAll PROCEDURE WITH ONLY BLOCK FILTER ADDED
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
        -- Like information with proper aggregation (SAME AS GetAll)
        COALESCE(lc.LikesCount, 0) as LikesCount,
        CASE WHEN al.UserID IS NOT NULL THEN 1 ELSE 0 END as IsLiked,
        -- Save information (SAME AS GetAll)
        CASE WHEN sa.UserID IS NOT NULL THEN 1 ELSE 0 END as IsSaved,
        -- View count from actual views table (SAME AS GetAll)
        COALESCE(vc.ViewsCount, 0) as ViewsCount,
        -- Repost count from reposts table (SAME AS GetAll)
        COALESCE(rc.RepostCount, 0) as RepostCount,
        -- Check if current user reposted this article (SAME AS GetAll)
        CASE WHEN rp.UserID IS NOT NULL THEN 1 ELSE 0 END as IsReposted
    FROM NewsSitePro2025_NewsArticles na
    INNER JOIN NewsSitePro2025_Users u ON na.UserID = u.UserID
    -- Aggregate likes count (SAME AS GetAll)
    LEFT JOIN (
        SELECT ArticleID, COUNT(*) as LikesCount
        FROM NewsSitePro2025_ArticleLikes
        GROUP BY ArticleID
    ) lc ON na.ArticleID = lc.ArticleID
    -- Aggregate views count (SAME AS GetAll)
    LEFT JOIN (
        SELECT ArticleID, COUNT(*) as ViewsCount
        FROM NewsSitePro2025_ArticleViews
        GROUP BY ArticleID
    ) vc ON na.ArticleID = vc.ArticleID
    -- Aggregate repost count (SAME AS GetAll)
    LEFT JOIN (
        SELECT OriginalArticleID, COUNT(*) as RepostCount
        FROM NewsSitePro2025_Reposts
        GROUP BY OriginalArticleID
    ) rc ON na.ArticleID = rc.OriginalArticleID
    -- Check if current user liked this article (SAME AS GetAll)
    LEFT JOIN NewsSitePro2025_ArticleLikes al ON na.ArticleID = al.ArticleID AND al.UserID = @CurrentUserID
    -- Check if current user saved this article (SAME AS GetAll)
    LEFT JOIN NewsSitePro2025_SavedArticles sa ON na.ArticleID = sa.ArticleID AND sa.UserID = @CurrentUserID
    -- Check if current user reposted this article (SAME AS GetAll)
    LEFT JOIN NewsSitePro2025_Reposts rp ON na.ArticleID = rp.OriginalArticleID AND rp.UserID = @CurrentUserID
    WHERE (@Category IS NULL OR LOWER(na.Category) = LOWER(@Category))
        AND (@SortBy != 'trending' OR na.PublishDate >= DATEADD(day, -7, GETDATE()))
        -- ONLY ADDITION: Block filter for logged in users
        AND (@CurrentUserID IS NULL OR NOT EXISTS (
            SELECT 1 FROM NewsSitePro2025_UserBlocks ub 
            WHERE ub.BlockerUserID = @CurrentUserID 
              AND ub.BlockedUserID = na.UserID 
              AND ub.IsActive = 1
        ))
    -- EXACT SAME SORTING AS GetAll
    ORDER BY 
        CASE 
            WHEN @SortBy = 'trending' THEN (COALESCE(lc.LikesCount, 0) * 3 + COALESCE(vc.ViewsCount, 0) + COALESCE(rc.RepostCount, 0) * 5)
            ELSE 0
        END DESC,
        na.PublishDate DESC
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;
    
    -- Also return total count for pagination (SAME AS GetAll)
    SELECT COUNT(*) as TotalCount
    FROM NewsSitePro2025_NewsArticles na
    WHERE (@Category IS NULL OR LOWER(na.Category) = LOWER(@Category))
        AND (@SortBy != 'trending' OR na.PublishDate >= DATEADD(day, -7, GETDATE()))
        -- ONLY ADDITION: Block filter for logged in users  
        AND (@CurrentUserID IS NULL OR NOT EXISTS (
            SELECT 1 FROM NewsSitePro2025_UserBlocks ub 
            WHERE ub.BlockerUserID = @CurrentUserID 
              AND ub.BlockedUserID = na.UserID 
              AND ub.IsActive = 1
        ));
END
GO

-- =============================================
-- SP: Get mutual block check (for notifications)
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'NewsSitePro2025_sp_UserBlocks_CheckMutual')
    DROP PROCEDURE NewsSitePro2025_sp_UserBlocks_CheckMutual
GO

CREATE PROCEDURE [dbo].[NewsSitePro2025_sp_UserBlocks_CheckMutual]
    @UserID1 INT,
    @UserID2 INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        CASE WHEN EXISTS (
            SELECT 1 FROM NewsSitePro2025_UserBlocks 
            WHERE BlockerUserID = @UserID1 AND BlockedUserID = @UserID2 AND IsActive = 1
        ) THEN 1 ELSE 0 END as User1BlockedUser2,
        
        CASE WHEN EXISTS (
            SELECT 1 FROM NewsSitePro2025_UserBlocks 
            WHERE BlockerUserID = @UserID2 AND BlockedUserID = @UserID1 AND IsActive = 1
        ) THEN 1 ELSE 0 END as User2BlockedUser1,
        
        CASE WHEN EXISTS (
            SELECT 1 FROM NewsSitePro2025_UserBlocks 
            WHERE ((BlockerUserID = @UserID1 AND BlockedUserID = @UserID2) 
                OR (BlockerUserID = @UserID2 AND BlockedUserID = @UserID1)) 
              AND IsActive = 1
        ) THEN 1 ELSE 0 END as AnyBlockExists;
END
GO

PRINT 'User blocking system setup completed successfully!';
