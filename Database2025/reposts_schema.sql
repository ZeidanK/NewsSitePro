-- =============================================
-- Reposts Schema for NewsSitePro2025
-- Creates tables and stored procedures for repost functionality
-- Author: System
-- Date: 2025-08-03
-- =============================================

USE [NewsSitePro2025]
GO

-- Create Reposts table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='NewsSitePro2025_Reposts' AND xtype='U')
BEGIN
    CREATE TABLE [dbo].[NewsSitePro2025_Reposts] (
        [RepostID] INT IDENTITY(1,1) PRIMARY KEY,
        [OriginalArticleID] INT NOT NULL,
        [UserID] INT NOT NULL,
        [RepostText] NVARCHAR(500) NULL,  -- Optional text user can add when reposting
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETDATE(),
        [UpdatedAt] DATETIME2 NULL,
        [IsDeleted] BIT NOT NULL DEFAULT 0,
        [DeletedAt] DATETIME2 NULL,
        
        -- Foreign key constraints
        CONSTRAINT [FK_Reposts_Articles] FOREIGN KEY ([OriginalArticleID]) 
            REFERENCES [NewsSitePro2025_NewsArticles]([ArticleID]),
        CONSTRAINT [FK_Reposts_Users] FOREIGN KEY ([UserID]) 
            REFERENCES [NewsSitePro2025_Users]([UserID]),
        
        -- Ensure user can't repost the same article multiple times
        CONSTRAINT [UQ_Reposts_UserArticle] UNIQUE ([UserID], [OriginalArticleID])
    );
    
    PRINT 'Reposts table created successfully';
END
ELSE
BEGIN
    PRINT 'Reposts table already exists';
END
GO

-- Create Repost Likes table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='NewsSitePro2025_RepostLikes' AND xtype='U')
BEGIN
    CREATE TABLE [dbo].[NewsSitePro2025_RepostLikes] (
        [RepostLikeID] INT IDENTITY(1,1) PRIMARY KEY,
        [RepostID] INT NOT NULL,
        [UserID] INT NOT NULL,
        [LikedAt] DATETIME2 NOT NULL DEFAULT GETDATE(),
        [IsDeleted] BIT NOT NULL DEFAULT 0,
        
        -- Foreign key constraints
        CONSTRAINT [FK_RepostLikes_Reposts] FOREIGN KEY ([RepostID]) 
            REFERENCES [NewsSitePro2025_Reposts]([RepostID]),
        CONSTRAINT [FK_RepostLikes_Users] FOREIGN KEY ([UserID]) 
            REFERENCES [NewsSitePro2025_Users]([UserID]),
        
        -- Ensure user can't like the same repost multiple times
        CONSTRAINT [UQ_RepostLikes_UserRepost] UNIQUE ([UserID], [RepostID])
    );
    
    PRINT 'Repost Likes table created successfully';
END
ELSE
BEGIN
    PRINT 'Repost Likes table already exists';
END
GO

-- Create Repost Comments table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='NewsSitePro2025_RepostComments' AND xtype='U')
BEGIN
    CREATE TABLE [dbo].[NewsSitePro2025_RepostComments] (
        [RepostCommentID] INT IDENTITY(1,1) PRIMARY KEY,
        [RepostID] INT NOT NULL,
        [UserID] INT NOT NULL,
        [Content] NVARCHAR(1000) NOT NULL,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETDATE(),
        [UpdatedAt] DATETIME2 NULL,
        [IsDeleted] BIT NOT NULL DEFAULT 0,
        [DeletedAt] DATETIME2 NULL,
        [ParentCommentID] INT NULL, -- For nested comments
        
        -- Foreign key constraints
        CONSTRAINT [FK_RepostComments_Reposts] FOREIGN KEY ([RepostID]) 
            REFERENCES [NewsSitePro2025_Reposts]([RepostID]),
        CONSTRAINT [FK_RepostComments_Users] FOREIGN KEY ([UserID]) 
            REFERENCES [NewsSitePro2025_Users]([UserID]),
        CONSTRAINT [FK_RepostComments_Parent] FOREIGN KEY ([ParentCommentID]) 
            REFERENCES [NewsSitePro2025_RepostComments]([RepostCommentID])
    );
    
    PRINT 'Repost Comments table created successfully';
END
ELSE
BEGIN
    PRINT 'Repost Comments table already exists';
END
GO

-- Create indexes for performance
CREATE NONCLUSTERED INDEX [IX_Reposts_UserID] ON [NewsSitePro2025_Reposts] ([UserID]);
CREATE NONCLUSTERED INDEX [IX_Reposts_OriginalArticleID] ON [NewsSitePro2025_Reposts] ([OriginalArticleID]);
CREATE NONCLUSTERED INDEX [IX_Reposts_CreatedAt] ON [NewsSitePro2025_Reposts] ([CreatedAt]);
CREATE NONCLUSTERED INDEX [IX_RepostLikes_RepostID] ON [NewsSitePro2025_RepostLikes] ([RepostID]);
CREATE NONCLUSTERED INDEX [IX_RepostComments_RepostID] ON [NewsSitePro2025_RepostComments] ([RepostID]);

-- Create trigger to soft delete reposts when original article is soft deleted
IF NOT EXISTS (SELECT * FROM sys.triggers WHERE name = 'TR_SoftDeleteReposts')
BEGIN
    EXEC('
    CREATE TRIGGER [dbo].[TR_SoftDeleteReposts]
    ON [dbo].[NewsSitePro2025_NewsArticles]
    AFTER UPDATE
    AS
    BEGIN
        SET NOCOUNT ON;
        
        -- Soft delete reposts when original article is soft deleted
        UPDATE r
        SET IsDeleted = 1, DeletedAt = GETDATE()
        FROM NewsSitePro2025_Reposts r
        INNER JOIN inserted i ON r.OriginalArticleID = i.ArticleID
        INNER JOIN deleted d ON i.ArticleID = d.ArticleID
        WHERE i.IsDeleted = 1 AND d.IsDeleted = 0;
    END
    ');
    
    PRINT 'Soft delete trigger created successfully';
END
GO

PRINT 'Reposts schema setup completed successfully!';
