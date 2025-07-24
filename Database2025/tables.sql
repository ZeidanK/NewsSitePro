-- =============================================
-- NewsSitePro2025: Main Table Definitions
-- =============================================

-- Users Table
CREATE TABLE NewsSitePro2025_Users (
    UserID INT IDENTITY(1,1) PRIMARY KEY,
    Username NVARCHAR(100) NOT NULL UNIQUE,
    Email NVARCHAR(100) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(200) NOT NULL,
    IsAdmin BIT NOT NULL DEFAULT 0,
    IsLocked BIT NOT NULL DEFAULT 0,
    IsActive BIT NOT NULL DEFAULT 1,
    IsBanned BIT NOT NULL DEFAULT 0,
    BannedUntil DATETIME2 NULL,
    BanReason NVARCHAR(500) NULL,
    Bio NVARCHAR(500) NULL,
    JoinDate DATETIME2 NOT NULL DEFAULT GETDATE(),
    LastUpdated DATETIME2 NOT NULL DEFAULT GETDATE(),
    ProfilePicture NVARCHAR(255) NULL
);
GO

-- News Articles Table
CREATE TABLE NewsSitePro2025_NewsArticles (
    ArticleID INT IDENTITY(1,1) PRIMARY KEY,
    Title NVARCHAR(100) NOT NULL,
    Content NVARCHAR(4000) NOT NULL,
    ImageURL NVARCHAR(255) NULL,
    SourceURL NVARCHAR(500) NULL,
    SourceName NVARCHAR(100) NULL,
    Category NVARCHAR(50) NOT NULL,
    UserID INT NOT NULL,
    PublishDate DATETIME2 NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (UserID) REFERENCES NewsSitePro2025_Users(UserID)
);
GO

-- Article Likes Table
CREATE TABLE NewsSitePro2025_ArticleLikes (
    LikeID INT IDENTITY(1,1) PRIMARY KEY,
    ArticleID INT NOT NULL,
    UserID INT NOT NULL,
    LikeDate DATETIME2 NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (ArticleID) REFERENCES NewsSitePro2025_NewsArticles(ArticleID),
    FOREIGN KEY (UserID) REFERENCES NewsSitePro2025_Users(UserID),
    CONSTRAINT UQ_ArticleUser_Like_2025 UNIQUE(ArticleID, UserID)
);
GO

-- Article Views Table
CREATE TABLE NewsSitePro2025_ArticleViews (
    ViewID INT IDENTITY(1,1) PRIMARY KEY,
    ArticleID INT NOT NULL,
    UserID INT NULL,
    ViewDate DATETIME2 NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (ArticleID) REFERENCES NewsSitePro2025_NewsArticles(ArticleID),
    FOREIGN KEY (UserID) REFERENCES NewsSitePro2025_Users(UserID)
);
GO

-- Saved Articles Table
CREATE TABLE NewsSitePro2025_SavedArticles (
    SaveID INT IDENTITY(1,1) PRIMARY KEY,
    ArticleID INT NOT NULL,
    UserID INT NOT NULL,
    SaveDate DATETIME2 NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (ArticleID) REFERENCES NewsSitePro2025_NewsArticles(ArticleID),
    FOREIGN KEY (UserID) REFERENCES NewsSitePro2025_Users(UserID),
    CONSTRAINT UQ_ArticleUser_Save_2025 UNIQUE(ArticleID, UserID)
);
GO

-- Comments Table (Recommended Feature)
CREATE TABLE NewsSitePro2025_Comments (
    CommentID INT IDENTITY(1,1) PRIMARY KEY,
    ArticleID INT NOT NULL,
    UserID INT NOT NULL,
    Content NVARCHAR(1000) NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    IsFlagged BIT NOT NULL DEFAULT 0,
    FOREIGN KEY (ArticleID) REFERENCES NewsSitePro2025_NewsArticles(ArticleID),
    FOREIGN KEY (UserID) REFERENCES NewsSitePro2025_Users(UserID)
);
GO

-- Tags Table (Recommended Feature)
CREATE TABLE NewsSitePro2025_Tags (
    TagID INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(50) NOT NULL UNIQUE
);
GO

-- ArticleTags Table (Recommended Feature)
CREATE TABLE NewsSitePro2025_ArticleTags (
    ArticleID INT NOT NULL,
    TagID INT NOT NULL,
    PRIMARY KEY (ArticleID, TagID),
    FOREIGN KEY (ArticleID) REFERENCES NewsSitePro2025_NewsArticles(ArticleID),
    FOREIGN KEY (TagID) REFERENCES NewsSitePro2025_Tags(TagID)
);
GO

-- UserInterestTags Table (Recommended Feature)
CREATE TABLE NewsSitePro2025_UserInterestTags (
    UserID INT NOT NULL,
    TagID INT NOT NULL,
    PRIMARY KEY (UserID, TagID),
    FOREIGN KEY (UserID) REFERENCES NewsSitePro2025_Users(UserID),
    FOREIGN KEY (TagID) REFERENCES NewsSitePro2025_Tags(TagID)
);
GO

-- Notifications Table
CREATE TABLE NewsSitePro2025_Notifications (
    NotificationID INT IDENTITY(1,1) PRIMARY KEY,
    UserID INT NOT NULL,
    Type NVARCHAR(50) NOT NULL,
    Title NVARCHAR(200) NOT NULL,
    Message NVARCHAR(1000) NOT NULL,
    RelatedEntityType NVARCHAR(50) NULL,
    RelatedEntityID INT NULL,
    IsRead BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    FromUserID INT NULL,
    ActionUrl NVARCHAR(255) NULL,
    FOREIGN KEY (UserID) REFERENCES NewsSitePro2025_Users(UserID),
    FOREIGN KEY (FromUserID) REFERENCES NewsSitePro2025_Users(UserID)
);
GO

-- Notification Preferences Table
CREATE TABLE NewsSitePro2025_NotificationPreferences (
    PreferenceID INT IDENTITY(1,1) PRIMARY KEY,
    UserID INT NOT NULL,
    NotificationType NVARCHAR(50) NOT NULL,
    IsEnabled BIT NOT NULL DEFAULT 1,
    EmailNotification BIT NOT NULL DEFAULT 0,
    PushNotification BIT NOT NULL DEFAULT 1,
    FOREIGN KEY (UserID) REFERENCES NewsSitePro2025_Users(UserID),
    CONSTRAINT UQ_User_NotificationType_2025 UNIQUE(UserID, NotificationType)
);
GO

-- Activity Logs Table
CREATE TABLE NewsSitePro2025_ActivityLogs (
    LogID INT IDENTITY(1,1) PRIMARY KEY,
    UserID INT NOT NULL,
    Action NVARCHAR(100) NOT NULL,
    Details NVARCHAR(500) NOT NULL,
    Timestamp DATETIME2 NOT NULL DEFAULT GETDATE(),
    IpAddress NVARCHAR(50) NOT NULL,
    UserAgent NVARCHAR(500) NOT NULL,
    FOREIGN KEY (UserID) REFERENCES NewsSitePro2025_Users(UserID)
);
GO

-- Reports Table (for articles and users)
CREATE TABLE NewsSitePro2025_Reports (
    ReportID INT IDENTITY(1,1) PRIMARY KEY,
    ArticleID INT NULL,
    ReporterID INT NOT NULL,
    ReportedUserID INT NULL,
    Reason NVARCHAR(255) NOT NULL,
    Description NVARCHAR(1000) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    Status NVARCHAR(20) NOT NULL DEFAULT 'Pending',
    ResolvedBy INT NULL,
    ResolvedAt DATETIME2 NULL,
    ResolutionNotes NVARCHAR(500) NULL,
    FOREIGN KEY (ArticleID) REFERENCES NewsSitePro2025_NewsArticles(ArticleID),
    FOREIGN KEY (ReporterID) REFERENCES NewsSitePro2025_Users(UserID),
    FOREIGN KEY (ReportedUserID) REFERENCES NewsSitePro2025_Users(UserID),
    FOREIGN KEY (ResolvedBy) REFERENCES NewsSitePro2025_Users(UserID)
);
GO

-- User Follows Table (Social Feature)
CREATE TABLE NewsSitePro2025_UserFollows (
    FollowID INT IDENTITY(1,1) PRIMARY KEY,
    FollowerUserID INT NOT NULL,
    FollowedUserID INT NOT NULL,
    FollowDate DATETIME2 NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (FollowerUserID) REFERENCES NewsSitePro2025_Users(UserID),
    FOREIGN KEY (FollowedUserID) REFERENCES NewsSitePro2025_Users(UserID),
    CONSTRAINT UQ_UserFollows_2025 UNIQUE(FollowerUserID, FollowedUserID),
    CHECK (FollowerUserID != FollowedUserID)
);
GO 