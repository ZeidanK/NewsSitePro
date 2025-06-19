-- sp_Users2025Pro_Get
CREATE PROCEDURE sp_Users2025Pro_Get
    @id INT = NULL,
    @name NVARCHAR(100) = NULL,
    @Email NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP 1 *
    FROM Users
    WHERE
        (@id IS NULL OR id = @id)
        AND (@name IS NULL OR name = @name)
        AND (@Email IS NULL OR email = @Email)
END

-- sp_Users2025Pro_Insert
CREATE PROCEDURE sp_Users2025Pro_Insert
    @name NVARCHAR(100),
    @Email NVARCHAR(100),
    @passwordHash NVARCHAR(200),
    @isAdmin BIT,
    @isLocked BIT
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO Users (name, email, passwordHash, isAdmin, isLocked)
    VALUES (@name, @Email, @passwordHash, @isAdmin, @isLocked)
END


-- sp_Users2025Pro_Update
CREATE PROCEDURE sp_Users2025Pro_Update
    @id INT,
    @name NVARCHAR(100),
    @passwordHash NVARCHAR(200),
    @isAdmin BIT,
    @isLocked BIT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE Users
    SET
        name = @name,
        passwordHash = @passwordHash,
        isAdmin = @isAdmin,
        isLocked = @isLocked
    WHERE id = @id
END