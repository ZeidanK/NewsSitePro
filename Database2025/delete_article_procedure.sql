-- =============================================
-- NewsSitePro2025: Soft Delete Article Stored Procedure
-- Creates a soft delete stored procedure for articles
-- =============================================

USE [myProjDB]
GO

-- Add IsDeleted and DeletedAt columns if they don't exist
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('NewsSitePro2025_NewsArticles') AND name = 'IsDeleted')
BEGIN
    ALTER TABLE NewsSitePro2025_NewsArticles ADD IsDeleted BIT NOT NULL DEFAULT 0;
    PRINT 'Added IsDeleted column to NewsArticles table.';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('NewsSitePro2025_NewsArticles') AND name = 'DeletedAt')
BEGIN
    ALTER TABLE NewsSitePro2025_NewsArticles ADD DeletedAt DATETIME2 NULL;
    PRINT 'Added DeletedAt column to NewsArticles table.';
END

-- =============================================
-- SP: Soft Delete News Article
-- Description: Soft deletes a news article (marks as deleted)
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'NewsSitePro2025_sp_NewsArticle_Delete')
    DROP PROCEDURE NewsSitePro2025_sp_NewsArticle_Delete
GO

CREATE PROCEDURE [dbo].[NewsSitePro2025_sp_NewsArticle_Delete]
    @ArticleID INT
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- Check if the article exists and is not already deleted
        IF NOT EXISTS (SELECT 1 FROM NewsSitePro2025_NewsArticles WHERE ArticleID = @ArticleID AND IsDeleted = 0)
        BEGIN
            ROLLBACK TRANSACTION;
            SELECT 0 AS Success, 'Article not found or already deleted' AS Message;
            RETURN;
        END
        
        -- Soft delete the main article
        UPDATE NewsSitePro2025_NewsArticles 
        SET IsDeleted = 1, 
            DeletedAt = GETDATE()
        WHERE ArticleID = @ArticleID AND IsDeleted = 0;
        
        -- Check if the article was actually updated
        IF @@ROWCOUNT = 0
        BEGIN
            ROLLBACK TRANSACTION;
            SELECT 0 AS Success, 'Article could not be deleted' AS Message;
            RETURN;
        END
        
        -- Optionally soft delete related data (comments, likes, etc.)
        -- Note: We're keeping the related data but you could also soft delete them
        
        -- Soft delete comments if they have IsDeleted column
        IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('NewsSitePro2025_Comments') AND name = 'IsDeleted')
        BEGIN
            UPDATE NewsSitePro2025_Comments 
            SET IsDeleted = 1, UpdatedAt = GETDATE()
            WHERE PostID = @ArticleID AND IsDeleted = 0;
        END
        
        -- Delete notifications related to this article (these can be hard deleted)
        IF OBJECT_ID('NewsSitePro2025_Notifications', 'U') IS NOT NULL
            DELETE FROM NewsSitePro2025_Notifications WHERE RelatedEntityID = @ArticleID;
        
        COMMIT TRANSACTION;
        
        -- Return success
        SELECT 1 AS Success, 'Article deleted successfully' AS Message;
        
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
            
        -- Return error
        SELECT 0 AS Success, ERROR_MESSAGE() AS Message;
    END CATCH
END
GO

PRINT 'Delete article stored procedure created successfully!';
