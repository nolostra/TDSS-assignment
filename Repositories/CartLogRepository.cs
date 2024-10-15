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
        Task<IEnumerable<CartLogFetch?>> GetCartLogsAsync(string? cartType, string? location, int? employeeId);
        Task<CartLog> UpsertCartLogAsync(CartLogInsert cartLog);
        Task<bool> DeleteCartLogAsync(int cartLogId, int employeeId);
    }
    public class CartLogRepository : ICartLogRepository
    {
        private readonly ApplicationDbContext _context;

        public CartLogRepository(ApplicationDbContext context) // Corrected this line
        {
            _context = context;
        }

        public async Task<CartLogFetch?> GetCartLogByIdAsync(int cartLogId)
        {
            var cartLog = await _context.CartLog
                .Where(cl => cl.CartLogId == cartLogId) // Filter by cartLogId
                .Select(cl => new CartLogFetch
                {
                    CartLogId = cl.CartLogId,
                    ReceiptNumber = cl.ReceiptNumber,
                    DateWeighed = cl.DateWeighed,
                    Employee = _context.Employees
                        .Where(e => e.EmployeeId == cl.EmployeeId)
                        .Select(e => new EmployeeDtoFetch
                        {
                            EmployeeId = e.EmployeeId,
                            Name = e.Name ?? "Unknown" // Handle potential null case
                        })
                        .FirstOrDefault(),
                    Location = _context.Locations
                        .Where(e => e.LocationId == cl.LocationId)
                        .Select(e => new LocationDto
                        {
                            LocationId = e.LocationId,
                            Name = e.Name ?? "Unknown", // Handle null case
                            Type = e.Type ?? "Unknown"  // Handle null case
                        })
                        .FirstOrDefault(),
                    ReportedWeight = cl.ReportedWeight ?? 0,
                    ActualWeight = cl.ActualWeight,
                    Comments = cl.Comments ?? "No comments", // Default comment if null
                    Cart = _context.Carts
                        .Where(e => e.CartId == cl.CartId)
                        .Select(e => new CartDto
                        {
                            CartId = e.CartId,
                            Type = e.Type ?? "Unknown", // Handle null case
                            Weight = e.Weight,
                            Name = e.Name ?? "Unknown" // Handle null case
                        })
                        .FirstOrDefault(),
                    Linen = _context.CartLogDetail
                        .Where(d => d.CartLogId == cl.CartLogId)
                        .Join(_context.Linen,
                            detail => detail.LinenId,
                            linen => linen.LinenId,
                            (detail, linen) => new LinenDtoFetch
                            {
                                CartLogDetailId = detail.CartLogDetailId,
                                LinenId = linen.LinenId,
                                Name = linen.Name ?? "Unknown", // Handle null case
                                Count = detail.Count
                            })
                        .ToList() // Convert inner query results to List
                })
                .FirstOrDefaultAsync(); // Get the first matching CartLog

            return cartLog;
        }



        public async Task<IEnumerable<CartLogFetch?>> GetCartLogsAsync(string? cartType, string? location, int? employeeId)
        {
            var query = _context.CartLog.AsQueryable();




            // Fetch CartLogs and project them into CartLogFetch DTOs
            var cartLogs = await query
    .OrderByDescending(cl => cl.DateWeighed)
    .Where(cl => _context.Employees.Any(e => e.EmployeeId == cl.EmployeeId && (employeeId == null || e.EmployeeId == employeeId))) // Filter out logs without valid Employee
    .Where(cl => _context.Locations.Any(l => l.LocationId == cl.LocationId && (location == null || l.Name == location))) // Filter out logs without valid Location
    .Where(cl => _context.Carts.Any(c => c.CartId == cl.CartId && (cartType == null || c.Type == cartType))) // Filter out logs without valid Cart
    .Select(cl => new CartLogFetch
    {
        CartLogId = cl.CartLogId,
        ReceiptNumber = cl.ReceiptNumber,
        DateWeighed = cl.DateWeighed,
        Employee = _context.Employees
            .Where(e => e.EmployeeId == cl.EmployeeId && (employeeId == null || e.EmployeeId == employeeId))
            .Select(e => new EmployeeDtoFetch
            {
                EmployeeId = e.EmployeeId,
                Name = e.Name ?? "Unknown" // Handle potential null case
            })
            .FirstOrDefault(),
        Location = _context.Locations
            .Where(e => e.LocationId == cl.LocationId && (location == null || e.Name == location))
           .Select(e => new LocationDto
           {
               LocationId = e.LocationId,
               Name = e.Name, // Handle null case
               Type = e.Type // Handle null case
           }).FirstOrDefault(),
        ReportedWeight = cl.ReportedWeight ?? 0,
        ActualWeight = cl.ActualWeight,
        Comments = cl.Comments ?? "No comments", // Fixed quote
        Cart = _context.Carts
            .Where(e => e.CartId == cl.CartId && (cartType == null || e.Type == cartType))
           .Select(e => new CartDto
           {
               CartId = e.CartId,
               Type = e.Type,
               Weight = e.Weight,
               Name = e.Name ?? "Unknown"
           }).FirstOrDefault(),
        Linen = _context.CartLogDetail
             .Where(d => d.CartLogId == cl.CartLogId)
             .Join(_context.Linen,
                 detail => detail.LinenId,
                 linen => linen.LinenId,
                 (detail, linen) => new LinenDtoFetch
                 {
                     CartLogDetailId = detail.CartLogDetailId,
                     LinenId = linen.LinenId,
                     Name = linen.Name ?? "Unknown", // Handle null case
                     Count = detail.Count
                 })
             .ToList() // Convert inner query results to List
    })
    .ToListAsync();


            return cartLogs;
        }





        public async Task<CartLog> UpsertCartLogAsync(CartLogInsert cartLogDto)
        {
            try
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

                if (cartLogDto.CartLogId == 0) // If new CartLog
                {

                    await _context.CartLog.AddAsync(cartLog);
                    await _context.SaveChangesAsync(); // Save to generate CartLogId
                }
                else
                {
                    // Check if the CartLog exists
                    var existingCartLog = await _context.CartLog
    .FirstOrDefaultAsync(cl => cl.CartLogId == cartLogDto.CartLogId);

                    if (existingCartLog == null)
                    {
                        throw new InvalidOperationException($"CartLog with ID {cartLogDto.CartLogId} does not exist.");
                    }

                    // Update properties of the existing CartLog
                    existingCartLog.ReceiptNumber = cartLogDto.ReceiptNumber;
                    existingCartLog.ReportedWeight = cartLogDto.ReportedWeight;
                    existingCartLog.ActualWeight = cartLogDto.ActualWeight;
                    existingCartLog.Comments = cartLogDto.Comments;
                    existingCartLog.DateWeighed = cartLogDto.DateWeighed;
                    existingCartLog.CartId = cartLogDto.CartId;
                    existingCartLog.LocationId = cartLogDto.LocationId;
                    existingCartLog.EmployeeId = cartLogDto.EmployeeId;

                    // Mark the entity as modified
                    _context.CartLog.Update(existingCartLog);
                }

                foreach (var item in cartLogDto.Linen)
                {
                    // Check if CartLogDetail exists
                    var existingCartLogDetail = await _context.CartLogDetail
                        .FirstOrDefaultAsync(cld => cld.CartLogDetailId == item.CartLogDetailId);

                    if (existingCartLogDetail == null) // If CartLogDetail doesn't exist, add it
                    {
                        var cartLogDetail = new CartLogDetail
                        {
                            CartLogId = cartLogDto.CartLogId, // Use the existing CartLogId
                            LinenId = item.LinenId,
                            Count = item.Count,
                        };
                        await _context.CartLogDetail.AddAsync(cartLogDetail);
                    }
                    else // If CartLogDetail exists, update it
                    {
                        existingCartLogDetail.CartLogId = cartLogDto.CartLogId; // Ensure it's set correctly
                        existingCartLogDetail.LinenId = item.LinenId;
                        existingCartLogDetail.Count = item.Count;

                        _context.CartLogDetail.Update(existingCartLogDetail);
                    }

                    // Check if Linen exists
                    var existingLinen = await _context.Linen
                        .FirstOrDefaultAsync(l => l.LinenId == item.LinenId);

                    if (existingLinen == null) // If Linen doesn't exist, add it
                    {
                        var cartLogLinen = new Linen
                        {
                            LinenId = item.LinenId,
                            Name = item.Name,
                            Weight = 0.00M, // You can set the actual weight if available
                        };
                        await _context.Linen.AddAsync(cartLogLinen);
                    }
                    else // If Linen exists, update it
                    {
                        existingLinen.Name = item.Name;
                        existingLinen.Weight = existingLinen.Weight; // Assuming weight isn't changing here

                        _context.Linen.Update(existingLinen);
                    }
                }

                await _context.SaveChangesAsync();
                return cartLog;
            }
            catch (Exception ex)
            {
                // Log the error
                Console.Error.WriteLine($"Error occurred while upserting CartLog: {ex.Message}");
                throw; // Rethrow the exception to propagate
            }
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
