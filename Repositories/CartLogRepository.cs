using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using LinenManagementSystem.Models;
using LinenManagementSystem.Data;
using LinenManagementSystem.DTOs;

namespace LinenManagementSystem.Repositories
{
    public interface ICartLogRepository
    {
        Task<CartLogFetch?> GetCartLogByIdAsync(int cartLogId);
        Task<IEnumerable<CartLog>> GetCartLogsAsync(string cartType, string location, int? employeeId);
        Task<CartLog> UpsertCartLogAsync(CartLogInsert cartLog);
        Task<bool> DeleteCartLogAsync(int cartLogId, int employeeId);
    }
    public class CartLogRepository(ApplicationDbContext context) : ICartLogRepository
    {
        private readonly ApplicationDbContext _context = context;

        public async Task<CartLogFetch?> GetCartLogByIdAsync(int cartLogId)
        {
            // Fetch the CartLog based on the cartLogId
            var cartLog = await _context.CartLog
                .FirstOrDefaultAsync(cl => cl.CartLogId == cartLogId);

            // If no CartLog is found, return null
            if (cartLog == null)
            {
                return null;
            }

            // Fetch related entities
            var cart = await _context.Carts
                .FirstOrDefaultAsync(c => c.CartId == cartLog.CartId);

            var location = await _context.Locations
                .FirstOrDefaultAsync(l => l.LocationId == cartLog.LocationId);

            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.EmployeeId == cartLog.EmployeeId);

            // Fetch CartLogDetails with associated Linens
            var linenDetails = await (from detail in _context.CartLogDetails
                                      join linen in _context.Linens on detail.LinenId equals linen.LinenId
                                      where detail.CartLogId == cartLogId
                                      select new LinenDtoFetch
                                      {
                                          CartLogDetailId = detail.CartLogDetailId,
                                          LinenId = detail.LinenId,
                                          Name = linen.Name, // Assuming Linen has a Name property
                                          Count = detail.Count
                                      }).ToListAsync();

            // Return the combined results
            return new CartLogFetch
            {
                CartLogId = cartLog.CartLogId,
                ReceiptNumber = cartLog.ReceiptNumber,
                DateWeighed = cartLog.DateWeighed,
                Employee = employee != null ? new EmployeeDtoFetch
                {
                    EmployeeId = employee.EmployeeId,
                    Name = employee.Name,

                } : null,
                Location = location != null ? new LocationDto
                {
                    LocationId = location.LocationId,
                    Name = location.Name,
                    Type = location.Type
                } : null,
                ReportedWeight = cartLog.ReportedWeight,
                ActualWeight = cartLog.ActualWeight,
                Comments = cartLog.Comments,
                Cart = cart != null ? new CartDto
                {
                    CartId = cart.CartId,
                    Type = cart.Type,
                    Weight = cart.Weight,
                    Name = cart.Name
                } : null,
                Linen = linenDetails
            };
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
                if (item.CartLogDetailId == 0)
                {
                    await _context.CartLogDetails.AddAsync(cartLogDetail);
                }
                else
                {
                    _context.CartLogDetails.Update(cartLogDetail);
                }



                var cartLogLinen = new Linen
                {
                    LinenId = item.LinenId,
                    Name = item.Name,
                    Weight = 0,
                };
                if (item.LinenId == 0)
                {
                    await _context.Linens.AddAsync(cartLogLinen);
                }
                else
                {
                    _context.Linens.Update(cartLogLinen);
                }

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
