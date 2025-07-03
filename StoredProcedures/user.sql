-- sp_Users_News_Get
CREATE PROCEDURE sp_Users_News_Get
    @UserID INT = NULL,
    @Username NVARCHAR(100) = NULL,
    @Email NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP 1 *
    FROM dbo.Users_News
    WHERE
        (@UserID IS NULL OR UserID = @UserID)
        AND (@Username IS NULL OR Username = @Username)
        AND (@Email IS NULL OR Email = @Email)
END

-- sp_Users_News_Insert
CREATE PROCEDURE sp_Users_News_Insert
    @Username NVARCHAR(100),
    @Email NVARCHAR(100),
    @PasswordHash NVARCHAR(200),
    @IsAdmin BIT,
    @IsLocked BIT
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.Users_News (Username, Email, PasswordHash, IsAdmin, IsLocked)
    VALUES (@Username, @Email, @PasswordHash, @IsAdmin, @IsLocked)
END

-- sp_Users_News_Update
CREATE PROCEDURE sp_Users_News_Update
    @UserID INT,
    @Username NVARCHAR(100),
    @PasswordHash NVARCHAR(200),
    @IsAdmin BIT,
    @IsLocked BIT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.Users_News
    SET
        Username = @Username,
        PasswordHash = @PasswordHash,
        IsAdmin = @IsAdmin,
        IsLocked = @IsLocked
    WHERE UserID = @UserID
END
        
-- sp_SharedArticles_Insert        
CREATE PROCEDURE sp_SharedArticles_Insert
    @UserID INT,
    @Title NVARCHAR(255),
    @Content NVARCHAR(MAX),
    @Tag NVARCHAR(100),
    @SharedAt DATETIME
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO SharedArticles (UserID, Title, Content, Tag, SharedAt)
    VALUES (@UserID, @Title, @Content, @Tag, @SharedAt);
END
    
-- sp_SharedArticles_GetAll
CREATE PROCEDURE sp_SharedArticles_GetAll
AS
BEGIN
    SET NOCOUNT ON;

    SELECT Title, Content, Tag, SharedAt
    FROM SharedArticles
    ORDER BY SharedAt DESC;
END
-- sp_SystemLog_Insert
CREATE PROCEDURE sp_SystemLog_Insert
    @LogLevel NVARCHAR(50),
    @Message NVARCHAR(MAX),
    @Details NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO SystemLog (LogLevel, Message, Details)
    VALUES (@LogLevel, @Message, @Details);
END

-- sp_UserTags_Insert
CREATE PROCEDURE sp_UserTags_Insert
    @UserID INT,
    @Tag NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO UserTags (UserID, Tag)
    VALUES (@UserID, @Tag);
END
-- sp_UserTags_GetByUser
CREATE PROCEDURE sp_UserTags_GetByUser
    @UserID INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT Tag
    FROM UserTags
    WHERE UserID = @UserID;
END
