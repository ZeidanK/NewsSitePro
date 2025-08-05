-- AdminNews System Database Improvements
-- This script adds the necessary tables and procedures to fix the current AdminNews issues

USE [myProjDB]
GO

-- 1. Add ArticleWorkflow table for proper article state management
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='ArticleWorkflow' AND xtype='U')
BEGIN
    PRINT 'Creating ArticleWorkflow table...'
    
    CREATE TABLE ArticleWorkflow (
        WorkflowID INT IDENTITY(1,1) PRIMARY KEY,
        ArticleData NVARCHAR(MAX) NOT NULL, -- JSON storage for external articles
        Status NVARCHAR(50) NOT NULL DEFAULT 'pending', 
        -- Status values: 'pending', 'approved', 'rejected', 'published'
        SourceType NVARCHAR(50) NOT NULL, 
        -- SourceType values: 'manual', 'background', 'api-fetch', 'breaking', 'top-headlines'
        Category NVARCHAR(50) DEFAULT 'general',
        FetchedAt DATETIME2 DEFAULT GETDATE(),
        FetchedBy INT, -- Admin user ID who fetched it
        ReviewedAt DATETIME2 NULL,
        ReviewedBy INT NULL, -- Admin user ID who reviewed it
        ReviewNotes NVARCHAR(500) NULL,
        PublishedArticleID INT NULL, -- Links to NewsArticles.ArticleID when published
        
        -- Foreign key constraints
        CONSTRAINT FK_ArticleWorkflow_FetchedBy FOREIGN KEY (FetchedBy) REFERENCES Users(UserID),
        CONSTRAINT FK_ArticleWorkflow_ReviewedBy FOREIGN KEY (ReviewedBy) REFERENCES Users(UserID),
        CONSTRAINT FK_ArticleWorkflow_PublishedArticle FOREIGN KEY (PublishedArticleID) REFERENCES NewsArticles(ArticleID)
    );
    
    -- Add indexes for performance
    CREATE INDEX IX_ArticleWorkflow_Status ON ArticleWorkflow(Status);
    CREATE INDEX IX_ArticleWorkflow_FetchedAt ON ArticleWorkflow(FetchedAt DESC);
    CREATE INDEX IX_ArticleWorkflow_FetchedBy ON ArticleWorkflow(FetchedBy);
    CREATE INDEX IX_ArticleWorkflow_Category ON ArticleWorkflow(Category);
    
    PRINT 'ArticleWorkflow table created successfully'
END
ELSE
BEGIN
    PRINT 'ArticleWorkflow table already exists'
END
GO

-- 2. Add efficient statistics stored procedure
IF EXISTS (SELECT * FROM sysobjects WHERE name='sp_GetAdminNewsStats' AND type='P')
BEGIN
    DROP PROCEDURE sp_GetAdminNewsStats
    PRINT 'Dropped existing sp_GetAdminNewsStats procedure'
END
GO

CREATE PROCEDURE sp_GetAdminNewsStats
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @TotalArticles INT = 0;
    DECLARE @PublishedToday INT = 0;
    DECLARE @PendingReview INT = 0;
    DECLARE @LastBackgroundSync DATETIME2 = NULL;
    DECLARE @ApprovedCount INT = 0;
    DECLARE @RejectedCount INT = 0;
    
    -- Get total articles count
    SELECT @TotalArticles = COUNT(*) FROM NewsArticles;
    
    -- Get articles published today
    SELECT @PublishedToday = COUNT(*) 
    FROM NewsArticles 
    WHERE CAST(PublishDate AS DATE) = CAST(GETDATE() AS DATE);
    
    -- Get pending review count (if ArticleWorkflow table exists)
    IF EXISTS (SELECT * FROM sysobjects WHERE name='ArticleWorkflow' AND xtype='U')
    BEGIN
        SELECT @PendingReview = COUNT(*) FROM ArticleWorkflow WHERE Status = 'pending';
        SELECT @ApprovedCount = COUNT(*) FROM ArticleWorkflow WHERE Status = 'approved';
        SELECT @RejectedCount = COUNT(*) FROM ArticleWorkflow WHERE Status = 'rejected';
        SELECT @LastBackgroundSync = MAX(FetchedAt) FROM ArticleWorkflow WHERE SourceType = 'background';
    END
    
    -- Return all statistics in one result set
    SELECT 
        @TotalArticles AS TotalArticles,
        @PublishedToday AS PublishedToday,
        @PendingReview AS PendingReview,
        @ApprovedCount AS ApprovedCount,
        @RejectedCount AS RejectedCount,
        @LastBackgroundSync AS LastBackgroundSync;
END
GO

PRINT 'sp_GetAdminNewsStats procedure created successfully'
GO

-- 3. Add stored procedure for workflow item management
IF EXISTS (SELECT * FROM sysobjects WHERE name='sp_CreateWorkflowItem' AND type='P')
BEGIN
    DROP PROCEDURE sp_CreateWorkflowItem
END
GO

CREATE PROCEDURE sp_CreateWorkflowItem
    @ArticleData NVARCHAR(MAX),
    @SourceType NVARCHAR(50),
    @Category NVARCHAR(50) = 'general',
    @FetchedBy INT
AS
BEGIN
    SET NOCOUNT ON;
    
    INSERT INTO ArticleWorkflow (ArticleData, SourceType, Category, FetchedBy)
    VALUES (@ArticleData, @SourceType, @Category, @FetchedBy);
    
    SELECT SCOPE_IDENTITY() AS WorkflowID;
END
GO

PRINT 'sp_CreateWorkflowItem procedure created successfully'
GO

-- 4. Add stored procedure to get workflow items by status
IF EXISTS (SELECT * FROM sysobjects WHERE name='sp_GetWorkflowItemsByStatus' AND type='P')
BEGIN
    DROP PROCEDURE sp_GetWorkflowItemsByStatus
END
GO

CREATE PROCEDURE sp_GetWorkflowItemsByStatus
    @Status NVARCHAR(50),
    @Page INT = 1,
    @PageSize INT = 20
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Offset INT = (@Page - 1) * @PageSize;
    
    SELECT 
        w.WorkflowID,
        w.ArticleData,
        w.Status,
        w.SourceType,
        w.Category,
        w.FetchedAt,
        w.ReviewedAt,
        w.ReviewNotes,
        w.PublishedArticleID,
        uf.Name AS FetchedByName,
        ur.Name AS ReviewedByName
    FROM ArticleWorkflow w
    LEFT JOIN Users uf ON w.FetchedBy = uf.UserID
    LEFT JOIN Users ur ON w.ReviewedBy = ur.UserID
    WHERE w.Status = @Status
    ORDER BY w.FetchedAt DESC
    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
    
    -- Also return total count for pagination
    SELECT COUNT(*) AS TotalCount 
    FROM ArticleWorkflow 
    WHERE Status = @Status;
END
GO

PRINT 'sp_GetWorkflowItemsByStatus procedure created successfully'
GO

-- 5. Add stored procedure to update workflow item status
IF EXISTS (SELECT * FROM sysobjects WHERE name='sp_UpdateWorkflowItemStatus' AND type='P')
BEGIN
    DROP PROCEDURE sp_UpdateWorkflowItemStatus
END
GO

CREATE PROCEDURE sp_UpdateWorkflowItemStatus
    @WorkflowID INT,
    @Status NVARCHAR(50),
    @ReviewedBy INT,
    @ReviewNotes NVARCHAR(500) = NULL,
    @PublishedArticleID INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE ArticleWorkflow 
    SET 
        Status = @Status,
        ReviewedAt = GETDATE(),
        ReviewedBy = @ReviewedBy,
        ReviewNotes = @ReviewNotes,
        PublishedArticleID = @PublishedArticleID
    WHERE WorkflowID = @WorkflowID;
    
    SELECT @@ROWCOUNT AS RowsAffected;
END
GO

PRINT 'sp_UpdateWorkflowItemStatus procedure created successfully'
GO

-- 6. Add stored procedure for background service status tracking
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='SystemSettings' AND xtype='U')
BEGIN
    PRINT 'Creating SystemSettings table for configuration...'
    
    CREATE TABLE SystemSettings (
        SettingID INT IDENTITY(1,1) PRIMARY KEY,
        SettingKey NVARCHAR(100) NOT NULL UNIQUE,
        SettingValue NVARCHAR(MAX) NOT NULL,
        SettingType NVARCHAR(50) DEFAULT 'string', -- 'string', 'int', 'bool', 'json'
        Description NVARCHAR(500) NULL,
        UpdatedAt DATETIME2 DEFAULT GETDATE(),
        UpdatedBy INT NULL,
        CONSTRAINT FK_SystemSettings_UpdatedBy FOREIGN KEY (UpdatedBy) REFERENCES Users(UserID)
    );
    
    CREATE INDEX IX_SystemSettings_Key ON SystemSettings(SettingKey);
    
    -- Insert default settings
    INSERT INTO SystemSettings (SettingKey, SettingValue, SettingType, Description) VALUES
    ('BackgroundService.NewsSync.Enabled', 'false', 'bool', 'Enable/disable automatic news syncing'),
    ('BackgroundService.NewsSync.IntervalHours', '24', 'int', 'Hours between automatic news syncs'),
    ('NewsApi.QuotaLimit', '1000', 'int', 'Daily API quota limit'),
    ('AdminNews.DefaultPageSize', '20', 'int', 'Default number of articles per page'),
    ('AdminNews.AutoPublish.Enabled', 'false', 'bool', 'Enable automatic publishing of approved articles');
    
    PRINT 'SystemSettings table created with default values'
END
ELSE
BEGIN
    PRINT 'SystemSettings table already exists'
END
GO

-- 7. Add helper procedures for system settings
IF EXISTS (SELECT * FROM sysobjects WHERE name='sp_GetSystemSetting' AND type='P')
BEGIN
    DROP PROCEDURE sp_GetSystemSetting
END
GO

CREATE PROCEDURE sp_GetSystemSetting
    @SettingKey NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT SettingValue, SettingType 
    FROM SystemSettings 
    WHERE SettingKey = @SettingKey;
END
GO

IF EXISTS (SELECT * FROM sysobjects WHERE name='sp_SetSystemSetting' AND type='P')
BEGIN
    DROP PROCEDURE sp_SetSystemSetting
END
GO

CREATE PROCEDURE sp_SetSystemSetting
    @SettingKey NVARCHAR(100),
    @SettingValue NVARCHAR(MAX),
    @UpdatedBy INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    IF EXISTS (SELECT 1 FROM SystemSettings WHERE SettingKey = @SettingKey)
    BEGIN
        UPDATE SystemSettings 
        SET SettingValue = @SettingValue, 
            UpdatedAt = GETDATE(),
            UpdatedBy = @UpdatedBy
        WHERE SettingKey = @SettingKey;
    END
    ELSE
    BEGIN
        INSERT INTO SystemSettings (SettingKey, SettingValue, UpdatedBy)
        VALUES (@SettingKey, @SettingValue, @UpdatedBy);
    END
    
    SELECT @@ROWCOUNT AS RowsAffected;
END
GO

PRINT 'System settings procedures created successfully'
GO

-- 8. Create view for easy workflow monitoring
IF EXISTS (SELECT * FROM sysobjects WHERE name='vw_AdminNewsWorkflow' AND type='V')
BEGIN
    DROP VIEW vw_AdminNewsWorkflow
END
GO

CREATE VIEW vw_AdminNewsWorkflow AS
SELECT 
    w.WorkflowID,
    w.Status,
    w.SourceType,
    w.Category,
    w.FetchedAt,
    w.ReviewedAt,
    uf.Name AS FetchedBy,
    ur.Name AS ReviewedBy,
    w.ReviewNotes,
    w.PublishedArticleID,
    na.Title AS PublishedTitle,
    -- Extract article title from JSON (SQL Server 2016+)
    CASE 
        WHEN ISJSON(w.ArticleData) = 1 
        THEN JSON_VALUE(w.ArticleData, '$.title')
        ELSE 'Invalid JSON'
    END AS ArticleTitle,
    CASE 
        WHEN ISJSON(w.ArticleData) = 1 
        THEN JSON_VALUE(w.ArticleData, '$.source.name')
        ELSE 'Unknown'
    END AS SourceName
FROM ArticleWorkflow w
LEFT JOIN Users uf ON w.FetchedBy = uf.UserID
LEFT JOIN Users ur ON w.ReviewedBy = ur.UserID
LEFT JOIN NewsArticles na ON w.PublishedArticleID = na.ArticleID;
GO

PRINT 'vw_AdminNewsWorkflow view created successfully'
GO

-- 9. Add indexes to existing tables for better performance
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_NewsArticles_PublishDate')
BEGIN
    CREATE INDEX IX_NewsArticles_PublishDate ON NewsArticles(PublishDate DESC);
    PRINT 'Added index IX_NewsArticles_PublishDate'
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_NewsArticles_Category')
BEGIN
    CREATE INDEX IX_NewsArticles_Category ON NewsArticles(Category);
    PRINT 'Added index IX_NewsArticles_Category'
END

-- 10. Test the new procedures
PRINT 'Testing new stored procedures...'
GO

-- Test statistics procedure
EXEC sp_GetAdminNewsStats;
PRINT 'Statistics procedure test completed'
GO

-- Test system settings
EXEC sp_GetSystemSetting 'BackgroundService.NewsSync.Enabled';
PRINT 'System settings test completed'
GO

PRINT 'AdminNews database improvements completed successfully!'
PRINT 'You can now use the improved workflow system for better article management.'
GO
