using Microsoft.AspNetCore.Mvc;
using LinenManagementSystem.DTOs.Auth;
using LinenManagementSystem.Services;

namespace LinenManagementSystem.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthenticationController : ControllerBase
    {
        private readonly IAuthenticationService _authService;

        public AuthenticationController(IAuthenticationService authService)
        {
            _authService = authService;
        }

        // POST: api/auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto loginRequest)
        {
            var result = await _authService.LoginAsync(loginRequest);

            if (result == null)
            {
                return Unauthorized("Invalid credentials");
            }

            return Ok(result);
        }

        // POST: api/auth/logout
        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] LogoutRequest logoutRequest)
        {
            var success = await _authService.LogoutAsync(logoutRequest.RefreshToken);

            if (!success)
            {
                return BadRequest("Invalid refresh token");
            }

            return Ok(new {  message = "Logout successful" });
        }

        // POST: api/auth/refresh
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshRequest refreshRequest)
        {
            var result = await _authService.RefreshTokenAsync(refreshRequest.RefreshToken);

            if (result == null)
            {
                return Unauthorized("Invalid refresh token");
            }

            return Ok(result);
        }
    }
}
