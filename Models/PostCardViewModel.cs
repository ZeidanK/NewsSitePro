using NewsSite.BL;

namespace NewsSitePro.Models
{
    /// <summary>
    /// Enhanced ViewModel for PostCard component that supports both legacy Config
    /// and new Context-based rendering for backward compatibility during migration
    /// </summary>
    public class PostCardViewModel
    {
        public NewsArticle Post { get; set; } = new NewsArticle();
        
        /// <summary>
        /// Legacy configuration object (maintained for backward compatibility)
        /// </summary>
        public PostDisplayConfig? Config { get; set; }
        
        /// <summary>
        /// New enhanced context system (preferred for new implementations)
        /// </summary>
        public PostDisplayContext? Context { get; set; }
        
        public User? CurrentUser { get; set; }
        public string? BaseUrl { get; set; }
        public IEnumerable<dynamic>? Comments { get; set; }

        /// <summary>
        /// Gets the effective context, converting from Config if Context is not set
        /// </summary>
        public PostDisplayContext EffectiveContext
        {
            get
            {
                if (Context != null)
                    return Context;

                // Convert legacy Config to Context for backward compatibility
                if (Config != null)
                {
                    return new PostDisplayContext
                    {
                        Layout = Config.Layout,
                        ShowAuthorInfo = Config.ShowAuthorInfo,
                        ShowCategory = Config.ShowCategory,
                        ShowImage = Config.ShowImage,
                        ShowSource = Config.ShowSource,
                        ShowStats = Config.ShowStats,
                        EnableInteractions = Config.EnableInteractions,
                        ShowFullContent = Config.ShowFullContent,
                        ContentPreviewLength = Config.ContentPreviewLength,
                        CssClass = Config.CssClass,
                        ContainerClass = Config.ContainerClass,
                        CanEdit = Config.AllowEdit,
                        CanDelete = Config.AllowDelete
                    };
                }

                // Default context if neither is set
                return new PostDisplayContext();
            }
        }
    }
}
