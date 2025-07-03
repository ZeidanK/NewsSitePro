using Microsoft.AspNetCore.Mvc;
using NewsSite.BL;
using System.Collections.Generic;

namespace NewsSite.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NewsController : ControllerBase
    {
        // GET: api/news/bytag/{tag}
        [HttpGet("bytag/{tag}")]
        public IActionResult GetByTag(string tag)
        {
            var newsList = new News().GetByTag(tag);
            if (newsList == null || newsList.Count == 0)
                return NotFound("No articles found with this tag.");

            return Ok(newsList);
        }

        // GET: api/news/search?query=term
        [HttpGet("search")]
        public IActionResult Search([FromQuery] string query)
        {
            var newsList = new News().Search(query);
            if (newsList == null || newsList.Count == 0)
                return NotFound("No matching articles found.");

            return Ok(newsList);
        }

        // GET: api/news/{id}
        [HttpGet("{id}")]
        public IActionResult ViewArticle(int id)
        {
            var article = new News().GetById(id);
            if (article == null)
                return NotFound("Article not found.");

            return Ok(article);
        }
    }
}
