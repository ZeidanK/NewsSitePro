-- =============================================
-- Database Schema for News Site
-- =============================================

-- Create NewsArticles table
CREATE TABLE NewsArticles (
    ArticleID INT IDENTITY(1,1) PRIMARY KEY,
    Title NVARCHAR(100) NOT NULL,
    Content NVARCHAR(500) NOT NULL,
    ImageURL NVARCHAR(255) NULL,
    Category NVARCHAR(50) NOT NULL,
    UserID INT NOT NULL,
    PublishDate DATETIME2 NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (UserID) REFERENCES Users_News(UserID)
);
GO

-- Create ArticleLikes table
CREATE TABLE ArticleLikes (
    LikeID INT IDENTITY(1,1) PRIMARY KEY,
    ArticleID INT NOT NULL,
    UserID INT NOT NULL,
    LikeDate DATETIME2 NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (ArticleID) REFERENCES NewsArticles(ArticleID),
    FOREIGN KEY (UserID) REFERENCES Users_News(UserID),
    UNIQUE(ArticleID, UserID)
);
GO

-- Create SavedArticles table
CREATE TABLE SavedArticles (
    SaveID INT IDENTITY(1,1) PRIMARY KEY,
    ArticleID INT NOT NULL,
    UserID INT NOT NULL,
    SaveDate DATETIME2 NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (ArticleID) REFERENCES NewsArticles(ArticleID),
    FOREIGN KEY (UserID) REFERENCES Users_News(UserID),
    UNIQUE(ArticleID, UserID)
);
GO

-- Create ArticleViews table
CREATE TABLE ArticleViews (
    ViewID INT IDENTITY(1,1) PRIMARY KEY,
    ArticleID INT NOT NULL,
    UserID INT NULL,
    ViewDate DATETIME2 NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (ArticleID) REFERENCES NewsArticles(ArticleID),
    FOREIGN KEY (UserID) REFERENCES Users_News(UserID)
);
GO

-- Create Reports table
CREATE TABLE Reports (
    ReportID INT IDENTITY(1,1) PRIMARY KEY,
    ArticleID INT NOT NULL,
    UserID INT NOT NULL,
    Reason NVARCHAR(255) NULL,
    ReportDate DATETIME2 NOT NULL DEFAULT GETDATE(),
    Status NVARCHAR(20) DEFAULT 'Pending',
    FOREIGN KEY (ArticleID) REFERENCES NewsArticles(ArticleID),
    FOREIGN KEY (UserID) REFERENCES Users_News(UserID),
    UNIQUE(ArticleID, UserID)
);
GO

-- Create UserPreferences table
CREATE TABLE UserPreferences (
    PreferenceID INT IDENTITY(1,1) PRIMARY KEY,
    UserID INT NOT NULL,
    Categories NVARCHAR(255) NULL,
    EmailNotifications BIT DEFAULT 1,
    PushNotifications BIT DEFAULT 0,
    WeeklyDigest BIT DEFAULT 1,
    LastUpdated DATETIME2 NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (UserID) REFERENCES Users_News(UserID),
    UNIQUE(UserID)
);
GO

-- Create UserFollows table (for future following functionality)
CREATE TABLE UserFollows (
    FollowID INT IDENTITY(1,1) PRIMARY KEY,
    FollowerUserID INT NOT NULL,
    FollowedUserID INT NOT NULL,
    FollowDate DATETIME2 NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (FollowerUserID) REFERENCES Users_News(UserID),
    FOREIGN KEY (FollowedUserID) REFERENCES Users_News(UserID),
    UNIQUE(FollowerUserID, FollowedUserID),
    CHECK (FollowerUserID != FollowedUserID)
);
GO

-- Add Bio column to Users_News table if it doesn't exist
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Users_News' AND COLUMN_NAME = 'Bio')
BEGIN
    ALTER TABLE Users_News ADD Bio NVARCHAR(500) NULL;
END
GO

-- Add LastUpdated column to Users_News table if it doesn't exist
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Users_News' AND COLUMN_NAME = 'LastUpdated')
BEGIN
    ALTER TABLE Users_News ADD LastUpdated DATETIME2 NULL;
END
GO

-- Create indexes for better performance
CREATE INDEX IX_NewsArticles_Category ON NewsArticles(Category);
GO

CREATE INDEX IX_NewsArticles_PublishDate ON NewsArticles(PublishDate DESC);
GO

CREATE INDEX IX_NewsArticles_UserID ON NewsArticles(UserID);
GO

CREATE INDEX IX_ArticleLikes_ArticleID ON ArticleLikes(ArticleID);
GO

CREATE INDEX IX_ArticleLikes_UserID ON ArticleLikes(UserID);
GO

CREATE INDEX IX_SavedArticles_UserID ON SavedArticles(UserID);
GO

CREATE INDEX IX_ArticleViews_ArticleID ON ArticleViews(ArticleID);
GO
