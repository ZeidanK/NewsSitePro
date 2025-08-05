# Notification System Implementation

## Overview
This document outlines the comprehensive notification system implementation for the NewsSitePro application. The system uses stored procedures for database operations and integrates with the existing business logic layer to provide real-time notifications for user actions.

## Architecture

### Database Layer (DAL)
- **Stored Procedures Used:**
  - `NewsSitePro2025_sp_Notifications_Insert` - Creates new notifications
  - `NewsSitePro2025_sp_Notifications_MarkAsRead` - Marks single notification as read
  - `NewsSitePro2025_sp_Notifications_GetUserNotifications` - Gets paginated user notifications
  - `NewsSitePro2025_sp_Notifications_GetUnreadCount` - Gets unread notification count
  - `NewsSitePro2025_sp_Notifications_MarkAllAsRead` - Marks all notifications as read
  - `NewsSitePro2025_sp_Notifications_GetSummary` - Gets notification summary

### Business Logic Layer (BL)
- **NotificationService**: Main service for notification operations
- **Integration with existing services**:
  - `UserService` - Follow notifications
  - `NewsService` - Like and new post notifications
  - `CommentService` - Comment notifications

### Controller Layer
- **NotificationController**: RESTful API endpoints for notification management

## Notification Types Implemented

### 1. Like Notifications
**Trigger**: When a user likes a post/article
**Recipient**: Post/article author
**Data**: 
- Liker name
- Post/article ID
- Action URL to the post

### 2. Comment Notifications
**Trigger**: When a user comments on a post/article
**Recipient**: Post/article author
**Data**:
- Commenter name
- Post/article ID
- Comment ID
- Action URL to the post with comment anchor

### 3. Follow Notifications
**Trigger**: When a user follows another user
**Recipient**: User being followed
**Data**:
- Follower name
- Follower ID
- Action URL to follower's profile

### 4. New Post Notifications
**Trigger**: When a user creates a new post
**Recipients**: All followers of the author
**Data**:
- Author name
- Post title (truncated)
- Post ID
- Action URL to the post

### 5. Admin Message Notifications
**Trigger**: Admin sends a message to users
**Recipients**: Specified users
**Data**:
- Admin ID
- Custom title and message
- Optional action URL

### 6. System Update Notifications
**Trigger**: System-wide updates or announcements
**Recipients**: All users or specified users
**Data**:
- System update title and message
- Optional action URL

### 7. Security Alert Notifications
**Trigger**: Security-related events
**Recipients**: Affected users
**Data**:
- Security alert message
- Optional action URL

## API Endpoints

### User Notification Management
- `GET /api/notification` - Get paginated notifications
- `GET /api/notification/summary` - Get notification summary
- `GET /api/notification/unread-count` - Get unread count
- `PUT /api/notification/mark-read/{id}` - Mark single notification as read
- `PUT /api/notification/mark-all-read` - Mark all notifications as read

### Admin Notification Management
- `POST /api/notification/admin/create` - Create custom notification
- `POST /api/notification/admin/message` - Send admin message
- `POST /api/notification/admin/system-update` - Send system update

## Integration Points

### 1. UserService Integration
```csharp
// Updated ToggleUserFollowAsync method
public async Task<FollowResult> ToggleUserFollowAsync(int currentUserId, int targetUserId)
{
    var result = await _dbService.ToggleUserFollow(currentUserId, targetUserId);
    
    // Create follow notification if user was followed
    if (result.IsFollowing)
    {
        var followerUser = _dbService.GetUserById(currentUserId);
        await _notificationService.CreateFollowNotificationAsync(
            currentUserId, targetUserId, followerUser.Name ?? "Unknown User");
    }
    
    return result;
}
```

### 2. NewsService Integration
```csharp
// Updated ToggleArticleLikeAsync method
public async Task<string> ToggleArticleLikeAsync(int articleId, int userId)
{
    var result = await Task.FromResult(_dbService.ToggleArticleLike(articleId, userId));
    
    // Create like notification if article was liked
    if (result == "liked")
    {
        var article = await _dbService.GetNewsArticleById(articleId);
        var liker = _dbService.GetUserById(userId);
        await _notificationService.CreateLikeNotificationAsync(
            articleId, userId, article.UserID, liker.Name ?? "Unknown User");
    }
    
    return result;
}

// Updated CreateNewsArticleAsync method
public async Task<int> CreateNewsArticleAsync(NewsArticle article)
{
    var articleId = await Task.FromResult(_dbService.CreateNewsArticle(article));
    
    // Create new post notifications for followers
    if (articleId > 0)
    {
        var author = _dbService.GetUserById(article.UserID);
        await _notificationService.CreateNewPostNotificationsForFollowersAsync(
            articleId, article.UserID, author.Name ?? "Unknown User", article.Title);
    }
    
    return articleId;
}
```

### 3. CommentService Integration
```csharp
// Updated CreateCommentAsync method
public async Task<bool> CreateCommentAsync(Comment comment)
{
    int commentId = await _dbService.CreateComment(comment);
    
    if (commentId > 0)
    {
        var article = await _dbService.GetNewsArticleById(comment.PostID);
        var commenter = _dbService.GetUserById(comment.UserID);
        await _notificationService.CreateCommentNotificationAsync(
            comment.PostID, commentId, comment.UserID, 
            article.UserID, commenter.Name ?? "Unknown User");
        return true;
    }
    
    return false;
}
```

## Database Schema

### Notifications Table
```sql
NewsSitePro2025_Notifications
- NotificationID (INT, Primary Key, Identity)
- UserID (INT, Foreign Key to Users)
- Type (NVARCHAR(50)) - Like, Comment, Follow, NewPost, etc.
- Title (NVARCHAR(200))
- Message (NVARCHAR(1000))
- RelatedEntityType (NVARCHAR(50)) - Post, User, etc.
- RelatedEntityID (INT) - ID of related entity
- IsRead (BIT) - Read status
- CreatedAt (DATETIME2) - Creation timestamp
- FromUserID (INT) - ID of user who triggered notification
- ActionUrl (NVARCHAR(255)) - URL for notification action
```

## Dependency Injection Setup

### Program.cs Configuration
```csharp
// Register NotificationService
builder.Services.AddScoped<NewsSite.BL.Services.NotificationService>();

// Updated service registrations to include NotificationService dependency
builder.Services.AddScoped<NewsSite.BL.Services.IUserService, NewsSite.BL.Services.UserService>();
builder.Services.AddScoped<NewsSite.BL.Services.INewsService, NewsSite.BL.Services.NewsService>();
builder.Services.AddScoped<NewsSite.BL.Services.ICommentService, NewsSite.BL.Services.CommentService>();
```

## Error Handling

The notification system implements graceful error handling:

1. **Non-blocking failures**: Notification errors don't prevent the main action (like, comment, follow) from completing
2. **Fallback mechanisms**: If stored procedures fail, the system falls back to direct SQL queries
3. **Logging**: All notification errors are logged but don't bubble up to the user interface
4. **Validation**: Proper validation prevents self-notifications (users can't get notifications for their own actions)

## Usage Examples

### Frontend Integration
```javascript
// Get unread notification count
fetch('/api/notification/unread-count')
  .then(response => response.json())
  .then(data => {
    if (data.success) {
      updateNotificationBadge(data.unreadCount);
    }
  });

// Mark notification as read
fetch(`/api/notification/mark-read/${notificationId}`, {
  method: 'PUT'
})
.then(response => response.json())
.then(data => {
  if (data.success) {
    updateNotificationDisplay();
  }
});

// Get notifications with pagination
fetch(`/api/notification?page=1&pageSize=20`)
  .then(response => response.json())
  .then(data => {
    if (data.success) {
      displayNotifications(data.notifications);
    }
  });
```

## Performance Considerations

1. **Pagination**: All notification retrieval uses pagination to prevent large data sets
2. **Indexing**: Database indexes on UserID, CreatedAt, and IsRead columns for optimal query performance
3. **Batch operations**: Bulk notification creation for system updates
4. **Async operations**: All database operations are asynchronous to prevent blocking

## Security Features

1. **User isolation**: Users can only access their own notifications
2. **Admin verification**: Admin-only endpoints verify user permissions
3. **Parameter validation**: All inputs are validated to prevent injection attacks
4. **Rate limiting**: Consider implementing rate limiting for notification creation

## Future Enhancements

1. **Real-time notifications**: Integrate with SignalR for real-time push notifications
2. **Email notifications**: Add email notification preferences and delivery
3. **Push notifications**: Implement browser push notifications
4. **Notification categories**: Allow users to customize notification preferences by type
5. **Notification templates**: Create customizable notification templates
6. **Notification scheduling**: Add ability to schedule notifications for later delivery

## Testing

The notification system should be tested for:

1. **Unit tests**: Test individual notification service methods
2. **Integration tests**: Test end-to-end notification flows
3. **Performance tests**: Test with large numbers of notifications
4. **Security tests**: Verify user isolation and permission checks
5. **Error handling tests**: Test fallback mechanisms and error scenarios

## Conclusion

This notification system provides a robust, scalable foundation for user engagement in the NewsSitePro application. It integrates seamlessly with existing business logic and provides comprehensive notification coverage for all major user actions. The system is designed for performance, security, and future extensibility.
