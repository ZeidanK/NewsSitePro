-- Admin tables and updates for the News Site database

-- Create ActivityLogs table for tracking user actions
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ActivityLogs')
BEGIN
    CREATE TABLE ActivityLogs (
        ID int IDENTITY(1,1) PRIMARY KEY,
        UserID int NOT NULL,
        Action nvarchar(100) NOT NULL,
        Details nvarchar(500) NOT NULL,
        Timestamp datetime NOT NULL DEFAULT GETDATE(),
        IpAddress nvarchar(50) NOT NULL,
        UserAgent nvarchar(500) NOT NULL,
        FOREIGN KEY (UserID) REFERENCES Users_News(ID)
    );
END

-- Create Reports table for user reporting system
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Reports')
BEGIN
    CREATE TABLE Reports (
        ID int IDENTITY(1,1) PRIMARY KEY,
        ReporterID int NOT NULL,
        ReportedUserID int NOT NULL,
        Reason nvarchar(100) NOT NULL,
        Description nvarchar(1000) NOT NULL,
        CreatedAt datetime NOT NULL DEFAULT GETDATE(),
        Status nvarchar(20) NOT NULL DEFAULT 'Pending',
        ResolvedBy int NULL,
        ResolvedAt datetime NULL,
        ResolutionNotes nvarchar(500) NULL,
        FOREIGN KEY (ReporterID) REFERENCES Users_News(ID),
        FOREIGN KEY (ReportedUserID) REFERENCES Users_News(ID),
        FOREIGN KEY (ResolvedBy) REFERENCES Users_News(ID)
    );
END

-- Add admin and ban related columns to Users_News if they don't exist
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users_News') AND name = 'IsAdmin')
BEGIN
    ALTER TABLE Users_News ADD IsAdmin bit NOT NULL DEFAULT 0;
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users_News') AND name = 'IsActive')
BEGIN
    ALTER TABLE Users_News ADD IsActive bit NOT NULL DEFAULT 1;
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users_News') AND name = 'IsBanned')
BEGIN
    ALTER TABLE Users_News ADD IsBanned bit NOT NULL DEFAULT 0;
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users_News') AND name = 'BannedUntil')
BEGIN
    ALTER TABLE Users_News ADD BannedUntil datetime NULL;
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users_News') AND name = 'BanReason')
BEGIN
    ALTER TABLE Users_News ADD BanReason nvarchar(500) NULL;
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users_News') AND name = 'LastActivity')
BEGIN
    ALTER TABLE Users_News ADD LastActivity datetime NULL;
END

-- Create indexes for better performance
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID('ActivityLogs') AND name = 'IX_ActivityLogs_UserID_Timestamp')
BEGIN
    CREATE INDEX IX_ActivityLogs_UserID_Timestamp ON ActivityLogs (UserID, Timestamp DESC);
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID('Reports') AND name = 'IX_Reports_Status_CreatedAt')
BEGIN
    CREATE INDEX IX_Reports_Status_CreatedAt ON Reports (Status, CreatedAt DESC);
END

-- Create stored procedures for admin operations

-- Get admin dashboard statistics
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_GetAdminDashboardStats')
    DROP PROCEDURE sp_GetAdminDashboardStats
GO

CREATE PROCEDURE sp_GetAdminDashboardStats
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        (SELECT COUNT(*) FROM Users_News) AS TotalUsers,
        (SELECT COUNT(*) FROM Users_News WHERE IsActive = 1 AND (IsBanned = 0 OR BannedUntil < GETDATE())) AS ActiveUsers,
        (SELECT COUNT(*) FROM Users_News WHERE IsBanned = 1 AND (BannedUntil IS NULL OR BannedUntil > GETDATE())) AS BannedUsers,
        (SELECT COUNT(*) FROM NewsArticles) AS TotalPosts,
        (SELECT COUNT(*) FROM Reports) AS TotalReports,
        (SELECT COUNT(*) FROM Reports WHERE Status = 'Pending') AS PendingReports,
        (SELECT COUNT(*) FROM Users_News WHERE CAST(JoinDate AS DATE) = CAST(GETDATE() AS DATE)) AS TodayRegistrations,
        (SELECT COUNT(*) FROM NewsArticles WHERE CAST(CreatedAt AS DATE) = CAST(GETDATE() AS DATE)) AS TodayPosts
END
GO

-- Get users for admin with pagination and filtering
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_GetUsersForAdmin')
    DROP PROCEDURE sp_GetUsersForAdmin
GO

CREATE PROCEDURE sp_GetUsersForAdmin
    @Page int = 1,
    @PageSize int = 50,
    @Search nvarchar(100) = '',
    @Status nvarchar(20) = '',
    @JoinDateFilter nvarchar(20) = ''
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Offset int = (@Page - 1) * @PageSize;
    DECLARE @SQL nvarchar(4000);
    
    SET @SQL = '
    SELECT u.ID, u.Name AS Username, u.Email, u.Bio, u.JoinDate, u.LastActivity, 
           u.IsAdmin, u.IsActive, u.IsBanned, u.BannedUntil,
           COALESCE(pc.PostCount, 0) AS PostCount,
           COALESCE(lc.LikesReceived, 0) AS LikesReceived
    FROM Users_News u
    LEFT JOIN (
        SELECT UserID, COUNT(*) AS PostCount 
        FROM NewsArticles 
        GROUP BY UserID
    ) pc ON u.ID = pc.UserID
    LEFT JOIN (
        SELECT na.UserID, COUNT(*) AS LikesReceived
        FROM NewsArticles na
        INNER JOIN ArticleLikes al ON na.ID = al.ArticleID
        GROUP BY na.UserID
    ) lc ON u.ID = lc.UserID
    WHERE 1=1';
    
    -- Add search filter
    IF @Search != ''
    BEGIN
        SET @SQL = @SQL + ' AND (u.Name LIKE ''%' + @Search + '%'' OR u.Email LIKE ''%' + @Search + '%'')';
    END
    
    -- Add status filter
    IF @Status = 'active'
    BEGIN
        SET @SQL = @SQL + ' AND u.IsActive = 1 AND (u.IsBanned = 0 OR u.BannedUntil < GETDATE())';
    END
    ELSE IF @Status = 'banned'
    BEGIN
        SET @SQL = @SQL + ' AND u.IsBanned = 1 AND (u.BannedUntil IS NULL OR u.BannedUntil > GETDATE())';
    END
    ELSE IF @Status = 'inactive'
    BEGIN
        SET @SQL = @SQL + ' AND u.IsActive = 0';
    END
    
    -- Add join date filter
    IF @JoinDateFilter = 'today'
    BEGIN
        SET @SQL = @SQL + ' AND CAST(u.JoinDate AS DATE) = CAST(GETDATE() AS DATE)';
    END
    ELSE IF @JoinDateFilter = 'week'
    BEGIN
        SET @SQL = @SQL + ' AND u.JoinDate >= DATEADD(week, -1, GETDATE())';
    END
    ELSE IF @JoinDateFilter = 'month'
    BEGIN
        SET @SQL = @SQL + ' AND u.JoinDate >= DATEADD(month, -1, GETDATE())';
    END
    ELSE IF @JoinDateFilter = 'year'
    BEGIN
        SET @SQL = @SQL + ' AND u.JoinDate >= DATEADD(year, -1, GETDATE())';
    END
    
    SET @SQL = @SQL + '
    ORDER BY u.JoinDate DESC
    OFFSET ' + CAST(@Offset AS nvarchar(10)) + ' ROWS 
    FETCH NEXT ' + CAST(@PageSize AS nvarchar(10)) + ' ROWS ONLY';
    
    EXEC sp_executesql @SQL;
END
GO

-- Ban user procedure
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_BanUser')
    DROP PROCEDURE sp_BanUser
GO

CREATE PROCEDURE sp_BanUser
    @UserID int,
    @Reason nvarchar(500),
    @DurationDays int, -- -1 for permanent
    @AdminID int
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @BannedUntil datetime = NULL;
    
    IF @DurationDays != -1
    BEGIN
        SET @BannedUntil = DATEADD(day, @DurationDays, GETDATE());
    END
    
    UPDATE Users_News 
    SET IsBanned = 1, 
        BannedUntil = @BannedUntil, 
        BanReason = @Reason
    WHERE ID = @UserID;
    
    -- Log the action
    INSERT INTO ActivityLogs (UserID, Action, Details, IpAddress, UserAgent)
    VALUES (@AdminID, 'BAN_USER', 'Banned user ' + CAST(@UserID AS nvarchar(10)) + ' for ' + CAST(@DurationDays AS nvarchar(10)) + ' days. Reason: ' + @Reason, 'Admin Panel', 'Admin Action');
    
    SELECT @@ROWCOUNT AS RowsAffected;
END
GO

-- Unban user procedure
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_UnbanUser')
    DROP PROCEDURE sp_UnbanUser
GO

CREATE PROCEDURE sp_UnbanUser
    @UserID int,
    @AdminID int
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE Users_News 
    SET IsBanned = 0, 
        BannedUntil = NULL, 
        BanReason = NULL
    WHERE ID = @UserID;
    
    -- Log the action
    INSERT INTO ActivityLogs (UserID, Action, Details, IpAddress, UserAgent)
    VALUES (@AdminID, 'UNBAN_USER', 'Unbanned user ' + CAST(@UserID AS nvarchar(10)), 'Admin Panel', 'Admin Action');
    
    SELECT @@ROWCOUNT AS RowsAffected;
END
GO

-- Get activity logs
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_GetActivityLogs')
    DROP PROCEDURE sp_GetActivityLogs
GO

CREATE PROCEDURE sp_GetActivityLogs
    @Page int = 1,
    @PageSize int = 20
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Offset int = (@Page - 1) * @PageSize;
    
    SELECT al.ID, al.UserID, u.Name AS Username, al.Action, 
           al.Details, al.Timestamp, al.IpAddress, al.UserAgent
    FROM ActivityLogs al
    INNER JOIN Users_News u ON al.UserID = u.ID
    ORDER BY al.Timestamp DESC
    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
END
GO

-- Get reports
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_GetReports')
    DROP PROCEDURE sp_GetReports
GO

CREATE PROCEDURE sp_GetReports
    @Status nvarchar(20) = 'ALL'
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT r.ID, r.ReporterID, ru.Name AS ReporterUsername, 
           r.ReportedUserID, rpu.Name AS ReportedUsername,
           r.Reason, r.Description, r.CreatedAt, r.Status,
           r.ResolvedBy, r.ResolvedAt, r.ResolutionNotes
    FROM Reports r
    INNER JOIN Users_News ru ON r.ReporterID = ru.ID
    INNER JOIN Users_News rpu ON r.ReportedUserID = rpu.ID
    WHERE (@Status = 'ALL' OR r.Status = @Status)
    ORDER BY r.CreatedAt DESC;
END
GO

-- Resolve report procedure
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_ResolveReport')
    DROP PROCEDURE sp_ResolveReport
GO

CREATE PROCEDURE sp_ResolveReport
    @ReportID int,
    @Action nvarchar(20),
    @Notes nvarchar(500),
    @AdminID int
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE Reports 
    SET Status = @Action, 
        ResolvedBy = @AdminID, 
        ResolvedAt = GETDATE(), 
        ResolutionNotes = @Notes
    WHERE ID = @ReportID;
    
    -- Log the action
    INSERT INTO ActivityLogs (UserID, Action, Details, IpAddress, UserAgent)
    VALUES (@AdminID, 'RESOLVE_REPORT', 'Resolved report ' + CAST(@ReportID AS nvarchar(10)) + ' with action: ' + @Action, 'Admin Panel', 'Admin Action');
    
    SELECT @@ROWCOUNT AS RowsAffected;
END
GO

-- Create first admin user if none exists
IF NOT EXISTS (SELECT * FROM Users_News WHERE IsAdmin = 1)
BEGIN
    -- Update the first user to be an admin (you can change this ID as needed)
    UPDATE Users_News 
    SET IsAdmin = 1 
    WHERE ID = (SELECT TOP 1 ID FROM Users_News ORDER BY ID);
    
    PRINT 'First user has been made an admin.';
END

PRINT 'Admin panel database setup completed successfully!';
