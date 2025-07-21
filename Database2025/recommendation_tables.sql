-- =============================================
-- Recommendation System Database Tables
-- Following NewsSitePro2025 naming convention
-- =============================================

-- User Interests Table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='NewsSitePro2025_UserInterests' AND xtype='U')
BEGIN
    CREATE TABLE NewsSitePro2025_UserInterests (
        InterestID int IDENTITY(1,1) PRIMARY KEY,
        UserID int NOT NULL,
        Category nvarchar(100) NOT NULL,
        InterestScore float NOT NULL DEFAULT 0.0,
        LastUpdated datetime NOT NULL DEFAULT GETDATE(),
        CONSTRAINT FK_NewsSitePro2025_UserInterests_Users FOREIGN KEY (UserID) REFERENCES Users_News(ID),
        CONSTRAINT UQ_NewsSitePro2025_UserInterests_UserCategory UNIQUE (UserID, Category)
    );
END
GO

-- User Behavior Table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='NewsSitePro2025_UserBehavior' AND xtype='U')
BEGIN
    CREATE TABLE NewsSitePro2025_UserBehavior (
        UserID int PRIMARY KEY,
        TotalViews int NOT NULL DEFAULT 0,
        TotalLikes int NOT NULL DEFAULT 0,
        TotalShares int NOT NULL DEFAULT 0,
        TotalComments int NOT NULL DEFAULT 0,
        AvgSessionDuration float NOT NULL DEFAULT 0.0,
        LastActivity datetime NOT NULL DEFAULT GETDATE(),
        PreferredReadingTime time NOT NULL DEFAULT '08:00:00',
        MostActiveHour int NOT NULL DEFAULT 8,
        FavoriteCategories nvarchar(500) NULL,
        CONSTRAINT FK_NewsSitePro2025_UserBehavior_Users FOREIGN KEY (UserID) REFERENCES Users_News(ID)
    );
END
GO

-- Article Interactions Table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='NewsSitePro2025_ArticleInteractions' AND xtype='U')
BEGIN
    CREATE TABLE NewsSitePro2025_ArticleInteractions (
        InteractionID int IDENTITY(1,1) PRIMARY KEY,
        UserID int NOT NULL,
        ArticleID int NOT NULL,
        InteractionType nvarchar(50) NOT NULL, -- 'view', 'like', 'share', 'comment', 'save'
        Timestamp datetime NOT NULL DEFAULT GETDATE(),
        CONSTRAINT FK_NewsSitePro2025_ArticleInteractions_Users FOREIGN KEY (UserID) REFERENCES Users_News(ID),
        CONSTRAINT FK_NewsSitePro2025_ArticleInteractions_Articles FOREIGN KEY (ArticleID) REFERENCES NewsArticles(ArticleID)
    );
END
GO

-- Feed Configurations Table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='NewsSitePro2025_FeedConfigurations' AND xtype='U')
BEGIN
    CREATE TABLE NewsSitePro2025_FeedConfigurations (
        UserID int PRIMARY KEY,
        PersonalizationWeight float NOT NULL DEFAULT 0.4,
        FreshnessWeight float NOT NULL DEFAULT 0.3,
        PopularityWeight float NOT NULL DEFAULT 0.2,
        SerendipityWeight float NOT NULL DEFAULT 0.1,
        MaxArticlesPerFeed int NOT NULL DEFAULT 20,
        PreferredCategories nvarchar(500) NULL,
        ExcludedCategories nvarchar(500) NULL,
        LastUpdated datetime NOT NULL DEFAULT GETDATE(),
        CONSTRAINT FK_NewsSitePro2025_FeedConfigurations_Users FOREIGN KEY (UserID) REFERENCES Users_News(ID)
    );
END
GO

-- Create indexes for better performance
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_NewsSitePro2025_UserInterests_UserID' AND object_id = OBJECT_ID('NewsSitePro2025_UserInterests'))
    CREATE NONCLUSTERED INDEX IX_NewsSitePro2025_UserInterests_UserID ON NewsSitePro2025_UserInterests (UserID);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_NewsSitePro2025_UserInterests_Category' AND object_id = OBJECT_ID('NewsSitePro2025_UserInterests'))
    CREATE NONCLUSTERED INDEX IX_NewsSitePro2025_UserInterests_Category ON NewsSitePro2025_UserInterests (Category);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_NewsSitePro2025_ArticleInteractions_UserID' AND object_id = OBJECT_ID('NewsSitePro2025_ArticleInteractions'))
    CREATE NONCLUSTERED INDEX IX_NewsSitePro2025_ArticleInteractions_UserID ON NewsSitePro2025_ArticleInteractions (UserID);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_NewsSitePro2025_ArticleInteractions_ArticleID' AND object_id = OBJECT_ID('NewsSitePro2025_ArticleInteractions'))
    CREATE NONCLUSTERED INDEX IX_NewsSitePro2025_ArticleInteractions_ArticleID ON NewsSitePro2025_ArticleInteractions (ArticleID);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_NewsSitePro2025_ArticleInteractions_Type' AND object_id = OBJECT_ID('NewsSitePro2025_ArticleInteractions'))
    CREATE NONCLUSTERED INDEX IX_NewsSitePro2025_ArticleInteractions_Type ON NewsSitePro2025_ArticleInteractions (InteractionType);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_NewsSitePro2025_ArticleInteractions_Timestamp' AND object_id = OBJECT_ID('NewsSitePro2025_ArticleInteractions'))
    CREATE NONCLUSTERED INDEX IX_NewsSitePro2025_ArticleInteractions_Timestamp ON NewsSitePro2025_ArticleInteractions (Timestamp);
GO

-- Sample data for testing (optional)
-- Insert default feed configurations for existing users
INSERT INTO NewsSitePro2025_FeedConfigurations (UserID, PersonalizationWeight, FreshnessWeight, PopularityWeight, SerendipityWeight)
SELECT ID, 0.4, 0.3, 0.2, 0.1
FROM Users_News 
WHERE ID NOT IN (SELECT UserID FROM NewsSitePro2025_FeedConfigurations);
GO

PRINT 'NewsSitePro2025 Recommendation system tables created successfully!';
