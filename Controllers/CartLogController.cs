using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using LinenManagementSystem.Models;
using LinenManagementSystem.Repositories;
using LinenManagementSystem.Services;

namespace LinenManagementSystem.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class CartLogController : ControllerBase
    {
        private readonly ICartLogService _cartLogService;

        public CartLogController(ICartLogService cartLogService)
        {
            _cartLogService = cartLogService;
        }

        [HttpGet("{cartLogId}")]
        public async Task<IActionResult> GetCartLogById(int cartLogId)
        {
            var cartLog = await _cartLogService.GetCartLogByIdAsync(cartLogId);
            if (cartLog == null)
            {
                return NotFound();
            }

            // Optionally filter out linen if the cart is soiled
            if (cartLog.Cart.Type == "Soiled")
            {
                cartLog.Linen = null; // or filter out as per requirements
            }

            return Ok(new { cartLog });
        }

        [HttpGet]
        public async Task<IActionResult> GetCartLogs([FromQuery] string cartType, [FromQuery] string location, [FromQuery] int? employeeId)
        {
            var cartLogs = await _cartLogService.GetCartLogsAsync(cartType, location, employeeId);
            return Ok(new { cartLogs });
        }

        [HttpPost("upsert")]
        public async Task<IActionResult> UpsertCartLog([FromBody] CartLog cartLog)
        {
            var employeeId = 1; // Replace with actual employee ID from claims or session
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

