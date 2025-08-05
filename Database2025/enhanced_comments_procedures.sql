-- =============================================
-- Enhanced Comments System Stored Procedures
-- =============================================

-- Get Comments by Post ID (the one your code is calling)
CREATE PROCEDURE NewsSitePro2025_sp_Comments_GetByPostID
    @PostID INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        c.CommentID, 
        c.PostID, 
        c.UserID, 
        c.Content, 
        c.CreatedAt, 
        c.UpdatedAt,
        c.IsDeleted, 
        c.ParentCommentID, 
        u.Username as UserName,
        (SELECT COUNT(*) FROM NewsSitePro2025_CommentLikes cl WHERE cl.CommentID = c.CommentID) as LikesCount
    FROM NewsSitePro2025_Comments c
    INNER JOIN NewsSitePro2025_Users u ON c.UserID = u.UserID
    WHERE c.PostID = @PostID AND c.IsDeleted = 0
    ORDER BY c.CreatedAt ASC;
END
GO

-- Insert New Comment
CREATE PROCEDURE NewsSitePro2025_sp_Comments_Insert
    @PostID INT,
    @UserID INT,
    @Content NVARCHAR(1000),
    @ParentCommentID INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @CommentID INT;
    
    INSERT INTO NewsSitePro2025_Comments (PostID, UserID, Content, ParentCommentID, CreatedAt)
    VALUES (@PostID, @UserID, @Content, @ParentCommentID, GETDATE());
    
    SET @CommentID = SCOPE_IDENTITY();
    
    -- Return the newly created comment with user info
    SELECT 
        c.CommentID, 
        c.PostID, 
        c.UserID, 
        c.Content, 
        c.CreatedAt, 
        c.UpdatedAt,
        c.IsDeleted, 
        c.ParentCommentID, 
        u.Username as UserName,
        (SELECT COUNT(*) FROM NewsSitePro2025_CommentLikes cl WHERE cl.CommentID = c.CommentID) as LikesCount
    FROM NewsSitePro2025_Comments c
    INNER JOIN NewsSitePro2025_Users u ON c.UserID = u.UserID
    WHERE c.CommentID = @CommentID;
END
GO

-- Update Comment
CREATE PROCEDURE NewsSitePro2025_sp_Comments_Update
    @CommentID INT,
    @UserID INT,
    @Content NVARCHAR(1000)
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE NewsSitePro2025_Comments 
    SET Content = @Content, UpdatedAt = GETDATE()
    WHERE CommentID = @CommentID AND UserID = @UserID AND IsDeleted = 0;
    
    SELECT @@ROWCOUNT as RowsAffected;
END
GO

-- Soft Delete Comment
CREATE PROCEDURE NewsSitePro2025_sp_Comments_Delete
    @CommentID INT,
    @UserID INT
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE NewsSitePro2025_Comments 
    SET IsDeleted = 1, UpdatedAt = GETDATE()
    WHERE CommentID = @CommentID AND UserID = @UserID;
    
    SELECT @@ROWCOUNT as RowsAffected;
END
GO

-- Get Comment by ID
CREATE PROCEDURE NewsSitePro2025_sp_Comments_GetByID
    @CommentID INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        c.CommentID, 
        c.PostID, 
        c.UserID, 
        c.Content, 
        c.CreatedAt, 
        c.UpdatedAt,
        c.IsDeleted, 
        c.ParentCommentID, 
        u.Username as UserName,
        (SELECT COUNT(*) FROM NewsSitePro2025_CommentLikes cl WHERE cl.CommentID = c.CommentID) as LikesCount
    FROM NewsSitePro2025_Comments c
    INNER JOIN NewsSitePro2025_Users u ON c.UserID = u.UserID
    WHERE c.CommentID = @CommentID AND c.IsDeleted = 0;
END
GO

-- Like/Unlike Comment
CREATE PROCEDURE NewsSitePro2025_sp_Comments_ToggleLike
    @CommentID INT,
    @UserID INT
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Exists INT;
    SELECT @Exists = COUNT(*) FROM NewsSitePro2025_CommentLikes 
    WHERE CommentID = @CommentID AND UserID = @UserID;
    
    IF @Exists > 0
    BEGIN
        -- Unlike
        DELETE FROM NewsSitePro2025_CommentLikes 
        WHERE CommentID = @CommentID AND UserID = @UserID;
        SELECT 'unliked' as Action, 
               (SELECT COUNT(*) FROM NewsSitePro2025_CommentLikes WHERE CommentID = @CommentID) as LikesCount;
    END
    ELSE
    BEGIN
        -- Like
        INSERT INTO NewsSitePro2025_CommentLikes (CommentID, UserID, LikeDate)
        VALUES (@CommentID, @UserID, GETDATE());
        SELECT 'liked' as Action, 
               (SELECT COUNT(*) FROM NewsSitePro2025_CommentLikes WHERE CommentID = @CommentID) as LikesCount;
    END
END
GO

-- Get Comments Count for Post
CREATE PROCEDURE NewsSitePro2025_sp_Comments_GetCountByPostID
    @PostID INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT COUNT(*) as CommentsCount
    FROM NewsSitePro2025_Comments 
    WHERE PostID = @PostID AND IsDeleted = 0;
END
GO

-- Get Recent Comments by User
CREATE PROCEDURE NewsSitePro2025_sp_Comments_GetByUserID
    @UserID INT,
    @PageNumber INT = 1,
    @PageSize INT = 20
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;
    
    SELECT 
        c.CommentID, 
        c.PostID, 
        c.UserID, 
        c.Content, 
        c.CreatedAt, 
        c.UpdatedAt,
        c.IsDeleted, 
        c.ParentCommentID, 
        u.Username as UserName,
        (SELECT COUNT(*) FROM NewsSitePro2025_CommentLikes cl WHERE cl.CommentID = c.CommentID) as LikesCount,
        a.Title as PostTitle
    FROM NewsSitePro2025_Comments c
    INNER JOIN NewsSitePro2025_Users u ON c.UserID = u.UserID
    INNER JOIN NewsSitePro2025_NewsArticles a ON c.PostID = a.ArticleID
    WHERE c.UserID = @UserID AND c.IsDeleted = 0
    ORDER BY c.CreatedAt DESC
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END
GO

-- Admin: Get All Comments (for moderation)
CREATE PROCEDURE NewsSitePro2025_sp_Comments_GetAll
    @PageNumber INT = 1,
    @PageSize INT = 50,
    @IncludeDeleted BIT = 0
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;
    
    SELECT 
        c.CommentID, 
        c.PostID, 
        c.UserID, 
        c.Content, 
        c.CreatedAt, 
        c.UpdatedAt,
        c.IsDeleted, 
        c.ParentCommentID, 
        c.IsFlagged,
        u.Username as UserName,
        (SELECT COUNT(*) FROM NewsSitePro2025_CommentLikes cl WHERE cl.CommentID = c.CommentID) as LikesCount,
        a.Title as PostTitle
    FROM NewsSitePro2025_Comments c
    INNER JOIN NewsSitePro2025_Users u ON c.UserID = u.UserID
    INNER JOIN NewsSitePro2025_NewsArticles a ON c.PostID = a.ArticleID
    WHERE (@IncludeDeleted = 1 OR c.IsDeleted = 0)
    ORDER BY c.CreatedAt DESC
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END
GO

-- Flag/Unflag Comment
CREATE PROCEDURE NewsSitePro2025_sp_Comments_ToggleFlag
    @CommentID INT,
    @AdminUserID INT
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @CurrentFlag BIT;
    SELECT @CurrentFlag = IsFlagged FROM NewsSitePro2025_Comments WHERE CommentID = @CommentID;
    
    UPDATE NewsSitePro2025_Comments 
    SET IsFlagged = CASE WHEN @CurrentFlag = 1 THEN 0 ELSE 1 END,
        UpdatedAt = GETDATE()
    WHERE CommentID = @CommentID;
    
    SELECT 
        CommentID,
        IsFlagged,
        'Comment flag status updated' as Message
    FROM NewsSitePro2025_Comments 
    WHERE CommentID = @CommentID;
END
GO
