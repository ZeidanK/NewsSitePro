-- =============================================
-- Enhanced Comments System Tables
-- =============================================

-- Drop existing comments table if it exists
IF OBJECT_ID('NewsSitePro2025_Comments', 'U') IS NOT NULL
    DROP TABLE NewsSitePro2025_Comments;
GO

-- Enhanced Comments Table (supports both ArticleID and PostID for flexibility)
CREATE TABLE NewsSitePro2025_Comments (
    CommentID INT IDENTITY(1,1) PRIMARY KEY,
    PostID INT NOT NULL, -- Can reference either NewsArticles.ArticleID or Posts.PostID
    UserID INT NOT NULL,
    Content NVARCHAR(1000) NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME2 NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    ParentCommentID INT NULL, -- For nested/reply comments
    IsFlagged BIT NOT NULL DEFAULT 0,
    FOREIGN KEY (PostID) REFERENCES NewsSitePro2025_NewsArticles(ArticleID),
    FOREIGN KEY (UserID) REFERENCES NewsSitePro2025_Users(UserID),
    FOREIGN KEY (ParentCommentID) REFERENCES NewsSitePro2025_Comments(CommentID)
);
GO

-- Comment Likes Table
CREATE TABLE NewsSitePro2025_CommentLikes (
    CommentLikeID INT IDENTITY(1,1) PRIMARY KEY,
    CommentID INT NOT NULL,
    UserID INT NOT NULL,
    LikeDate DATETIME2 NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (CommentID) REFERENCES NewsSitePro2025_Comments(CommentID) ON DELETE CASCADE,
    FOREIGN KEY (UserID) REFERENCES NewsSitePro2025_Users(UserID),
    CONSTRAINT UQ_CommentUser_Like_2025 UNIQUE(CommentID, UserID)
);
GO

-- Indexes for better performance
CREATE INDEX IX_NewsSitePro2025_Comments_PostID ON NewsSitePro2025_Comments(PostID);
CREATE INDEX IX_NewsSitePro2025_Comments_UserID ON NewsSitePro2025_Comments(UserID);
CREATE INDEX IX_NewsSitePro2025_Comments_ParentCommentID ON NewsSitePro2025_Comments(ParentCommentID);
CREATE INDEX IX_NewsSitePro2025_Comments_CreatedAt ON NewsSitePro2025_Comments(CreatedAt);
CREATE INDEX IX_NewsSitePro2025_CommentLikes_CommentID ON NewsSitePro2025_CommentLikes(CommentID);
CREATE INDEX IX_NewsSitePro2025_CommentLikes_UserID ON NewsSitePro2025_CommentLikes(UserID);
GO
