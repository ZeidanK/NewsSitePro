-- Posts Table
CREATE TABLE Posts (
    PostID INT IDENTITY(1,1) PRIMARY KEY,
    ArticleID INT NOT NULL,
    UserID INT NOT NULL,
    OriginalPostID INT NULL,
    PostedDate DATETIME NOT NULL DEFAULT GETDATE(),
    LastEditedDate DATETIME NULL,
    DeletedAt DATETIME NULL,
    CONSTRAINT FK_Posts_Articles FOREIGN KEY (ArticleID) REFERENCES Articles(ArticleID),
    CONSTRAINT FK_Posts_Users FOREIGN KEY (UserID) REFERENCES Users(UserID),
    CONSTRAINT FK_Posts_OriginalPosts FOREIGN KEY (OriginalPostID) REFERENCES Posts(PostID)
);

-- Comments Table
CREATE TABLE Comments (
    CommentID INT IDENTITY(1,1) PRIMARY KEY,
    PostID INT NOT NULL,
    UserID INT NOT NULL,
    Content NVARCHAR(1000) NOT NULL,
    CommentDate DATETIME NOT NULL DEFAULT GETDATE(),
    DeletedAt DATETIME NULL,
    CONSTRAINT FK_Comments_Posts FOREIGN KEY (PostID) REFERENCES Posts(PostID),
    CONSTRAINT FK_Comments_Users FOREIGN KEY (UserID) REFERENCES Users(UserID)
);

-- Likes Table
CREATE TABLE Likes (
    LikeID INT IDENTITY(1,1) PRIMARY KEY,
    PostID INT NOT NULL,
    UserID INT NOT NULL,
    LikeDate DATETIME NOT NULL DEFAULT GETDATE(),
    DeletedAt DATETIME NULL,
    CONSTRAINT FK_Likes_Posts FOREIGN KEY (PostID) REFERENCES Posts(PostID),
    CONSTRAINT FK_Likes_Users FOREIGN KEY (UserID) REFERENCES Users(UserID),
    -- Ensure a user can only like a post once
    CONSTRAINT UQ_Likes_PostUser UNIQUE (PostID, UserID)
);

-- Comment Likes Table (recommended addition)
CREATE TABLE CommentLikes (
    CommentLikeID INT IDENTITY(1,1) PRIMARY KEY,
    CommentID INT NOT NULL,
    UserID INT NOT NULL,
    LikeDate DATETIME NOT NULL DEFAULT GETDATE(),
    DeletedAt DATETIME NULL,
    CONSTRAINT FK_CommentLikes_Comments FOREIGN KEY (CommentID) REFERENCES Comments(CommentID),
    CONSTRAINT FK_CommentLikes_Users FOREIGN KEY (UserID) REFERENCES Users(UserID),
    -- Ensure a user can only like a comment once
    CONSTRAINT UQ_CommentLikes_CommentUser UNIQUE (CommentID, UserID)
);

-- Views for calculated fields

-- View to get post details with counts
CREATE VIEW vw_PostDetails AS
SELECT 
    p.PostID, 
    p.ArticleID, 
    p.UserID,
    u.UserName,
    u.ProfileImageURL AS UserProfileImageURL,
    p.OriginalPostID,
    op.UserID AS OriginalUserID,
    ou.UserName AS OriginalPosterName,
    ou.ProfileImageURL AS OriginalPosterProfileImageURL,
    p.PostedDate,
    p.LastEditedDate,
    (SELECT COUNT(*) FROM Likes l WHERE l.PostID = p.PostID AND l.DeletedAt IS NULL) AS LikeCount,
    (SELECT COUNT(*) FROM Comments c WHERE c.PostID = p.PostID AND c.DeletedAt IS NULL) AS CommentCount,
    (SELECT COUNT(*) FROM Posts sp WHERE sp.OriginalPostID = p.PostID AND sp.DeletedAt IS NULL) AS ShareCount,
    CASE WHEN p.OriginalPostID IS NULL THEN 0 ELSE 1 END AS IsShared
FROM 
    Posts p
    LEFT JOIN Users u ON p.UserID = u.UserID
    LEFT JOIN Posts op ON p.OriginalPostID = op.PostID
    LEFT JOIN Users ou ON op.UserID = ou.UserID
WHERE
    p.DeletedAt IS NULL;

-- Function to check if a post is liked by a specific user
CREATE FUNCTION dbo.IsPostLikedByUser(@PostID INT, @UserID INT)
RETURNS BIT
AS
BEGIN
    DECLARE @IsLiked BIT = 0;
    
    IF EXISTS (SELECT 1 FROM Likes WHERE PostID = @PostID AND UserID = @UserID AND DeletedAt IS NULL)
        SET @IsLiked = 1;
    
    RETURN @IsLiked;
END;

CREATE INDEX IX_Posts_UserID ON Posts(UserID);
CREATE INDEX IX_Posts_ArticleID ON Posts(ArticleID);
CREATE INDEX IX_Posts_OriginalPostID ON Posts(OriginalPostID);
CREATE INDEX IX_Comments_PostID ON Comments(PostID);
CREATE INDEX IX_Comments_UserID ON Comments(UserID);
CREATE INDEX IX_Likes_PostID ON Likes(PostID);
CREATE INDEX IX_Likes_UserID ON Likes(UserID);


CREATE PROCEDURE GetUserFeed
    @UserID INT,
    @PageNumber INT = 1,
    @PageSize INT = 20
AS
BEGIN
    SELECT *
    FROM vw_PostDetails
    WHERE UserID IN (
        SELECT FollowedUserID 
        FROM UserFollows 
        WHERE FollowerUserID = @UserID
    )
    ORDER BY PostedDate DESC
    OFFSET (@PageNumber - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END;


CREATE TABLE Notifications (
    NotificationID INT IDENTITY(1,1) PRIMARY KEY,
    UserID INT NOT NULL,
    PostID INT NULL,
    CommentID INT NULL,
    NotificationType VARCHAR(20) NOT NULL, -- 'LIKE', 'COMMENT', 'SHARE', etc.
    SourceUserID INT NOT NULL,
    IsRead BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_Notifications_Users FOREIGN KEY (UserID) REFERENCES Users(UserID),
    CONSTRAINT FK_Notifications_Posts FOREIGN KEY (PostID) REFERENCES Posts(PostID),
    CONSTRAINT FK_Notifications_Comments FOREIGN KEY (CommentID) REFERENCES Comments(CommentID),
    CONSTRAINT FK_Notifications_SourceUsers FOREIGN KEY (SourceUserID) REFERENCES Users(UserID)
);