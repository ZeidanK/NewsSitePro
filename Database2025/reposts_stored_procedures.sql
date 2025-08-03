-- =============================================
-- Stored Procedures for Repost Functionality
-- Handles repost creation, management, likes, and comments
-- Author: System
-- Date: 2025-08-03
-- =============================================

USE [NewsSitePro2025]
GO

-- =============================================
-- SP: Create a new repost
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'NewsSitePro2025_sp_Reposts_Create')
    DROP PROCEDURE NewsSitePro2025_sp_Reposts_Create
GO

CREATE PROCEDURE [dbo].[NewsSitePro2025_sp_Reposts_Create]
    @OriginalArticleID INT,
    @UserID INT,
    @RepostText NVARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @RepostID INT;
    
    BEGIN TRY
        -- Check if article exists and is not deleted
        IF NOT EXISTS (SELECT 1 FROM NewsSitePro2025_NewsArticles WHERE ArticleID = @OriginalArticleID AND IsDeleted = 0)
        BEGIN
            RAISERROR('Article not found or has been deleted', 16, 1);
            RETURN -1;
        END
        
        -- Check if user already reposted this article
        IF EXISTS (SELECT 1 FROM NewsSitePro2025_Reposts WHERE OriginalArticleID = @OriginalArticleID AND UserID = @UserID AND IsDeleted = 0)
        BEGIN
            RAISERROR('User has already reposted this article', 16, 1);
            RETURN -2;
        END
        
        -- Create repost
        INSERT INTO NewsSitePro2025_Reposts (OriginalArticleID, UserID, RepostText)
        VALUES (@OriginalArticleID, @UserID, @RepostText);
        
        SET @RepostID = SCOPE_IDENTITY();
        
        SELECT @RepostID AS RepostID;
        RETURN @RepostID;
    END TRY
    BEGIN CATCH
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        RAISERROR(@ErrorMessage, 16, 1);
        RETURN -1;
    END CATCH
END
GO

-- =============================================
-- SP: Get reposts for a user's feed
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'NewsSitePro2025_sp_Reposts_GetFeed')
    DROP PROCEDURE NewsSitePro2025_sp_Reposts_GetFeed
GO

CREATE PROCEDURE [dbo].[NewsSitePro2025_sp_Reposts_GetFeed]
    @UserID INT,
    @PageNumber INT = 1,
    @PageSize INT = 10
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;
    
    SELECT 
        r.RepostID,
        r.OriginalArticleID,
        r.UserID,
        r.RepostText,
        r.CreatedAt,
        
        -- Original article details
        a.Title,
        a.Content,
        a.ImageURL,
        a.PublishDate,
        a.Category,
        a.SourceURL,
        a.SourceName,
        a.UserID AS OriginalAuthorID,
        
        -- Repost author details
        u.Username AS RepostAuthor,
        u.Name AS RepostAuthorName,
        u.ProfileImagePath AS RepostAuthorImage,
        
        -- Original author details
        ou.Username AS OriginalAuthor,
        ou.Name AS OriginalAuthorName,
        ou.ProfileImagePath AS OriginalAuthorImage,
        
        -- Interaction counts
        (SELECT COUNT(*) FROM NewsSitePro2025_RepostLikes rl WHERE rl.RepostID = r.RepostID AND rl.IsDeleted = 0) AS LikesCount,
        (SELECT COUNT(*) FROM NewsSitePro2025_RepostComments rc WHERE rc.RepostID = r.RepostID AND rc.IsDeleted = 0) AS CommentsCount,
        
        -- Check if current user liked this repost
        CASE WHEN EXISTS (SELECT 1 FROM NewsSitePro2025_RepostLikes rl WHERE rl.RepostID = r.RepostID AND rl.UserID = @UserID AND rl.IsDeleted = 0) 
             THEN 1 ELSE 0 END AS IsLikedByUser
    FROM NewsSitePro2025_Reposts r
    INNER JOIN NewsSitePro2025_NewsArticles a ON r.OriginalArticleID = a.ArticleID
    INNER JOIN NewsSitePro2025_Users u ON r.UserID = u.UserID
    INNER JOIN NewsSitePro2025_Users ou ON a.UserID = ou.UserID
    WHERE r.IsDeleted = 0 AND a.IsDeleted = 0
    ORDER BY r.CreatedAt DESC
    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
END
GO

-- =============================================
-- SP: Toggle repost like
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'NewsSitePro2025_sp_RepostLikes_Toggle')
    DROP PROCEDURE NewsSitePro2025_sp_RepostLikes_Toggle
GO

CREATE PROCEDURE [dbo].[NewsSitePro2025_sp_RepostLikes_Toggle]
    @RepostID INT,
    @UserID INT
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @IsLiked BIT = 0;
    DECLARE @LikeCount INT;
    
    BEGIN TRY
        -- Check if already liked
        IF EXISTS (SELECT 1 FROM NewsSitePro2025_RepostLikes WHERE RepostID = @RepostID AND UserID = @UserID AND IsDeleted = 0)
        BEGIN
            -- Unlike
            UPDATE NewsSitePro2025_RepostLikes 
            SET IsDeleted = 1
            WHERE RepostID = @RepostID AND UserID = @UserID;
            SET @IsLiked = 0;
        END
        ELSE
        BEGIN
            -- Like (or restore previous like)
            IF EXISTS (SELECT 1 FROM NewsSitePro2025_RepostLikes WHERE RepostID = @RepostID AND UserID = @UserID)
            BEGIN
                UPDATE NewsSitePro2025_RepostLikes 
                SET IsDeleted = 0, LikedAt = GETDATE()
                WHERE RepostID = @RepostID AND UserID = @UserID;
            END
            ELSE
            BEGIN
                INSERT INTO NewsSitePro2025_RepostLikes (RepostID, UserID)
                VALUES (@RepostID, @UserID);
            END
            SET @IsLiked = 1;
        END
        
        -- Get current like count
        SELECT @LikeCount = COUNT(*)
        FROM NewsSitePro2025_RepostLikes 
        WHERE RepostID = @RepostID AND IsDeleted = 0;
        
        SELECT @IsLiked AS IsLiked, @LikeCount AS LikeCount;
    END TRY
    BEGIN CATCH
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        RAISERROR(@ErrorMessage, 16, 1);
    END CATCH
END
GO

-- =============================================
-- SP: Create repost comment
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'NewsSitePro2025_sp_RepostComments_Create')
    DROP PROCEDURE NewsSitePro2025_sp_RepostComments_Create
GO

CREATE PROCEDURE [dbo].[NewsSitePro2025_sp_RepostComments_Create]
    @RepostID INT,
    @UserID INT,
    @Content NVARCHAR(1000),
    @ParentCommentID INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @CommentID INT;
    
    BEGIN TRY
        INSERT INTO NewsSitePro2025_RepostComments (RepostID, UserID, Content, ParentCommentID)
        VALUES (@RepostID, @UserID, @Content, @ParentCommentID);
        
        SET @CommentID = SCOPE_IDENTITY();
        
        SELECT @CommentID AS CommentID;
        RETURN @CommentID;
    END TRY
    BEGIN CATCH
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        RAISERROR(@ErrorMessage, 16, 1);
        RETURN -1;
    END CATCH
END
GO

-- =============================================
-- SP: Get repost comments
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'NewsSitePro2025_sp_RepostComments_GetByRepostID')
    DROP PROCEDURE NewsSitePro2025_sp_RepostComments_GetByRepostID
GO

CREATE PROCEDURE [dbo].[NewsSitePro2025_sp_RepostComments_GetByRepostID]
    @RepostID INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        rc.RepostCommentID,
        rc.RepostID,
        rc.UserID,
        rc.Content,
        rc.CreatedAt,
        rc.UpdatedAt,
        rc.ParentCommentID,
        u.Username,
        u.Name AS UserName,
        u.ProfileImagePath AS UserProfileImage
    FROM NewsSitePro2025_RepostComments rc
    INNER JOIN NewsSitePro2025_Users u ON rc.UserID = u.UserID
    WHERE rc.RepostID = @RepostID AND rc.IsDeleted = 0
    ORDER BY rc.CreatedAt ASC;
END
GO

-- =============================================
-- SP: Delete repost (soft delete)
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'NewsSitePro2025_sp_Reposts_Delete')
    DROP PROCEDURE NewsSitePro2025_sp_Reposts_Delete
GO

CREATE PROCEDURE [dbo].[NewsSitePro2025_sp_Reposts_Delete]
    @RepostID INT,
    @UserID INT
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        -- Verify ownership
        IF NOT EXISTS (SELECT 1 FROM NewsSitePro2025_Reposts WHERE RepostID = @RepostID AND UserID = @UserID AND IsDeleted = 0)
        BEGIN
            RAISERROR('Repost not found or access denied', 16, 1);
            RETURN -1;
        END
        
        -- Soft delete repost
        UPDATE NewsSitePro2025_Reposts 
        SET IsDeleted = 1, DeletedAt = GETDATE()
        WHERE RepostID = @RepostID AND UserID = @UserID;
        
        RETURN 1;
    END TRY
    BEGIN CATCH
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        RAISERROR(@ErrorMessage, 16, 1);
        RETURN -1;
    END CATCH
END
GO

PRINT 'Repost stored procedures created successfully!';
