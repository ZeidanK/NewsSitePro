-- =============================================
-- NewsSitePro2025: Database Setup Script
-- =============================================
-- Run this script to create all necessary tables and initial data

USE [NewsSitePro2025]
GO

-- Create Users table if it doesn't exist
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='NewsSitePro2025_Users' AND xtype='U')
BEGIN
    CREATE TABLE [dbo].[NewsSitePro2025_Users] (
        [UserID] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [Username] NVARCHAR(100) NOT NULL UNIQUE,
        [Email] NVARCHAR(100) NOT NULL UNIQUE,
        [PasswordHash] NVARCHAR(200) NOT NULL,
        [IsAdmin] BIT NOT NULL DEFAULT 0,
        [IsLocked] BIT NOT NULL DEFAULT 0,
        [Bio] NVARCHAR(500) NULL,
        [ProfilePicture] NVARCHAR(255) NULL,
        [JoinDate] DATETIME2(7) NOT NULL DEFAULT GETDATE(),
        [LastUpdated] DATETIME2(7) NOT NULL DEFAULT GETDATE()
    );
    
    PRINT 'NewsSitePro2025_Users table created successfully!';
END
ELSE
BEGIN
    PRINT 'NewsSitePro2025_Users table already exists.';
END
GO

-- Create NewsArticles table if it doesn't exist
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='NewsSitePro2025_NewsArticles' AND xtype='U')
BEGIN
    CREATE TABLE [dbo].[NewsSitePro2025_NewsArticles] (
        [ArticleID] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [Title] NVARCHAR(100) NOT NULL,
        [Content] NVARCHAR(4000) NOT NULL,
        [ImageURL] NVARCHAR(255) NULL,
        [SourceURL] NVARCHAR(500) NULL,
        [SourceName] NVARCHAR(100) NULL,
        [Category] NVARCHAR(50) NOT NULL,
        [UserID] INT NOT NULL,
        [PublishDate] DATETIME2(7) NOT NULL DEFAULT GETDATE(),
        
        CONSTRAINT [FK_NewsArticles_Users] FOREIGN KEY ([UserID]) 
            REFERENCES [dbo].[NewsSitePro2025_Users] ([UserID]) ON DELETE NO ACTION
    );
    
    -- Create indexes for better performance
    CREATE INDEX [IX_NewsArticles_UserID] ON [dbo].[NewsSitePro2025_NewsArticles] ([UserID]);
    CREATE INDEX [IX_NewsArticles_Category] ON [dbo].[NewsSitePro2025_NewsArticles] ([Category]);
    CREATE INDEX [IX_NewsArticles_PublishDate] ON [dbo].[NewsSitePro2025_NewsArticles] ([PublishDate] DESC);
    
    PRINT 'NewsSitePro2025_NewsArticles table created successfully!';
END
ELSE
BEGIN
    PRINT 'NewsSitePro2025_NewsArticles table already exists.';
END
GO

-- Create ArticleLikes table if it doesn't exist
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='NewsSitePro2025_ArticleLikes' AND xtype='U')
BEGIN
    CREATE TABLE [dbo].[NewsSitePro2025_ArticleLikes] (
        [LikeID] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [ArticleID] INT NOT NULL,
        [UserID] INT NOT NULL,
        [LikeDate] DATETIME2(7) NOT NULL DEFAULT GETDATE(),
        
        CONSTRAINT [FK_ArticleLikes_Articles] FOREIGN KEY ([ArticleID]) 
            REFERENCES [dbo].[NewsSitePro2025_NewsArticles] ([ArticleID]) ON DELETE CASCADE,
        CONSTRAINT [FK_ArticleLikes_Users] FOREIGN KEY ([UserID]) 
            REFERENCES [dbo].[NewsSitePro2025_Users] ([UserID]) ON DELETE NO ACTION,
        CONSTRAINT [UK_ArticleLikes_User_Article] UNIQUE ([ArticleID], [UserID])
    );
    
    CREATE INDEX [IX_ArticleLikes_ArticleID] ON [dbo].[NewsSitePro2025_ArticleLikes] ([ArticleID]);
    CREATE INDEX [IX_ArticleLikes_UserID] ON [dbo].[NewsSitePro2025_ArticleLikes] ([UserID]);
    
    PRINT 'NewsSitePro2025_ArticleLikes table created successfully!';
END
ELSE
BEGIN
    PRINT 'NewsSitePro2025_ArticleLikes table already exists.';
END
GO

-- Create SavedArticles table if it doesn't exist
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='NewsSitePro2025_SavedArticles' AND xtype='U')
BEGIN
    CREATE TABLE [dbo].[NewsSitePro2025_SavedArticles] (
        [SaveID] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [ArticleID] INT NOT NULL,
        [UserID] INT NOT NULL,
        [SaveDate] DATETIME2(7) NOT NULL DEFAULT GETDATE(),
        
        CONSTRAINT [FK_SavedArticles_Articles] FOREIGN KEY ([ArticleID]) 
            REFERENCES [dbo].[NewsSitePro2025_NewsArticles] ([ArticleID]) ON DELETE CASCADE,
        CONSTRAINT [FK_SavedArticles_Users] FOREIGN KEY ([UserID]) 
            REFERENCES [dbo].[NewsSitePro2025_Users] ([UserID]) ON DELETE NO ACTION,
        CONSTRAINT [UK_SavedArticles_User_Article] UNIQUE ([ArticleID], [UserID])
    );
    
    CREATE INDEX [IX_SavedArticles_ArticleID] ON [dbo].[NewsSitePro2025_SavedArticles] ([ArticleID]);
    CREATE INDEX [IX_SavedArticles_UserID] ON [dbo].[NewsSitePro2025_SavedArticles] ([UserID]);
    
    PRINT 'NewsSitePro2025_SavedArticles table created successfully!';
END
ELSE
BEGIN
    PRINT 'NewsSitePro2025_SavedArticles table already exists.';
END
GO

-- Insert a test user if none exists
IF NOT EXISTS (SELECT 1 FROM NewsSitePro2025_Users)
BEGIN
    INSERT INTO [dbo].[NewsSitePro2025_Users] (
        [Username], 
        [Email], 
        [PasswordHash], 
        [IsAdmin], 
        [IsLocked], 
        [Bio]
    )
    VALUES 
    ('admin', 'admin@newssitepro.com', 'AQAAAAEAACcQAAAAEGwrm9h3K1X8Z9z3QZ8v4f7h1n2m5k6j9s8d7a4b6c3e1f0g9h8i7j6k5l4m3n2o1p0', 1, 0, 'Administrator account'),
    ('testuser', 'user@newssitepro.com', 'AQAAAAEAACcQAAAAEGwrm9h3K1X8Z9z3QZ8v4f7h1n2m5k6j9s8d7a4b6c3e1f0g9h8i7j6k5l4m3n2o1p0', 0, 0, 'Test user account');
    
    PRINT 'Test users created successfully! (Password: password123)';
END
ELSE
BEGIN
    PRINT 'Users already exist in the database.';
END
GO

-- Insert sample articles if none exist
IF NOT EXISTS (SELECT 1 FROM NewsSitePro2025_NewsArticles)
BEGIN
    DECLARE @UserID INT = (SELECT TOP 1 UserID FROM NewsSitePro2025_Users);
    
    INSERT INTO [dbo].[NewsSitePro2025_NewsArticles] (
        [Title], 
        [Content], 
        [ImageURL], 
        [SourceURL], 
        [SourceName], 
        [Category], 
        [UserID], 
        [PublishDate]
    )
    VALUES 
    ('Breaking: New Technology Revolutionizes News Industry', 
     'A groundbreaking technology has emerged that promises to transform how we consume and create news content. This innovation combines AI with traditional journalism to deliver more accurate and timely reporting.',
     'https://via.placeholder.com/600x400/1da1f2/ffffff?text=Tech+News',
     'https://example.com/tech-news',
     'Tech Daily',
     'Technology',
     @UserID,
     GETDATE()),
     
    ('Sports Update: Championship Finals This Weekend', 
     'The highly anticipated championship finals are set to take place this weekend, with teams preparing for what promises to be an exciting showdown. Fans worldwide are eagerly awaiting the outcome.',
     'https://via.placeholder.com/600x400/28a745/ffffff?text=Sports+News',
     'https://example.com/sports-news',
     'Sports Central',
     'Sports',
     @UserID,
     DATEADD(HOUR, -2, GETDATE())),
     
    ('Health Alert: New Guidelines for Mental Wellness', 
     'Health experts have released new guidelines focusing on mental wellness in the digital age. The recommendations include practical steps for maintaining psychological health in our increasingly connected world.',
     'https://via.placeholder.com/600x400/fd7e14/ffffff?text=Health+News',
     'https://example.com/health-news',
     'Health Today',
     'Health',
     @UserID,
     DATEADD(HOUR, -4, GETDATE())),
     
    ('Political Development: New Policy Announcements', 
     'Government officials have announced several new policy initiatives aimed at addressing current economic challenges. The proposals are expected to impact various sectors significantly.',
     'https://via.placeholder.com/600x400/dc3545/ffffff?text=Politics+News',
     'https://example.com/political-news',
     'Political Times',
     'Politics',
     @UserID,
     DATEADD(HOUR, -6, GETDATE())),
     
    ('Entertainment Buzz: Film Festival Highlights', 
     'The annual film festival concluded with spectacular premieres and award ceremonies. Critics and audiences alike have praised this year''s diverse selection of independent and mainstream films.',
     'https://via.placeholder.com/600x400/6f42c1/ffffff?text=Entertainment',
     'https://example.com/entertainment-news',
     'Entertainment Weekly',
     'Entertainment',
     @UserID,
     DATEADD(HOUR, -8, GETDATE())),
     
    ('Business News: Market Trends and Analysis', 
     'Financial analysts report significant shifts in market trends this quarter. The changes reflect evolving consumer preferences and emerging economic patterns globally.',
     'https://via.placeholder.com/600x400/17a2b8/ffffff?text=Business+News',
     'https://example.com/business-news',
     'Business Journal',
     'Business',
     @UserID,
     DATEADD(HOUR, -10, GETDATE()));

    PRINT 'Sample articles created successfully!';
END
ELSE
BEGIN
    PRINT 'Articles already exist in the database.';
END
GO

PRINT 'Database setup completed successfully!'
PRINT 'You can now run your NewsSitePro application.'
PRINT ''
PRINT 'Test Login Credentials:'
PRINT 'Email: admin@newssitepro.com'
PRINT 'Password: password123'
PRINT ''
PRINT 'Email: user@newssitepro.com'  
PRINT 'Password: password123'
