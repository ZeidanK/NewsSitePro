using NewsSite.BL;

namespace NewsSite.Models
{
    public class SearchUserResult
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Bio { get; set; }
        public DateTime JoinDate { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsLocked { get; set; }
        public int PostsCount { get; set; }
        public int FollowersCount { get; set; }
        public int FollowingCount { get; set; }
        public string? ProfilePicture { get; set; }
    }

    public class SearchArticleResult
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
        public string? UserName { get; set; }
        public int LikesCount { get; set; }
        public int ViewsCount { get; set; }
        public bool IsLiked { get; set; }
        public bool IsSaved { get; set; }
    }
}
