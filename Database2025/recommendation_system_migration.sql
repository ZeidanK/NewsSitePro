-- =============================================
-- Comprehensive Recommendation System Database Migration
-- Execute this script in your SQL Server Management Studio
-- Following NewsSitePro2025 naming convention
-- =============================================

-- 1. User Interests Table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='NewsSitePro2025_UserInterests' AND xtype='U')
BEGIN
    CREATE TABLE NewsSitePro2025_UserInterests (
        InterestID int IDENTITY(1,1) PRIMARY KEY,
        UserID int NOT NULL,
        Category nvarchar(100) NOT NULL,
        InterestScore float NOT NULL DEFAULT 0.0,
        LastUpdated datetime NOT NULL DEFAULT GETDATE(),
        Type int NOT NULL DEFAULT 0, -- 0=Category, 1=Keyword, 2=Source, 3=Author, 4=Geographic
        Keywords nvarchar(500) NULL, -- JSON array of keywords
        InteractionCount int NOT NULL DEFAULT 0,
        CONSTRAINT FK_NewsSitePro2025_UserInterests_Users FOREIGN KEY (UserID) REFERENCES Users_News(ID),
        CONSTRAINT UQ_NewsSitePro2025_UserInterests_UserCategory UNIQUE (UserID, Category)
    );
END
GO

-- 2. Enhanced User Behavior Table
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
        -- Enhanced behavior tracking
        PreferredTimeSlots nvarchar(200) NULL, -- JSON: ["morning", "evening"]
        PreferredContentTypes nvarchar(200) NULL, -- JSON: ["long_reads", "breaking_news"]
        AverageReadingTime float NOT NULL DEFAULT 0.0, -- in seconds
        PreferredSources nvarchar(500) NULL, -- JSON array
        DevicePreferences nvarchar(100) NULL, -- mobile, desktop, tablet
        GeographicPreferences nvarchar(200) NULL, -- JSON: country codes
        CONSTRAINT FK_NewsSitePro2025_UserBehavior_Users FOREIGN KEY (UserID) REFERENCES Users_News(ID)
    );
END
GO

-- 3. Article Interactions Table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='NewsSitePro2025_ArticleInteractions' AND xtype='U')
BEGIN
    CREATE TABLE NewsSitePro2025_ArticleInteractions (
        InteractionID int IDENTITY(1,1) PRIMARY KEY,
        UserID int NOT NULL,
        ArticleID int NOT NULL,
        InteractionType nvarchar(50) NOT NULL, -- 'view', 'like', 'share', 'comment', 'save'
        Timestamp datetime NOT NULL DEFAULT GETDATE(),
        ReadingProgress float NULL, -- 0-1 scale
        TimeSpentSeconds int NULL,
        DeviceType nvarchar(50) NULL,
        ReferrerSource nvarchar(100) NULL, -- how they found the article
        CONSTRAINT FK_NewsSitePro2025_ArticleInteractions_Users FOREIGN KEY (UserID) REFERENCES Users_News(ID),
        CONSTRAINT FK_NewsSitePro2025_ArticleInteractions_Articles FOREIGN KEY (ArticleID) REFERENCES NewsArticles(ArticleID)
    );
END
GO

-- 4. Feed Configurations Table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='NewsSitePro2025_FeedConfigurations' AND xtype='U')
BEGIN
    CREATE TABLE NewsSitePro2025_FeedConfigurations (
        UserID int PRIMARY KEY,
        Algorithm int NOT NULL DEFAULT 3, -- 0=Chronological, 1=Popular, 2=Personalized, 3=Balanced, 4=Trending, 5=Following
        ShowTrendingContent bit NOT NULL DEFAULT 1,
        ShowFollowedUsers bit NOT NULL DEFAULT 1,
        ShowRecommendedContent bit NOT NULL DEFAULT 1,
        PersonalizationWeight float NOT NULL DEFAULT 0.6,
        FreshnessWeight float NOT NULL DEFAULT 0.3,
        PopularityWeight float NOT NULL DEFAULT 0.1,
        SerendipityWeight float NOT NULL DEFAULT 0.1,
        MaxArticlesPerFeed int NOT NULL DEFAULT 20,
        PreferredCategories nvarchar(500) NULL, -- JSON array
        ExcludedCategories nvarchar(500) NULL, -- JSON array
        BlockedSources nvarchar(500) NULL, -- JSON array
        BlockedUsers nvarchar(500) NULL, -- JSON array of user IDs
        EnableSerendipity bit NOT NULL DEFAULT 1,
        LastUpdated datetime NOT NULL DEFAULT GETDATE(),
        CONSTRAINT FK_NewsSitePro2025_FeedConfigurations_Users FOREIGN KEY (UserID) REFERENCES Users_News(ID)
    );
END
GO

-- 5. Trending Topics Table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='NewsSitePro2025_TrendingTopics' AND xtype='U')
BEGIN
    CREATE TABLE NewsSitePro2025_TrendingTopics (
        TrendID int IDENTITY(1,1) PRIMARY KEY,
        Topic nvarchar(200) NOT NULL,
        Category nvarchar(100) NOT NULL,
        TrendScore float NOT NULL DEFAULT 0.0,
        TotalInteractions int NOT NULL DEFAULT 0,
        LastUpdated datetime NOT NULL DEFAULT GETDATE(),
        RelatedKeywords nvarchar(500) NULL, -- JSON array
        GeographicRegions nvarchar(500) NULL, -- JSON array of regions where trending
        INDEX IX_NewsSitePro2025_TrendingTopics_Score (TrendScore DESC),
        INDEX IX_NewsSitePro2025_TrendingTopics_Updated (LastUpdated DESC)
    );
END
GO

-- 6. User Follow Relationships Table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='NewsSitePro2025_UserFollows' AND xtype='U')
BEGIN
    CREATE TABLE NewsSitePro2025_UserFollows (
        FollowerUserID int NOT NULL,
        FollowedUserID int NOT NULL,
        FollowDate datetime NOT NULL DEFAULT GETDATE(),
        NotificationsEnabled bit NOT NULL DEFAULT 1,
        PRIMARY KEY (FollowerUserID, FollowedUserID),
        CONSTRAINT FK_NewsSitePro2025_UserFollows_Follower FOREIGN KEY (FollowerUserID) REFERENCES Users_News(ID),
        CONSTRAINT FK_NewsSitePro2025_UserFollows_Followed FOREIGN KEY (FollowedUserID) REFERENCES Users_News(ID),
        CONSTRAINT CHK_NewsSitePro2025_UserFollows_NotSelf CHECK (FollowerUserID != FollowedUserID)
    );
END
GO

-- 7. Article Similarity Table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='NewsSitePro2025_ArticleSimilarity' AND xtype='U')
BEGIN
    CREATE TABLE NewsSitePro2025_ArticleSimilarity (
        ArticleID1 int NOT NULL,
        ArticleID2 int NOT NULL,
        SimilarityScore float NOT NULL, -- 0-1
        SimilarityType nvarchar(50) NOT NULL DEFAULT 'content', -- content, category, keywords
        ComputedAt datetime NOT NULL DEFAULT GETDATE(),
        PRIMARY KEY (ArticleID1, ArticleID2),
        CONSTRAINT FK_NewsSitePro2025_ArticleSimilarity_Article1 FOREIGN KEY (ArticleID1) REFERENCES NewsArticles(ArticleID),
        CONSTRAINT FK_NewsSitePro2025_ArticleSimilarity_Article2 FOREIGN KEY (ArticleID2) REFERENCES NewsArticles(ArticleID),
        CONSTRAINT CHK_NewsSitePro2025_ArticleSimilarity_Order CHECK (ArticleID1 < ArticleID2)
    );
END
GO

-- 8. Content Quality Metrics Table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='NewsSitePro2025_ContentQuality' AND xtype='U')
BEGIN
    CREATE TABLE NewsSitePro2025_ContentQuality (
        ArticleID int PRIMARY KEY,
        QualityScore float NOT NULL DEFAULT 0.0, -- 0-1 based on various factors
        EngagementRate float NOT NULL DEFAULT 0.0, -- likes+comments+shares/views
        CompletionRate float NOT NULL DEFAULT 0.0, -- how many read to end
        ShareabilityScore float NOT NULL DEFAULT 0.0, -- how often it's shared
        IsFakeNews bit NOT NULL DEFAULT 0, -- Moderation flag
        IsSpam bit NOT NULL DEFAULT 0,
        ReportCount int NOT NULL DEFAULT 0,
        LastQualityCheck datetime NOT NULL DEFAULT GETDATE(),
        CONSTRAINT FK_NewsSitePro2025_ContentQuality_Article FOREIGN KEY (ArticleID) REFERENCES NewsArticles(ArticleID)
    );
END
GO

-- 9. Geographic Interest Tracking Table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='NewsSitePro2025_GeographicInterests' AND xtype='U')
BEGIN
    CREATE TABLE NewsSitePro2025_GeographicInterests (
        UserID int NOT NULL,
        CountryCode nvarchar(10) NOT NULL,
        RegionCode nvarchar(10) NULL,
        CityName nvarchar(100) NULL,
        InterestScore float NOT NULL DEFAULT 0.0,
        LastUpdated datetime NOT NULL DEFAULT GETDATE(),
        PRIMARY KEY (UserID, CountryCode),
        CONSTRAINT FK_NewsSitePro2025_GeographicInterests_Users FOREIGN KEY (UserID) REFERENCES Users_News(ID)
    );
END
GO

-- 10. User Engagement Analytics Table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='NewsSitePro2025_UserEngagementAnalytics' AND xtype='U')
BEGIN
    CREATE TABLE NewsSitePro2025_UserEngagementAnalytics (
        UserID int NOT NULL,
        AnalyticsDate date NOT NULL,
        TotalViews int NOT NULL DEFAULT 0,
        TotalLikes int NOT NULL DEFAULT 0,
        TotalShares int NOT NULL DEFAULT 0,
        TotalComments int NOT NULL DEFAULT 0,
        TotalSaves int NOT NULL DEFAULT 0,
        AverageSessionTime float NOT NULL DEFAULT 0.0,
        SessionCount int NOT NULL DEFAULT 0,
        TopCategories nvarchar(500) NULL, -- JSON array
        TopSources nvarchar(500) NULL, -- JSON array
        EngagementScore float NOT NULL DEFAULT 0.0, -- Overall engagement metric
        PRIMARY KEY (UserID, AnalyticsDate),
        CONSTRAINT FK_NewsSitePro2025_UserEngagementAnalytics_Users FOREIGN KEY (UserID) REFERENCES Users_News(ID)
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

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_NewsSitePro2025_UserFollows_Follower' AND object_id = OBJECT_ID('NewsSitePro2025_UserFollows'))
    CREATE NONCLUSTERED INDEX IX_NewsSitePro2025_UserFollows_Follower ON NewsSitePro2025_UserFollows (FollowerUserID);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_NewsSitePro2025_UserFollows_Followed' AND object_id = OBJECT_ID('NewsSitePro2025_UserFollows'))
    CREATE NONCLUSTERED INDEX IX_NewsSitePro2025_UserFollows_Followed ON NewsSitePro2025_UserFollows (FollowedUserID);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_NewsSitePro2025_ArticleSimilarity_Score' AND object_id = OBJECT_ID('NewsSitePro2025_ArticleSimilarity'))
    CREATE NONCLUSTERED INDEX IX_NewsSitePro2025_ArticleSimilarity_Score ON NewsSitePro2025_ArticleSimilarity (SimilarityScore DESC);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_NewsSitePro2025_ContentQuality_Score' AND object_id = OBJECT_ID('NewsSitePro2025_ContentQuality'))
    CREATE NONCLUSTERED INDEX IX_NewsSitePro2025_ContentQuality_Score ON NewsSitePro2025_ContentQuality (QualityScore DESC);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_NewsSitePro2025_UserEngagementAnalytics_Date' AND object_id = OBJECT_ID('NewsSitePro2025_UserEngagementAnalytics'))
    CREATE NONCLUSTERED INDEX IX_NewsSitePro2025_UserEngagementAnalytics_Date ON NewsSitePro2025_UserEngagementAnalytics (AnalyticsDate);
GO

-- Sample data for testing (optional)
-- Insert default feed configurations for existing users
INSERT INTO NewsSitePro2025_FeedConfigurations (UserID, PersonalizationWeight, FreshnessWeight, PopularityWeight, SerendipityWeight)
SELECT ID, 0.4, 0.3, 0.2, 0.1
FROM Users_News 
WHERE ID NOT IN (SELECT UserID FROM NewsSitePro2025_FeedConfigurations);

-- Initialize user behavior for existing users
INSERT INTO NewsSitePro2025_UserBehavior (UserID)
SELECT ID
FROM Users_News 
WHERE ID NOT IN (SELECT UserID FROM NewsSitePro2025_UserBehavior);

-- Initialize content quality metrics for existing articles
INSERT INTO NewsSitePro2025_ContentQuality (ArticleID, QualityScore)
SELECT ArticleID, 0.5 -- Start with neutral quality score
FROM NewsArticles 
WHERE ArticleID NOT IN (SELECT ArticleID FROM NewsSitePro2025_ContentQuality);

-- Add some sample trending topics
INSERT INTO NewsSitePro2025_TrendingTopics (Topic, Category, TrendScore, TotalInteractions)
VALUES 
    ('Artificial Intelligence', 'Technology', 85.5, 1250),
    ('Climate Change', 'Environment', 78.2, 980),
    ('Space Exploration', 'Science', 72.8, 750),
    ('Renewable Energy', 'Environment', 69.4, 650),
    ('Cybersecurity', 'Technology', 66.1, 580);

-- Add some sample user interests for demonstration
-- This would typically be populated through user interactions
DECLARE @SampleUserID int;
SELECT TOP 1 @SampleUserID = ID FROM Users_News;

IF @SampleUserID IS NOT NULL
BEGIN
    INSERT INTO NewsSitePro2025_UserInterests (UserID, Category, InterestScore, Type)
    VALUES 
        (@SampleUserID, 'Technology', 0.85, 0),
        (@SampleUserID, 'Science', 0.72, 0),
        (@SampleUserID, 'Business', 0.45, 0);
END
GO

PRINT 'NewsSitePro2025 Recommendation System Migration Completed Successfully!';
PRINT 'Total Tables Created: 10';
PRINT 'Total Indexes Created: 12';
PRINT 'Sample Data Inserted for Testing';
PRINT '';
PRINT 'Tables Created:';
PRINT '- NewsSitePro2025_UserInterests';
PRINT '- NewsSitePro2025_UserBehavior';
PRINT '- NewsSitePro2025_ArticleInteractions';
PRINT '- NewsSitePro2025_FeedConfigurations';
PRINT '- NewsSitePro2025_TrendingTopics';
PRINT '- NewsSitePro2025_UserFollows';
PRINT '- NewsSitePro2025_ArticleSimilarity';
PRINT '- NewsSitePro2025_ContentQuality';
PRINT '- NewsSitePro2025_GeographicInterests';
PRINT '- NewsSitePro2025_UserEngagementAnalytics';

-- Insert default feed configurations for existing users
INSERT INTO FeedConfigurations (UserID, PersonalizationWeight, FreshnessWeight, PopularityWeight, SerendipityWeight)
SELECT ID, 0.6, 0.3, 0.1, 0.1
FROM Users_News 
WHERE ID NOT IN (SELECT UserID FROM FeedConfigurations WHERE FeedConfigurations.UserID IS NOT NULL);
GO

-- Insert sample trending topics
IF NOT EXISTS (SELECT 1 FROM TrendingTopics)
BEGIN
    INSERT INTO TrendingTopics (Topic, Category, TrendScore, TotalInteractions) VALUES
    ('Technology Innovations', 'Technology', 95.5, 1250),
    ('Climate Change Updates', 'Environment', 88.2, 980),
    ('Sports Championships', 'Sports', 82.7, 750),
    ('Economic Trends', 'Business', 79.1, 650),
    ('Health & Wellness', 'Health', 76.8, 580),
    ('Entertainment News', 'Entertainment', 73.2, 520),
    ('Political Developments', 'Politics', 71.5, 490),
    ('Scientific Discoveries', 'Science', 69.8, 460),
    ('Travel & Tourism', 'Travel', 67.3, 420),
    ('Education Reform', 'Education', 64.9, 380);
END
GO

PRINT 'Comprehensive Recommendation System tables created successfully!';
PRINT 'Tables created: UserInterests, UserBehavior, ArticleInteractions, FeedConfigurations, TrendingTopics, UserFollows, ArticleSimilarity, ContentQuality, GeographicInterests, UserEngagementAnalytics';
PRINT 'Indexes created for optimal performance';
PRINT 'Sample data inserted for trending topics and default feed configurations';
