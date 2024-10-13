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
        Task<IEnumerable<CartLogFetch?>> GetCartLogsAsync(string cartType, string location, int? employeeId);
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


        public async Task<IEnumerable<CartLogFetch?>> GetCartLogsAsync(string cartType, string location, int? employeeId)
        {
            // Start the query for either fetching by ID or multiple records
            var query = _context.CartLog.AsQueryable();

            // If cartLogId is provided, fetch specific CartLog by ID


            // Apply filtering for cartType, location, and employeeId
            if (!string.IsNullOrEmpty(cartType))
            {
                query = query.Where(cl => cl.Cart.Type == cartType);
            }

            if (!string.IsNullOrEmpty(location))
            {
                query = query.Where(cl => cl.Location.Name == location);
            }

            if (employeeId.HasValue)
            {
                query = query.Where(cl => cl.EmployeeId == employeeId.Value);
            }

            // Fetch CartLogs and project them into CartLogFetch DTOs
            var cartLogs = await query
                .OrderByDescending(cl => cl.DateWeighed)
                .Select(cl => new CartLogFetch
                {
                    CartLogId = cl.CartLogId,
                    ReceiptNumber = cl.ReceiptNumber,
                    DateWeighed = cl.DateWeighed,
                    Employee = cl.Employee != null ? new EmployeeDtoFetch
                    {
                        EmployeeId = cl.Employee.EmployeeId,
                        Name = cl.Employee.Name,
                    } : null,
                    Location = cl.Location != null ? new LocationDto
                    {
                        LocationId = cl.Location.LocationId,
                        Name = cl.Location.Name,
                        Type = cl.Location.Type
                    } : null,
                    ReportedWeight = cl.ReportedWeight,
                    ActualWeight = cl.ActualWeight,
                    Comments = cl.Comments,
                    Cart = cl.Cart != null ? new CartDto
                    {
                        CartId = cl.Cart.CartId,
                        Type = cl.Cart.Type,
                        Weight = cl.Cart.Weight,
                        Name = cl.Cart.Name
                    } : null,
                    Linen = _context.CartLogDetails
                        .Where(d => d.CartLogId == cl.CartLogId)
                        .Join(_context.Linens,
                            detail => detail.LinenId,
                            linen => linen.LinenId,
                            (detail, linen) => new LinenDtoFetch
                            {
                                CartLogDetailId = detail.CartLogDetailId,
                                LinenId = linen.LinenId,
                                Name = linen.Name, // Assuming Linen has a Name property
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
