-- =============================================
-- NewsSitePro2025: More Stored Procedures (Comments, Tags, Follows, Notifications, Reports, Admin)
-- =============================================

-- Comments CRUD
CREATE PROCEDURE NewsSitePro2025_sp_Comments_Insert
    @ArticleID INT,
    @UserID INT,
    @Content NVARCHAR(1000)
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO NewsSitePro2025_Comments (ArticleID, UserID, Content, CreatedAt)
    VALUES (@ArticleID, @UserID, @Content, GETDATE());
    SELECT SCOPE_IDENTITY() as CommentID;
END
GO

CREATE PROCEDURE NewsSitePro2025_sp_Comments_Delete
    @CommentID INT
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM NewsSitePro2025_Comments WHERE CommentID = @CommentID;
END
GO

-- Tags CRUD
CREATE PROCEDURE NewsSitePro2025_sp_Tags_Insert
    @Name NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO NewsSitePro2025_Tags (Name) VALUES (@Name);
    SELECT SCOPE_IDENTITY() as TagID;
END
GO

-- ArticleTags
CREATE PROCEDURE NewsSitePro2025_sp_ArticleTags_Add
    @ArticleID INT,
    @TagID INT
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM NewsSitePro2025_ArticleTags WHERE ArticleID = @ArticleID AND TagID = @TagID)
        INSERT INTO NewsSitePro2025_ArticleTags (ArticleID, TagID) VALUES (@ArticleID, @TagID);
END
GO

-- UserInterestTags
CREATE PROCEDURE NewsSitePro2025_sp_UserInterestTags_Add
    @UserID INT,
    @TagID INT
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM NewsSitePro2025_UserInterestTags WHERE UserID = @UserID AND TagID = @TagID)
        INSERT INTO NewsSitePro2025_UserInterestTags (UserID, TagID) VALUES (@UserID, @TagID);
END
GO

-- Follows
CREATE PROCEDURE NewsSitePro2025_sp_UserFollows_Toggle
    @FollowerUserID INT,
    @FollowedUserID INT
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (SELECT 1 FROM NewsSitePro2025_UserFollows WHERE FollowerUserID = @FollowerUserID AND FollowedUserID = @FollowedUserID)
    BEGIN
        DELETE FROM NewsSitePro2025_UserFollows WHERE FollowerUserID = @FollowerUserID AND FollowedUserID = @FollowedUserID;
        SELECT 'unfollowed' as Action;
    END
    ELSE
    BEGIN
        INSERT INTO NewsSitePro2025_UserFollows (FollowerUserID, FollowedUserID, FollowDate)
        VALUES (@FollowerUserID, @FollowedUserID, GETDATE());
        SELECT 'followed' as Action;
    END
END
GO

-- Notifications
CREATE PROCEDURE NewsSitePro2025_sp_Notifications_Insert
    @UserID INT,
    @Type NVARCHAR(50),
    @Title NVARCHAR(200),
    @Message NVARCHAR(1000),
    @RelatedEntityType NVARCHAR(50) = NULL,
    @RelatedEntityID INT = NULL,
    @FromUserID INT = NULL,
    @ActionUrl NVARCHAR(255) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO NewsSitePro2025_Notifications (UserID, Type, Title, Message, RelatedEntityType, RelatedEntityID, IsRead, CreatedAt, FromUserID, ActionUrl)
    VALUES (@UserID, @Type, @Title, @Message, @RelatedEntityType, @RelatedEntityID, 0, GETDATE(), @FromUserID, @ActionUrl);
    SELECT SCOPE_IDENTITY() as NotificationID;
END
GO

CREATE PROCEDURE NewsSitePro2025_sp_Notifications_MarkAsRead
    @NotificationID INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE NewsSitePro2025_Notifications SET IsRead = 1 WHERE NotificationID = @NotificationID;
END
GO

-- Reports
CREATE PROCEDURE NewsSitePro2025_sp_Reports_Insert
    @ArticleID INT = NULL,
    @ReporterID INT,
    @ReportedUserID INT = NULL,
    @Reason NVARCHAR(255),
    @Description NVARCHAR(1000) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO NewsSitePro2025_Reports (ArticleID, ReporterID, ReportedUserID, Reason, Description, CreatedAt, Status)
    VALUES (@ArticleID, @ReporterID, @ReportedUserID, @Reason, @Description, GETDATE(), 'Pending');
    SELECT SCOPE_IDENTITY() as ReportID;
END
GO

-- Admin: Ban/Unban
CREATE PROCEDURE NewsSitePro2025_sp_BanUser
    @UserID INT,
    @Reason NVARCHAR(500),
    @DurationDays INT, -- -1 for permanent
    @AdminID INT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @BannedUntil DATETIME2 = NULL;
    IF @DurationDays != -1
        SET @BannedUntil = DATEADD(day, @DurationDays, GETDATE());
    UPDATE NewsSitePro2025_Users
    SET IsBanned = 1, BannedUntil = @BannedUntil, BanReason = @Reason
    WHERE UserID = @UserID;
    INSERT INTO NewsSitePro2025_ActivityLogs (UserID, Action, Details, IpAddress, UserAgent)
    VALUES (@AdminID, 'BAN_USER', 'Banned user ' + CAST(@UserID AS NVARCHAR(10)) + ' for ' + CAST(@DurationDays AS NVARCHAR(10)) + ' days. Reason: ' + @Reason, 'Admin Panel', 'Admin Action');
    SELECT @@ROWCOUNT AS RowsAffected;
END
GO

CREATE PROCEDURE NewsSitePro2025_sp_UnbanUser
    @UserID INT,
    @AdminID INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE NewsSitePro2025_Users
    SET IsBanned = 0, BannedUntil = NULL, BanReason = NULL
    WHERE UserID = @UserID;
    INSERT INTO NewsSitePro2025_ActivityLogs (UserID, Action, Details, IpAddress, UserAgent)
    VALUES (@AdminID, 'UNBAN_USER', 'Unbanned user ' + CAST(@UserID AS NVARCHAR(10)), 'Admin Panel', 'Admin Action');
    SELECT @@ROWCOUNT AS RowsAffected;
END
GO 