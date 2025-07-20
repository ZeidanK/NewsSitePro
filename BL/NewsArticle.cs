namespace NewsSite.BL
{
    public class NewsArticle
    {
        public int ArticleID { get; set; }
        public string? Title { get; set; }
        public string? Content { get; set; }
        public string? SourceURL { get; set; }
        public string? SourceName { get; set; }
        public string? ImageURL { get; set; }
        public DateTime PublishDate { get; set; }
        public string? Category { get; set; }
        public int UserID { get; set; }
        public string? Username { get; set; }
        public int LikesCount { get; set; }
        public int ViewsCount { get; set; }
        public bool IsLiked { get; set; }
        public bool IsSaved { get; set; }
    }

    public class UserActivity
    {
        public int PostsCount { get; set; }
        public int LikesCount { get; set; }
        public int SavedCount { get; set; }
        public int FollowersCount { get; set; }
    }

    public class UserProfile
    {
        public int UserID { get; set; }
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? Bio { get; set; }
        public DateTime JoinDate { get; set; }
        public bool IsAdmin { get; set; }
        public string? ProfilePicture { get; set; }
        public UserActivity? Activity { get; set; }
        public List<NewsArticle>? RecentPosts { get; set; }
    }
}
