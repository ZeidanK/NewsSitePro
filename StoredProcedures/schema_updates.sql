-- =============================================
-- Additional Schema Updates for News Site
-- =============================================

-- Add SourceURL and SourceName columns to NewsArticles table
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'NewsArticles' AND COLUMN_NAME = 'SourceURL')
BEGIN
    ALTER TABLE NewsArticles ADD SourceURL NVARCHAR(500) NULL;
END
GO

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'NewsArticles' AND COLUMN_NAME = 'SourceName')
BEGIN
    ALTER TABLE NewsArticles ADD SourceName NVARCHAR(100) NULL;
END
GO

-- Add Bio and JoinDate columns to Users_News table if they don't exist
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Users_News' AND COLUMN_NAME = 'Bio')
BEGIN
    ALTER TABLE Users_News ADD Bio NVARCHAR(500) NULL;
END
GO

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Users_News' AND COLUMN_NAME = 'JoinDate')
BEGIN
    ALTER TABLE Users_News ADD JoinDate DATETIME2 DEFAULT GETDATE();
END
GO

-- Update existing users to have a JoinDate if it's NULL
UPDATE Users_News 
SET JoinDate = GETDATE() 
WHERE JoinDate IS NULL;
GO
