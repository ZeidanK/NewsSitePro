-- sp_Users_News_Get
CREATE PROCEDURE sp_Users_News_Get
    @UserID INT = NULL,
    @Username NVARCHAR(100) = NULL,
    @Email NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP 1 UserID, Username, Email, PasswordHash, IsAdmin, IsLocked, Bio, JoinDate, LastUpdated
    FROM dbo.Users_News
    WHERE
        (@UserID IS NULL OR UserID = @UserID)
        AND (@Username IS NULL OR Username = @Username)
        AND (@Email IS NULL OR Email = @Email)
END
GO

-- sp_Users_News_Insert
CREATE PROCEDURE sp_Users_News_Insert
    @Username NVARCHAR(100),
    @Email NVARCHAR(100),
    @PasswordHash NVARCHAR(200),
    @IsAdmin BIT,
    @IsLocked BIT,
    @Bio NVARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.Users_News (Username, Email, PasswordHash, IsAdmin, IsLocked, Bio, JoinDate, LastUpdated)
    VALUES (@Username, @Email, @PasswordHash, @IsAdmin, @IsLocked, @Bio, GETDATE(), GETDATE())
    
    SELECT SCOPE_IDENTITY() as UserID;
END
GO

-- sp_Users_News_Update
CREATE PROCEDURE sp_Users_News_Update
    @UserID INT,
    @Username NVARCHAR(100) = NULL,
    @PasswordHash NVARCHAR(200) = NULL,
    @IsAdmin BIT = NULL,
    @IsLocked BIT = NULL,
    @Bio NVARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.Users_News
    SET
        Username = ISNULL(@Username, Username),
        PasswordHash = ISNULL(@PasswordHash, PasswordHash),
        IsAdmin = ISNULL(@IsAdmin, IsAdmin),
        IsLocked = ISNULL(@IsLocked, IsLocked),
        Bio = ISNULL(@Bio, Bio),
        LastUpdated = GETDATE()
    WHERE UserID = @UserID
END
GO
