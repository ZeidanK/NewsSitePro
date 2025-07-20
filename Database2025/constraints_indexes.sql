-- =============================================
-- NewsSitePro2025: Constraints, Indexes, and Triggers
-- =============================================

-- Indexes for performance
CREATE INDEX IX_NewsSitePro2025_NewsArticles_Category ON NewsSitePro2025_NewsArticles(Category);
GO
CREATE INDEX IX_NewsSitePro2025_ArticleLikes_ArticleID ON NewsSitePro2025_ArticleLikes(ArticleID);
GO
CREATE INDEX IX_NewsSitePro2025_ArticleLikes_UserID ON NewsSitePro2025_ArticleLikes(UserID);
GO
CREATE INDEX IX_NewsSitePro2025_ArticleViews_ArticleID ON NewsSitePro2025_ArticleViews(ArticleID);
GO
CREATE INDEX IX_NewsSitePro2025_ArticleViews_UserID ON NewsSitePro2025_ArticleViews(UserID);
GO
CREATE INDEX IX_NewsSitePro2025_SavedArticles_UserID ON NewsSitePro2025_SavedArticles(UserID);
GO
CREATE INDEX IX_NewsSitePro2025_Comments_ArticleID ON NewsSitePro2025_Comments(ArticleID);
GO
CREATE INDEX IX_NewsSitePro2025_Comments_UserID ON NewsSitePro2025_Comments(UserID);
GO
CREATE INDEX IX_NewsSitePro2025_Notifications_UserID ON NewsSitePro2025_Notifications(UserID);
GO
CREATE INDEX IX_NewsSitePro2025_Reports_ArticleID ON NewsSitePro2025_Reports(ArticleID);
GO
CREATE INDEX IX_NewsSitePro2025_Reports_ReporterID ON NewsSitePro2025_Reports(ReporterID);
GO
CREATE INDEX IX_NewsSitePro2025_UserFollows_FollowerUserID ON NewsSitePro2025_UserFollows(FollowerUserID);
GO
CREATE INDEX IX_NewsSitePro2025_UserFollows_FollowedUserID ON NewsSitePro2025_UserFollows(FollowedUserID);
GO

-- Constraints for data integrity are already included in table definitions (PK, FK, UNIQUE, CHECK)
-- Add triggers for auditing or cascading deletes if needed
-- Example: Audit trigger for NewsArticles (optional)
-- CREATE TRIGGER NewsSitePro2025_trg_NewsArticles_Audit
-- ON NewsSitePro2025_NewsArticles
-- AFTER INSERT, UPDATE, DELETE
-- AS
-- BEGIN
--     -- Audit logic here
-- END
-- GO 