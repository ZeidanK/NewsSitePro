-- Sample Posts for Testing NewsSitePro
-- Run this in your SQL Server database to add some test posts

USE [NewsSitePro2025]
GO

-- Ensure we have at least one user (adjust the user ID as needed)
-- This assumes you have user(s) in your Users table

-- Check if Posts table exists, if not create it
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Posts' AND xtype='U')
BEGIN
    -- If Posts table doesn't exist, you might need to create the NewsArticles table instead
    -- Let's check for NewsArticles table
    IF EXISTS (SELECT * FROM sysobjects WHERE name='NewsArticles' AND xtype='U')
    BEGIN
        -- Insert sample posts into NewsArticles table
        INSERT INTO [dbo].[NewsArticles] (
            [Title], 
            [Content], 
            [ImageURL], 
            [PublishDate], 
            [Category], 
            [SourceURL], 
            [SourceName], 
            [UserID]
        )
        VALUES 
        ('Breaking: New Technology Revolutionizes News Industry', 
         'A groundbreaking technology has emerged that promises to transform how we consume and create news content. This innovation combines AI with traditional journalism to deliver more accurate and timely reporting.',
         'https://via.placeholder.com/600x400/1da1f2/ffffff?text=Tech+News',
         GETDATE(),
         'Technology',
         'https://example.com/tech-news',
         'Tech Daily',
         1),
         
        ('Sports Update: Championship Finals This Weekend', 
         'The highly anticipated championship finals are set to take place this weekend, with teams preparing for what promises to be an exciting showdown. Fans worldwide are eagerly awaiting the outcome.',
         'https://via.placeholder.com/600x400/28a745/ffffff?text=Sports+News',
         DATEADD(HOUR, -2, GETDATE()),
         'Sports',
         'https://example.com/sports-news',
         'Sports Central',
         1),
         
        ('Health Alert: New Guidelines for Mental Wellness', 
         'Health experts have released new guidelines focusing on mental wellness in the digital age. The recommendations include practical steps for maintaining psychological health in our increasingly connected world.',
         'https://via.placeholder.com/600x400/fd7e14/ffffff?text=Health+News',
         DATEADD(HOUR, -4, GETDATE()),
         'Health',
         'https://example.com/health-news',
         'Health Today',
         1),
         
        ('Political Development: New Policy Announcements', 
         'Government officials have announced several new policy initiatives aimed at addressing current economic challenges. The proposals are expected to impact various sectors significantly.',
         'https://via.placeholder.com/600x400/dc3545/ffffff?text=Politics+News',
         DATEADD(HOUR, -6, GETDATE()),
         'Politics',
         'https://example.com/political-news',
         'Political Times',
         1),
         
        ('Entertainment Buzz: Film Festival Highlights', 
         'The annual film festival concluded with spectacular premieres and award ceremonies. Critics and audiences alike have praised this year''s diverse selection of independent and mainstream films.',
         'https://via.placeholder.com/600x400/6f42c1/ffffff?text=Entertainment',
         DATEADD(HOUR, -8, GETDATE()),
         'Entertainment',
         'https://example.com/entertainment-news',
         'Entertainment Weekly',
         1),
         
        ('Business News: Market Trends and Analysis', 
         'Financial analysts report significant shifts in market trends this quarter. The changes reflect evolving consumer preferences and emerging economic patterns globally.',
         'https://via.placeholder.com/600x400/17a2b8/ffffff?text=Business+News',
         DATEADD(HOUR, -10, GETDATE()),
         'Business',
         'https://example.com/business-news',
         'Business Journal',
         1);

        PRINT 'Sample posts added to NewsArticles table successfully!';
    END
    ELSE
    BEGIN
        PRINT 'Neither Posts nor NewsArticles table found. Please check your database schema.';
    END
END
ELSE
BEGIN
    -- Insert into Posts table if it exists
    INSERT INTO [dbo].[Posts] (
        [Title], 
        [Content], 
        [ImageURL], 
        [PublishDate], 
        [Category], 
        [SourceURL], 
        [SourceName], 
        [UserID]
    )
    VALUES 
    ('Breaking: New Technology Revolutionizes News Industry', 
     'A groundbreaking technology has emerged that promises to transform how we consume and create news content. This innovation combines AI with traditional journalism to deliver more accurate and timely reporting.',
     'https://via.placeholder.com/600x400/1da1f2/ffffff?text=Tech+News',
     GETDATE(),
     'Technology',
     'https://example.com/tech-news',
     'Tech Daily',
     1),
     
    ('Sports Update: Championship Finals This Weekend', 
     'The highly anticipated championship finals are set to take place this weekend, with teams preparing for what promises to be an exciting showdown. Fans worldwide are eagerly awaiting the outcome.',
     'https://via.placeholder.com/600x400/28a745/ffffff?text=Sports+News',
     DATEADD(HOUR, -2, GETDATE()),
     'Sports',
     'https://example.com/sports-news',
     'Sports Central',
     1),
     
    ('Health Alert: New Guidelines for Mental Wellness', 
     'Health experts have released new guidelines focusing on mental wellness in the digital age. The recommendations include practical steps for maintaining psychological health in our increasingly connected world.',
     'https://via.placeholder.com/600x400/fd7e14/ffffff?text=Health+News',
     DATEADD(HOUR, -4, GETDATE()),
     'Health',
     'https://example.com/health-news',
     'Health Today',
     1),
     
    ('Political Development: New Policy Announcements', 
     'Government officials have announced several new policy initiatives aimed at addressing current economic challenges. The proposals are expected to impact various sectors significantly.',
     'https://via.placeholder.com/600x400/dc3545/ffffff?text=Politics+News',
     DATEADD(HOUR, -6, GETDATE()),
     'Politics',
     'https://example.com/political-news',
     'Political Times',
     1),
     
    ('Entertainment Buzz: Film Festival Highlights', 
     'The annual film festival concluded with spectacular premieres and award ceremonies. Critics and audiences alike have praised this year''s diverse selection of independent and mainstream films.',
     'https://via.placeholder.com/600x400/6f42c1/ffffff?text=Entertainment',
     DATEADD(HOUR, -8, GETDATE()),
     'Entertainment',
     'https://example.com/entertainment-news',
     'Entertainment Weekly',
     1),
     
    ('Business News: Market Trends and Analysis', 
     'Financial analysts report significant shifts in market trends this quarter. The changes reflect evolving consumer preferences and emerging economic patterns globally.',
     'https://via.placeholder.com/600x400/17a2b8/ffffff?text=Business+News',
     DATEADD(HOUR, -10, GETDATE()),
     'Business',
     'https://example.com/business-news',
     'Business Journal',
     1);

    PRINT 'Sample posts added to Posts table successfully!';
END

-- Also ensure proper indexing exists
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_NewsArticles_UserID' AND object_id = OBJECT_ID('NewsArticles'))
BEGIN
    CREATE INDEX IX_NewsArticles_UserID ON NewsArticles(UserID);
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_NewsArticles_Category' AND object_id = OBJECT_ID('NewsArticles'))
BEGIN
    CREATE INDEX IX_NewsArticles_Category ON NewsArticles(Category);
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_NewsArticles_PublishDate' AND object_id = OBJECT_ID('NewsArticles'))
BEGIN
    CREATE INDEX IX_NewsArticles_PublishDate ON NewsArticles(PublishDate DESC);
END

PRINT 'Sample data and indexes setup completed!';
