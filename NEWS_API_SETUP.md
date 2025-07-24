# News API Integration Setup Guide

## Overview
This integration adds automatic news fetching capabilities to your news social platform using the News API service. The system will:

1. **Automatically fetch news** from News API every hour via background service
2. **Display live news** in a dedicated "Live News" tab
3. **Allow users to save articles** from live feed to their personal feed
4. **Support multiple news categories** (Business, Technology, Sports, etc.)
5. **Enable live news search** functionality

## Setup Instructions

### 1. Get News API Key
1. Visit [https://newsapi.org/](https://newsapi.org/)
2. Sign up for a free account
3. Get your API key from the dashboard

### 2. Configure API Key
Add your News API key to `appsettings.Development.json`:

```json
{
  "NewsApi": {
    "ApiKey": "YOUR_ACTUAL_NEWS_API_KEY_HERE",
    "BaseUrl": "https://newsapi.org/v2/",
    "MaxArticlesPerFetch": "20",
    "FetchIntervalHours": "1"
  }
}
```

### 3. Database Setup (Optional - for Post table)
If you want to implement the full Post system with shares and comments, run these SQL commands:

```sql
-- Posts Table
CREATE TABLE Posts (
    PostID INT IDENTITY(1,1) PRIMARY KEY,
    ArticleID INT NOT NULL,
    UserID INT NOT NULL,
    OriginalPostID INT NULL,
    PostedDate DATETIME NOT NULL DEFAULT GETDATE(),
    LastEditedDate DATETIME NULL,
    DeletedAt DATETIME NULL,
    CONSTRAINT FK_Posts_Articles FOREIGN KEY (ArticleID) REFERENCES Articles(ArticleID),
    CONSTRAINT FK_Posts_Users FOREIGN KEY (UserID) REFERENCES Users(UserID),
    CONSTRAINT FK_Posts_OriginalPosts FOREIGN KEY (OriginalPostID) REFERENCES Posts(PostID)
);

-- Indexes for performance
CREATE INDEX IX_Posts_UserID ON Posts(UserID);
CREATE INDEX IX_Posts_ArticleID ON Posts(ArticleID);
CREATE INDEX IX_Posts_OriginalPostID ON Posts(OriginalPostID);
```

### 4. Running the Application

1. **Install the NewsAPI package** (if not already done):
   ```bash
   dotnet add package NewsAPI
   ```

2. **Start the application**:
   ```bash
   dotnet run
   ```

3. **The background service will start automatically** and begin fetching news every hour.

## Features Implemented

### 🔴 Live News Tab
- Click the "Live News" tab to view fresh articles from News API
- Browse by category (General, Business, Technology, Sports, Health, Entertainment, Science)
- Search live news with custom queries
- Articles are fetched in real-time, not stored in your database

### 💾 Save to Feed
- Click "Save to Feed" on any live article to add it to your database
- Saved articles become regular posts that users can like, comment on, and share

### ⬇️ Sync to Database
- Click "Sync to Database" to automatically fetch and save articles from all categories
- This happens automatically every hour via the background service
- Great for populating your site with fresh content

### 🤖 NewsBot User
- System automatically creates a "NewsBot" user (UserID: -1)
- All News API articles are attributed to NewsBot
- NewsBot has admin privileges and a special avatar

## API Endpoints Added

- `GET /api/News/headlines` - Get live headlines by category
- `GET /api/News/search` - Search News API
- `POST /api/News/sync` - Manually trigger news sync
- `GET /api/Posts/live-headlines` - Live headlines formatted for your feed
- `GET /api/Posts/search-live` - Search live news
- `POST /api/Posts/sync-news` - Sync news to database
- `POST /api/Posts/save-live-article` - Save a live article to database

## Configuration Options

In `appsettings.json`, you can configure:

- `ApiKey`: Your News API key
- `MaxArticlesPerFetch`: How many articles to fetch per request (max 100 for free tier)
- `FetchIntervalHours`: How often to auto-sync news (default: 1 hour)

## Rate Limits (Free Tier)
- 1,000 requests per day
- 500 articles per request
- The background service respects these limits

## Troubleshooting

### "Failed to load live news"
1. Check your API key in `appsettings.Development.json`
2. Verify you haven't exceeded your daily quota
3. Check the console for detailed error messages

### Background service not working
1. Ensure the services are registered in `Program.cs`
2. Check application logs for background service errors
3. The service waits 1 minute after startup before first run

### Articles not displaying
1. Check your internet connection
2. Verify News API service status
3. Check browser console for JavaScript errors

## Optional Enhancements

You can further enhance the system by:

1. **Adding more news sources** via the News API sources endpoint
2. **Implementing user preferences** for news categories
3. **Adding trending topics** based on article engagement
4. **Creating news alerts** for breaking news
5. **Adding social sharing** features

## News API Documentation
For more advanced features, check the official [News API documentation](https://newsapi.org/docs).
