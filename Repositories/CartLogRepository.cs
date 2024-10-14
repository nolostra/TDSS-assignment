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
            var linenDetails = await (from detail in _context.CartLogDetail
                                      join linen in _context.Linen on detail.LinenId equals linen.LinenId
                                      where detail.CartLogId == cartLogId
                                      select new LinenDtoFetch
                                      {
                                          CartLogDetailId = detail.CartLogDetailId,
                                          LinenId = detail.LinenId,
                                          Name = linen.Name, // Assuming Linen has a Name property
                                          Count = detail.Count
                                      }).DefaultIfEmpty().ToListAsync();

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
                ReportedWeight = cartLog.ReportedWeight ?? 0,
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


        public async Task<IEnumerable<CartLogFetch?>> GetCartLogsAsync(string? cartType, string? location, int? employeeId)
        {
            var query = _context.CartLog.AsQueryable();

            // Apply filtering for cartType, location, and employeeId
            // if (!string.IsNullOrEmpty(cartType))
            // {
            //     query = query.Where(cl => cl.Cart.Type == cartType);
            // }

            // if (!string.IsNullOrEmpty(location))
            // {
            //     query = query.Where(cl => cl.Location.Name == location);
            // }

            // if (employeeId.HasValue)
            // {
            //     query = query.Where(cl => cl.EmployeeId == employeeId.Value);
            // }
            try
            {
                var dummyData = await query.ToListAsync();

                // Check if dummyData is not null or empty
                if (dummyData != null && dummyData.Any())
                {
                    // Iterate through each item in the list and print its properties
                    foreach (var cartLog in dummyData)
                    {
                        Console.WriteLine($"CartLogId: {cartLog.CartLogId}, " +
                                          $"ReceiptNumber: {cartLog.ReceiptNumber}, " +
                                          $"DateWeighed: {cartLog.DateWeighed}, " +
                                          $"ReportedWeight: {cartLog.ReportedWeight}, " +
                                          $"ActualWeight: {cartLog.ActualWeight}, " +
                                          $"Comments: {cartLog.Comments ?? "No comments"}");


                    }
                }
                else
                {
                    Console.WriteLine("No cart logs found.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }



            // Fetch CartLogs and project them into CartLogFetch DTOs
            var cartLogs = await query
     .OrderByDescending(cl => cl.DateWeighed)
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
               Name = e.Name, // Handle null case
               Type = e.Type // Handle null case
           }).FirstOrDefault(),
         ReportedWeight = cl.ReportedWeight ?? 0,
         ActualWeight = cl.ActualWeight,
         Comments = cl.Comments ?? "No comments", // Fixed quote
         Cart = _context.Carts
            .Where(e => e.CartId == cl.CartId)
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
                    await _context.CartLogDetail.AddAsync(cartLogDetail);
                }
                else
                {
                    _context.CartLogDetail.Update(cartLogDetail);
                }



                var cartLogLinen = new Linen
                {
                    LinenId = item.LinenId,
                    Name = item.Name,
                    Weight = 0,
                };
                if (item.LinenId == 0)
                {
                    await _context.Linen.AddAsync(cartLogLinen);
                }
                else
                {
                    _context.Linen.Update(cartLogLinen);
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
