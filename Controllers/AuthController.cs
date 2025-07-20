using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NewsSite.BL;
using NewsSite.Models;
namespace NewsSite.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _config;

        public AuthController(IConfiguration config)
        {
            _config = config;
        }


        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            var user = new User(_config);
            var token = user.LogIn(request.Password, request.Email);
            if (token == null)
                return Unauthorized("Invalid credentials or user locked.");

            return Ok(new { token });
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterRequest request)
        {
            try
            {
                var user = new User();
                bool success = user.Register(request.Name, request.Email, request.Password);
                if (!success)
                    return BadRequest("Registration failed. User may already exist.");

                return Ok("Registration successful.");
            }
            catch (System.Data.SqlClient.SqlException ex)
            {
                if (ex.Number == 2627) // UNIQUE constraint violation
                {
                    return BadRequest("A user with this email already exists.");
                }
                return BadRequest("Registration failed due to a database error.");
            }
            catch (Exception)
            {
                return BadRequest("Registration failed. Please try again.");
            }
        }

        
        [HttpPost("validate")]
        public IActionResult Validate([FromHeader(Name = "Authorization")] string authHeader)
        {
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
                return Unauthorized("Missing or invalid Authorization header.");
        
            var jwt = authHeader.Substring("Bearer ".Length);
        
            try
            {
                var user = new User().ExtractUserFromJWT(jwt); // Extract user details from the JWT token
                if (user == null)
                    return Unauthorized("Invalid token.");
        
                return Ok(new { message = "Token is valid.", userId = user.Id, username = user.Name });
            }
            catch (Exception ex)
            {
                return Unauthorized($"Token validation failed: {ex.Message}");
            }
        }
        [HttpPut("update")]
        public IActionResult Update([FromHeader(Name = "Authorization")] string authHeader, [FromBody] UpdateUserRequest request)
        {
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
                return Unauthorized();

            var jwt = authHeader.Substring("Bearer ".Length);
            var user = new User().ExtractUserFromJWT(jwt);

            bool success = user.UpdateDetails(user.Id, request.Name, request.Password);
            if (!success)
                return NotFound("User not found.");

            return Ok("User updated.");
        }
    }
}
