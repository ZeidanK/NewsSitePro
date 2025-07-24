using Microsoft.AspNetCore.Mvc;
using NewsSitePro.Models;
using NewsSite.BL;

namespace NewsSitePro.ViewComponents.PostCard
{
    public class PostCardViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(PostCardViewModel model)
        {
            if (model == null)
            {
                return Content(string.Empty);
            }

            // Apply configuration defaults if not set
            model.Config ??= new PostDisplayConfig();
            
            // Choose the appropriate view based on layout
            var viewName = model.Config.Layout switch
            {
                PostLayout.Compact => "Compact",
                PostLayout.Minimal => "Minimal",
                PostLayout.List => "List",
                _ => "Default"
            };

            return View(viewName, model);
        }
    }
}
