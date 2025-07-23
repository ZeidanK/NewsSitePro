namespace NewsSitePro.Models
{
    public class PostDisplayConfig
    {
        public PostLayout Layout { get; set; } = PostLayout.Default;
        public bool ShowAuthorInfo { get; set; } = true;
        public bool ShowCategory { get; set; } = true;
        public bool ShowImage { get; set; } = true;
        public bool ShowSource { get; set; } = true;
        public bool ShowStats { get; set; } = true;
        public bool EnableInteractions { get; set; } = true;
        public bool ShowFullContent { get; set; } = false;
        public int ContentPreviewLength { get; set; } = 200;
        public bool AllowEdit { get; set; } = false;
        public bool AllowDelete { get; set; } = false;
        public string CssClass { get; set; } = "";
        public string ContainerClass { get; set; } = "post-card";
    }

    public enum PostLayout
    {
        Default,
        Compact,
        Minimal,
        List,
        Grid
    }
}
