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
