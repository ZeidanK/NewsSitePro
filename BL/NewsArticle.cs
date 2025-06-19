namespace NewsSite.BL
{
    public class NewsArticle
    {
        public int ArticleID { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string SourceURL { get; set; }
        public string ImageURL { get; set; }
        public DateTime PublishDate { get; set; }
        public string Category { get; set; }
    }

}
