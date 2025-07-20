-- NewsSitePro Comment System Stored Procedures
-- Run these procedures in your SQL Server database

USE [NewsSitePro2025]
GO

-- Create Comments table if it doesn't exist
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Comments' AND xtype='U')
BEGIN
    CREATE TABLE [dbo].[Comments] (
        [ID] int IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [PostID] int NOT NULL,
        [UserID] int NOT NULL,
        [Content] nvarchar(1000) NOT NULL,
        [ParentCommentID] int NULL,
        [CreatedAt] datetime2(7) NOT NULL DEFAULT GETDATE(),
        [UpdatedAt] datetime2(7) NULL,
        [IsDeleted] bit NOT NULL DEFAULT 0,
        
        CONSTRAINT [FK_Comments_Posts] FOREIGN KEY ([PostID]) 
            REFERENCES [dbo].[Posts] ([ID]) ON DELETE CASCADE,
        CONSTRAINT [FK_Comments_Users] FOREIGN KEY ([UserID]) 
            REFERENCES [dbo].[Users] ([ID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Comments_ParentComment] FOREIGN KEY ([ParentCommentID]) 
            REFERENCES [dbo].[Comments] ([ID]) ON DELETE NO ACTION
    );
    
    -- Create indexes for better performance
    CREATE INDEX [IX_Comments_PostID] ON [dbo].[Comments] ([PostID]);
    CREATE INDEX [IX_Comments_UserID] ON [dbo].[Comments] ([UserID]);
    CREATE INDEX [IX_Comments_ParentCommentID] ON [dbo].[Comments] ([ParentCommentID]);
    CREATE INDEX [IX_Comments_CreatedAt] ON [dbo].[Comments] ([CreatedAt]);
END
GO

-- Stored Procedure: Get Comments by Post ID
CREATE OR ALTER PROCEDURE [dbo].[NewsSitePro2025_sp_Comments_GetByPostID]
    @PostID int
AS
BEGIN
    SET NOCOUNT ON;
    
    WITH CommentHierarchy AS (
        -- Get parent comments
        SELECT 
            c.ID,
            c.PostID,
            c.UserID,
            u.FirstName + ' ' + u.LastName AS UserName,
            c.Content,
            c.ParentCommentID,
            c.CreatedAt,
            c.UpdatedAt,
            0 AS Level
        FROM Comments c
        INNER JOIN Users u ON c.UserID = u.ID
        WHERE c.PostID = @PostID 
            AND c.ParentCommentID IS NULL 
            AND c.IsDeleted = 0
        
        UNION ALL
        
        -- Get child comments (replies)
        SELECT 
            c.ID,
            c.PostID,
            c.UserID,
            u.FirstName + ' ' + u.LastName AS UserName,
            c.Content,
            c.ParentCommentID,
            c.CreatedAt,
            c.UpdatedAt,
            ch.Level + 1
        FROM Comments c
        INNER JOIN Users u ON c.UserID = u.ID
        INNER JOIN CommentHierarchy ch ON c.ParentCommentID = ch.ID
        WHERE c.IsDeleted = 0
    )
    SELECT 
        ID,
        PostID,
        UserID,
        UserName,
        Content,
        ParentCommentID,
        CreatedAt,
        UpdatedAt,
        Level
    FROM CommentHierarchy
    ORDER BY 
        CASE WHEN ParentCommentID IS NULL THEN ID ELSE ParentCommentID END,
        CASE WHEN ParentCommentID IS NULL THEN 0 ELSE 1 END,
        CreatedAt;
END
GO

-- Stored Procedure: Insert Comment
CREATE OR ALTER PROCEDURE [dbo].[NewsSitePro2025_sp_Comments_Insert]
    @PostID int,
    @UserID int,
    @Content nvarchar(1000),
    @ParentCommentID int = NULL,
    @CommentID int OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Validate input
    IF @PostID IS NULL OR @UserID IS NULL OR @Content IS NULL OR LEN(TRIM(@Content)) = 0
    BEGIN
        RAISERROR('PostID, UserID, and Content are required', 16, 1);
        RETURN;
    END
    
    -- Check if post exists
    IF NOT EXISTS (SELECT 1 FROM Posts WHERE ID = @PostID)
    BEGIN
        RAISERROR('Post not found', 16, 1);
        RETURN;
    END
    
    -- Check if user exists
    IF NOT EXISTS (SELECT 1 FROM Users WHERE ID = @UserID)
    BEGIN
        RAISERROR('User not found', 16, 1);
        RETURN;
    END
    
    -- Check if parent comment exists (if provided)
    IF @ParentCommentID IS NOT NULL AND NOT EXISTS (SELECT 1 FROM Comments WHERE ID = @ParentCommentID AND PostID = @PostID)
    BEGIN
        RAISERROR('Parent comment not found or belongs to different post', 16, 1);
        RETURN;
    END
    
    INSERT INTO Comments (PostID, UserID, Content, ParentCommentID, CreatedAt)
    VALUES (@PostID, @UserID, @Content, @ParentCommentID, GETDATE());
    
    SET @CommentID = SCOPE_IDENTITY();
END
GO

-- Stored Procedure: Update Comment
CREATE OR ALTER PROCEDURE [dbo].[NewsSitePro2025_sp_Comments_Update]
    @CommentID int,
    @UserID int,
    @Content nvarchar(1000)
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Validate input
    IF @CommentID IS NULL OR @UserID IS NULL OR @Content IS NULL OR LEN(TRIM(@Content)) = 0
    BEGIN
        RAISERROR('CommentID, UserID, and Content are required', 16, 1);
        RETURN;
    END
    
    -- Check if comment exists and belongs to the user
    IF NOT EXISTS (SELECT 1 FROM Comments WHERE ID = @CommentID AND UserID = @UserID AND IsDeleted = 0)
    BEGIN
        RAISERROR('Comment not found or you do not have permission to edit this comment', 16, 1);
        RETURN;
    END
    
    UPDATE Comments 
    SET Content = @Content, UpdatedAt = GETDATE()
    WHERE ID = @CommentID AND UserID = @UserID;
    
    IF @@ROWCOUNT = 0
    BEGIN
        RAISERROR('Failed to update comment', 16, 1);
        RETURN;
    END
END
GO

-- Stored Procedure: Delete Comment (Soft Delete)
CREATE OR ALTER PROCEDURE [dbo].[NewsSitePro2025_sp_Comments_Delete]
    @CommentID int,
    @UserID int
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Validate input
    IF @CommentID IS NULL OR @UserID IS NULL
    BEGIN
        RAISERROR('CommentID and UserID are required', 16, 1);
        RETURN;
    END
    
    -- Check if comment exists and belongs to the user
    IF NOT EXISTS (SELECT 1 FROM Comments WHERE ID = @CommentID AND UserID = @UserID AND IsDeleted = 0)
    BEGIN
        RAISERROR('Comment not found or you do not have permission to delete this comment', 16, 1);
        RETURN;
    END
    
    -- Soft delete the comment and its replies
    WITH CommentTree AS (
        -- Get the comment to delete
        SELECT ID FROM Comments WHERE ID = @CommentID
        
        UNION ALL
        
        -- Get all child comments (replies)
        SELECT c.ID 
        FROM Comments c 
        INNER JOIN CommentTree ct ON c.ParentCommentID = ct.ID
    )
    UPDATE Comments 
    SET IsDeleted = 1, UpdatedAt = GETDATE()
    WHERE ID IN (SELECT ID FROM CommentTree);
    
    IF @@ROWCOUNT = 0
    BEGIN
        RAISERROR('Failed to delete comment', 16, 1);
        RETURN;
    END
END
GO

-- Stored Procedure: Get Comments Count by Post ID
CREATE OR ALTER PROCEDURE [dbo].[NewsSitePro2025_sp_Comments_GetCount]
    @PostID int,
    @Count int OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT @Count = COUNT(*)
    FROM Comments 
    WHERE PostID = @PostID AND IsDeleted = 0;
    
    IF @Count IS NULL
        SET @Count = 0;
END
GO

-- Stored Procedure: Get Comment by ID
CREATE OR ALTER PROCEDURE [dbo].[NewsSitePro2025_sp_Comments_GetByID]
    @CommentID int
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        c.ID,
        c.PostID,
        c.UserID,
        u.FirstName + ' ' + u.LastName AS UserName,
        c.Content,
        c.ParentCommentID,
        c.CreatedAt,
        c.UpdatedAt
    FROM Comments c
    INNER JOIN Users u ON c.UserID = u.ID
    WHERE c.ID = @CommentID AND c.IsDeleted = 0;
END
GO

-- Stored Procedure: Check if user exists by email
CREATE OR ALTER PROCEDURE [dbo].[NewsSitePro2025_sp_Users_EmailExists]
    @Email nvarchar(255),
    @Exists bit OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    IF EXISTS (SELECT 1 FROM Users WHERE Email = @Email)
        SET @Exists = 1;
    ELSE
        SET @Exists = 0;
END
GO

PRINT 'NewsSitePro Comment System Stored Procedures created successfully!';
