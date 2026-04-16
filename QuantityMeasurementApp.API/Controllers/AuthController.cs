using Microsoft.AspNetCore.Mvc;
using QuantityMeasurementApp.API.DTO;
using QuantityMeasurementApp.Service;

namespace QuantityMeasurementApp.API.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _service;

        public AuthController(IAuthService service)
        {
            _service = service;
        }

        // 🔐 REGISTER API
        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterRequest request)
        {
            if (request == null) return BadRequest(new { message = "Request body is empty" });
            if (string.IsNullOrEmpty(request.Email)) return BadRequest(new { message = "Email is required" });
            if (string.IsNullOrEmpty(request.Password)) return BadRequest(new { message = "Password is required" });
            if (string.IsNullOrEmpty(request.Name)) return BadRequest(new { message = "Name is required" });

            var result = _service.Register(request.Name, request.Email, request.Password);

            return Ok(new
            {
                message = result
            });
        }

        // 🔐 LOGIN API
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            if (request == null) return BadRequest(new { message = "Request body is empty" });
            if (string.IsNullOrEmpty(request.Email)) return BadRequest(new { message = "Email is required" });
            if (string.IsNullOrEmpty(request.Password)) return BadRequest(new { message = "Password is required" });

            var token = _service.Login(request.Email, request.Password);

            return Ok(new
            {
                message = "Login successful",
                token = token
            });
        }
    }
}

