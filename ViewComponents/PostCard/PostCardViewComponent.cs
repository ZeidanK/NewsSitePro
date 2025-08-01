using Microsoft.AspNetCore.Mvc;
using NewsSitePro.Models;
using NewsSite.BL;

namespace NewsSitePro.ViewComponents.PostCard
{
    /// <summary>
    /// Enhanced PostCard ViewComponent that supports both legacy Config-based
    /// and new Context-based rendering for maximum flexibility and scalability
    /// </summary>
    public class PostCardViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(PostCardViewModel model)
        {
            if (model == null)
            {
                return Content(string.Empty);
            }

            // Ensure we have either Config or Context
            if (model.Config == null && model.Context == null)
            {
                model.Config = new PostDisplayConfig();
            }

            // Get the effective context (handles backward compatibility)
            var context = model.EffectiveContext;
            
            // Pass comments to ViewData if available
            if (model.Comments != null)
            {
                ViewData["Comments"] = model.Comments;
            }
            
            // Pass follow status to ViewData if context has follow information
            if (model.Post != null)
            {
                ViewData["IsFollowing_" + model.Post.UserID] = context.IsFollowingAuthor;
            }
            
            // Choose the appropriate view based on layout
            var viewName = context.Layout switch
            {
                PostLayout.Compact => "Compact",
                PostLayout.Minimal => "Minimal",
                PostLayout.List => "List",
                PostLayout.Grid => "Grid",
                _ => "Default"
            };

            return View(viewName, model);
        }
    }
}
