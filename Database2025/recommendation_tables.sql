-- =============================================
-- Recommendation System Database Tables (FIXED)
-- Following NewsSitePro2025 naming convention
-- =============================================

-- First, let's check the actual structure of Users_News table
-- Run this query first to identify the correct primary key column:
-- SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Users_News' AND COLUMNPROPERTY(OBJECT_ID('Users_News'), COLUMN_NAME, 'IsIdentity') = 1;

-- User Interests Table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='NewsSitePro2025_UserInterests' AND xtype='U')
BEGIN
    CREATE TABLE NewsSitePro2025_UserInterests (
        InterestID int IDENTITY(1,1) PRIMARY KEY,
        UserID int NOT NULL,
        Category nvarchar(100) NOT NULL,
        InterestScore float NOT NULL DEFAULT 0.0,
        LastUpdated datetime NOT NULL DEFAULT GETDATE(),
        -- Note: Foreign key will be added after confirming column name
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
        FavoriteCategories nvarchar(500) NULL
        -- Note: Foreign key will be added after confirming column name
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
        Timestamp datetime NOT NULL DEFAULT GETDATE()
        -- Note: Foreign keys will be added after confirming column names
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
        LastUpdated datetime NOT NULL DEFAULT GETDATE()
        -- Note: Foreign key will be added after confirming column name
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

PRINT 'NewsSitePro2025 Recommendation system tables created successfully (without foreign keys)!';
PRINT 'Next: Run the column identification query and then add foreign keys manually.';


-- Run this to find the correct primary key column in Users_News
SELECT 
    c.COLUMN_NAME,
    c.DATA_TYPE,
    CASE 
        WHEN pk.COLUMN_NAME IS NOT NULL THEN 'PRIMARY KEY'
        ELSE ''
    END AS KEY_TYPE
FROM INFORMATION_SCHEMA.COLUMNS c
LEFT JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS tc 
    ON c.TABLE_NAME = tc.TABLE_NAME
LEFT JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE pk 
    ON c.TABLE_NAME = pk.TABLE_NAME 
    AND c.COLUMN_NAME = pk.COLUMN_NAME 
    AND tc.CONSTRAINT_TYPE = 'PRIMARY KEY'
WHERE c.TABLE_NAME = 'Users_News'
ORDER BY c.ORDINAL_POSITION;

-- Also check NewsArticles table structure
SELECT 
    c.COLUMN_NAME,
    c.DATA_TYPE,
    CASE 
        WHEN pk.COLUMN_NAME IS NOT NULL THEN 'PRIMARY KEY'
        ELSE ''
    END AS KEY_TYPE
FROM INFORMATION_SCHEMA.COLUMNS c
LEFT JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS tc 
    ON c.TABLE_NAME = tc.TABLE_NAME
LEFT JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE pk 
    ON c.TABLE_NAME = pk.TABLE_NAME 
    AND c.COLUMN_NAME = pk.COLUMN_NAME 
    AND tc.CONSTRAINT_TYPE = 'PRIMARY KEY'
WHERE c.TABLE_NAME = 'NewsArticles'
ORDER BY c.ORDINAL_POSITION;