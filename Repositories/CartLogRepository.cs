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
                CartLog cartLog;

                if (cartLogDto.CartLogId == 0) // If new CartLog
                {
                    // Create a new CartLog entity
                    cartLog = new CartLog
                    {
                        ReceiptNumber = cartLogDto.ReceiptNumber,
                        ReportedWeight = cartLogDto.ReportedWeight,
                        ActualWeight = cartLogDto.ActualWeight,
                        Comments = cartLogDto.Comments,
                        DateWeighed = cartLogDto.DateWeighed,
                        CartId = cartLogDto.CartId,
                        LocationId = cartLogDto.LocationId,
                        EmployeeId = cartLogDto.EmployeeId,
                    };

                    await _context.CartLog.AddAsync(cartLog);
                    await _context.SaveChangesAsync(); // Save to generate CartLogId

                    cartLogDto.CartLogId = cartLog.CartLogId; // Update DTO with generated ID
                }
                else
                {
                    // Check if the CartLog exists
                    cartLog = await _context.CartLog
                        .FirstOrDefaultAsync(cl => cl.CartLogId == cartLogDto.CartLogId);

                    if (cartLog == null)
                    {
                        throw new InvalidOperationException($"CartLog with ID {cartLogDto.CartLogId} does not exist.");
                    }

                    // Update properties of the existing CartLog
                    cartLog.ReceiptNumber = cartLogDto.ReceiptNumber;
                    cartLog.ReportedWeight = cartLogDto.ReportedWeight;
                    cartLog.ActualWeight = cartLogDto.ActualWeight;
                    cartLog.Comments = cartLogDto.Comments;
                    cartLog.DateWeighed = cartLogDto.DateWeighed;
                    cartLog.CartId = cartLogDto.CartId;
                    cartLog.LocationId = cartLogDto.LocationId;
                    cartLog.EmployeeId = cartLogDto.EmployeeId;

                    // Mark the entity as modified
                    _context.CartLog.Update(cartLog);
                }

                // Process CartLogDetails and Linen updates
                foreach (var item in cartLogDto.Linen)
                {
                    // If LinenId is 0, it's a new Linen entity
                    if (item.LinenId == 0)
                    {
                        var cartLogLinen = new Linen
                        {
                            Name = item.Name,
                            Weight = 0.00M, // You can set the actual weight if available
                        };
                        await _context.Linen.AddAsync(cartLogLinen);
                        await _context.SaveChangesAsync();

                        // Update LinenId in the DTO after saving (in case it's needed elsewhere)
                        item.LinenId = cartLogLinen.LinenId;
                    }
                    else
                    {
                        // Check if Linen exists
                        var existingLinen = await _context.Linen.FirstOrDefaultAsync(l => l.LinenId == item.LinenId);

                        if (existingLinen != null) // If Linen exists, update it
                        {
                            existingLinen.Name = item.Name;
                            // Update the weight if needed, or leave this line out if no changes
                            // existingLinen.Weight = newWeight;

                            _context.Linen.Update(existingLinen);
                            await _context.SaveChangesAsync();
                        }
                    }

                    // If CartLogDetailId is 0, it's a new CartLogDetail entity
                    if (item.CartLogDetailId == 0)
                    {
                        var cartLogDetail = new CartLogDetail
                        {
                            CartLogId = cartLog.CartLogId, // Use the CartLogId from the saved CartLog
                            LinenId = item.LinenId, // Use the LinenId (whether newly inserted or existing)
                            Count = item.Count,
                        };
                        await _context.CartLogDetail.AddAsync(cartLogDetail);
                        await _context.SaveChangesAsync();
                    }
                    else
                    {
                        // Check if CartLogDetail exists
                        var existingCartLogDetail = await _context.CartLogDetail
                            .FirstOrDefaultAsync(cld => cld.CartLogDetailId == item.CartLogDetailId);

                        if (existingCartLogDetail != null) // If CartLogDetail exists, update it
                        {
                            existingCartLogDetail.LinenId = item.LinenId;
                            existingCartLogDetail.Count = item.Count;

                            _context.CartLogDetail.Update(existingCartLogDetail);
                            await _context.SaveChangesAsync();
                        }
                    }
                }


                await _context.SaveChangesAsync();
                return cartLog;
            }
            catch (Exception ex)
            {
                // Log the error with stack trace for better debugging
                Console.Error.WriteLine($"Error occurred while upserting CartLog: {ex.Message}\n{ex.StackTrace}");
                throw; // Rethrow the exception to propagate
            }
        }





        public async Task<bool> DeleteCartLogAsync(int cartLogId, int employeeId)
        {
            // Find the CartLog by ID
            var cartLog = await _context.CartLog.FindAsync(cartLogId);

            // Check if the cartLog exists and belongs to the specified employee
            if (cartLog != null && cartLog.EmployeeId == employeeId)
            {
                // Get all associated CartLogDetails for the cartLogId
                var cartLogDetails = await _context.CartLogDetail
                    .Where(cld => cld.CartLogId == cartLogId)
                    .ToListAsync();

                if (cartLogDetails.Count != 0)
                {
                    // Remove CartLogDetails first (since they reference Linen)
                    foreach (var detail in cartLogDetails)
                    {
                        _context.CartLogDetail.Remove(detail);
                        await _context.SaveChangesAsync(); // Save after removing each detail
                    }

                    // Now remove the associated Linen entities after CartLogDetails are deleted
                    var linenIds = cartLogDetails.Select(cld => cld.LinenId).Distinct().ToList();
                    var linens = await _context.Linen
                        .Where(l => linenIds.Contains(l.LinenId))
                        .ToListAsync();

                    foreach (var linen in linens)
                    {
                        _context.Linen.Remove(linen);
                        await _context.SaveChangesAsync(); // Save after removing each linen
                    }
                }

                // Finally, remove the CartLog
                _context.CartLog.Remove(cartLog);

                // Save all changes in a single call
                await _context.SaveChangesAsync();

                return true; // Return true indicating successful deletion
            }

            return false; // Return false if deletion is not successful
        }



    }
}
