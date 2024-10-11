using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using LinenManagementSystem.Models;
using LinenManagementSystem.Data;
using LinenManagementSystem.DTOs;

namespace LinenManagementSystem.Repositories
{
    public class CartLogRepository : ICartLogRepository
    {
        private readonly ApplicationDbContext _context;

        public CartLogRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<CartLog> GetCartLogByIdAsync(int cartLogId)
        {
            return await _context.CartLog
                .Include(c => c.Cart)
                .Include(c => c.Location)
                .Include(c => c.Employee)
                .Include(c => c.CartLogDetails) // Adjusted to ensure CartLogDetails are included
                .FirstOrDefaultAsync(cl => cl.CartLogId == cartLogId);
        }

        public async Task<IEnumerable<CartLog>> GetCartLogsAsync(string cartType, string location, int? employeeId)
        {
            var query = _context.CartLog.AsQueryable();

            if (!string.IsNullOrEmpty(cartType))
            {
                query = query.Include(cl => cl.Cart) // Ensure Cart is included for filtering
                             .Where(cl => cl.Cart.Type == cartType);
            }

            if (!string.IsNullOrEmpty(location))
            {
                query = query.Include(cl => cl.Location) // Ensure Location is included for filtering
                             .Where(cl => cl.Location.Name == location);
            }

            if (employeeId.HasValue)
            {
                query = query.Where(cl => cl.EmployeeId == employeeId.Value);
            }

            return await query.OrderByDescending(cl => cl.DateWeighed).ToListAsync();
        }

        public async Task<CartLog> UpsertCartLogAsync(CartLogInsert cartLogDto)
        {
            var cartLog = new CartLog
            {
                CartLogId = cartLogDto.CartLogId,
                ReceiptNumber = cartLogDto.ReceiptNumber,
                ReportedWeight = cartLogDto.ReportedWeight,
                ActualWeight = cartLogDto.ActualWeight,
                Comments = cartLogDto.Comments,
                DateWeighed = cartLogDto.DateWeighed,
                CartId = cartLogDto.CartId,
                LocationId = cartLogDto.LocationId,
                EmployeeId = cartLogDto.EmployeeId,
            };

            foreach (var item in cartLogDto.Linen)
            {
                var cartLogDetail = new CartLogDetail
                {
                    CartLogDetailId = item.CartLogDetailId,
                    CartLogId = cartLogDto.CartLogId,
                    LinenId = item.LinenId,
                    Count = item.Count,
                };

                await _context.CartLogDetails.AddAsync(cartLogDetail);

                var cartLogLinen = new Linen
                {
                    LinenId = item.LinenId,
                    Name = item.Name,
                    Weight = 0,
                };

                await _context.Linens.AddAsync(cartLogLinen);

            }

            if (cartLog.CartLogId == 0)
            {
                await _context.CartLog.AddAsync(cartLog);
            }
            else
            {
                _context.CartLog.Update(cartLog);
            }

            await _context.SaveChangesAsync();
            return cartLog;
        }


        public async Task<bool> DeleteCartLogAsync(int cartLogId, int employeeId)
        {
            var cartLog = await _context.CartLog.FindAsync(cartLogId);
            if (cartLog != null && cartLog.EmployeeId == employeeId)
            {
                _context.CartLog.Remove(cartLog);
                await _context.SaveChangesAsync();
                return true;
            }
            return false; // Return false if deletion is not successful
        }
    }
}
