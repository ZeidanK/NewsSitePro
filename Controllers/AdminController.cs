using Microsoft.AspNetCore.Mvc;
using NewsSite.DAL;
using NewsSite.BL;
using System.Collections.Generic;

namespace NewsSite.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly UserRepository _userRepo;
        private readonly ReportRepository _reportRepo;

        public AdminController()
        {
            _userRepo = new UserRepository();
            _reportRepo = new ReportRepository();
        }

        [HttpPut("lock/{id}")]
        public IActionResult LockUser(int id)
        {
            bool success = _userRepo.SetLockStatus(id, true);
            if (!success)
                return NotFound("User not found or could not be locked.");

            return Ok("User locked.");
        }

        [HttpPut("unlock/{id}")]
        public IActionResult UnlockUser(int id)
        {
            bool success = _userRepo.SetLockStatus(id, false);
            if (!success)
                return NotFound("User not found or could not be unlocked.");

            return Ok("User unlocked.");
        }

        [HttpGet("reports")]
        public ActionResult<List<Report>> GetAllReports()
        {
            var reports = _reportRepo.GetAllReports();
            return Ok(reports);
        }
    }
}
