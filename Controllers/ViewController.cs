using Microsoft.AspNetCore.Mvc;

namespace NewsSite.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ViewController : ControllerBase
    {
        /// <summary>
        /// Returns the status of the API to confirm it's running properly.
        /// </summary>
        [HttpGet("status")]
        public IActionResult GetStatus()
        {
            return Ok(new
            {
                status = "API is running",
                version = "1.0",
                serverTime = DateTime.Now
            });
        }
    }
}
