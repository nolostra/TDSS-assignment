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

        private readonly EmployeeService _employeeService;

        public CartLogController(ILogger<CartLogController> logger, ICartLogService cartLogService, EmployeeService EmployeeService)
        {
            _logger = logger;
            _cartLogService = cartLogService;
            _employeeService = EmployeeService;
        }

        [HttpGet("{cartLogId}")]
        public async Task<IActionResult> GetCartLogById(int cartLogId)
        {
            _logger.LogInformation($"Fetching cart log with ID: {cartLogId}");
            var cartLog = await _cartLogService.GetCartLogByIdAsync(cartLogId);
            if (cartLog == null)
            {
                _logger.LogWarning($"Cart log with ID {cartLogId} not found.");
                return NotFound();
            }
            _logger.LogInformation($"Cart log with ID {cartLogId} retrieved successfully.");

            return Ok(new { cartLog });
        }

        [HttpGet]
        public async Task<IActionResult> GetCartLogs([FromQuery] string cartType, [FromQuery] string location, [FromQuery] int? employeeId)
        {
            var cartLogs = await _cartLogService.GetCartLogsAsync(cartType, location, employeeId);
            return Ok(new { cartLogs });
        }

        [HttpPost("upsert")]
        public async Task<IActionResult> UpsertCartLog([FromBody] CartLogInsert cartLog)
        {
            // Extract employeeId from the JWT claims
            var employeeIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);

            if (employeeIdClaim == null)
            {
                return Unauthorized(new { message = "Employee ID not found in token" });
            }

            var employeeId = int.Parse(employeeIdClaim.Value); // Parse the claim value to an integer

            // Proceed with upserting the cart log
            var updatedCartLog = await _cartLogService.UpsertCartLogAsync(cartLog, employeeId);

            return Ok(new { cartLogs = updatedCartLog });
        }

        [HttpDelete("{cartLogId}")]
        public async Task<IActionResult> DeleteCartLog(int cartLogId)
        {
            var employeeId = 1; // Replace with actual employee ID from claims or session
            var deleted = await _cartLogService.DeleteCartLogAsync(cartLogId, employeeId);
            if (!deleted)
            {
                return Unauthorized(); // or NotFound, based on your logic
            }

            return NoContent();
        }
    }

}

