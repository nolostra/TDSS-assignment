using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using LinenManagementSystem.Models;
using LinenManagementSystem.Repositories;
using LinenManagementSystem.Services;
using LinenManagementSystem.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace LinenManagementSystem.Controllers
{
    [ApiController]
    [Route("api/cartlogs")]
    [Authorize]
    public class CartLogController : ControllerBase
    {
        private readonly ILogger<CartLogController> _logger;
        private readonly ICartLogService _cartLogService;
        // private readonly IEmployeeService _employeeService;

        public CartLogController(ILogger<CartLogController> logger, ICartLogService cartLogService)
        {
            _logger = logger;
            _cartLogService = cartLogService;
            // _employeeService = employeeService;
        }

        [HttpGet("{cartLogId}")]
        public async Task<IActionResult> GetCartLogById(int cartLogId)
        {
            _logger.LogInformation($"Fetching cart log with ID: {cartLogId}");
            var cartLog = await _cartLogService.GetCartLogByIdAsync(cartLogId);
            if (cartLog == null)
            {
                _logger.LogWarning($"Cart log with ID {cartLogId} not found.");
                return NotFound(new { message = $"Cart log with ID {cartLogId} not found." });
            }
            _logger.LogInformation($"Cart log with ID {cartLogId} retrieved successfully.");
            return Ok(new { cartLog });
        }

        [HttpGet]
        public async Task<IActionResult> GetCartLogs([FromQuery] string? cartType, [FromQuery] string? location, [FromQuery] int? employeeId)
        {
            var cartLogs = await _cartLogService.GetCartLogsAsync(cartType, location, employeeId);
             _logger.LogInformation($"Fetched -  {cartLogs}");
            return Ok(new { cartLogs });
        }

        [HttpPost("upsert")]
        public async Task<IActionResult> UpsertCartLog([FromBody] CartLogInsert cartLog)
        {
            var employeeIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (employeeIdClaim == null || !int.TryParse(employeeIdClaim.Value, out var employeeId))
            {
                return Unauthorized(new { message = "Invalid employee ID in token" });
            }

            var updatedCartLog = await _cartLogService.UpsertCartLogAsync(cartLog, employeeId);
            return Ok(new { cartLogs = updatedCartLog });
        }

        [HttpDelete("{cartLogId}")]
        public async Task<IActionResult> DeleteCartLog(int cartLogId)
        {
            var employeeIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (employeeIdClaim == null || !int.TryParse(employeeIdClaim.Value, out var employeeId))
            {
                return Unauthorized(new { message = "Invalid employee ID in token" });
            }

            var deleted = await _cartLogService.DeleteCartLogAsync(cartLogId, employeeId);
            if (!deleted)
            {
                return NotFound(new { message = $"Cart log with ID {cartLogId} not found." });
            }

            return NoContent();
        }
    }
}
