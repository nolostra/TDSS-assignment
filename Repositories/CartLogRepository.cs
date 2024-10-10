using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using LinenManagementSystem.Models;
using LinenManagementSystem.Data;

namespace LinenManagementSystem.Repositories
{
    public class CartLogRepository(ApplicationDbContext context) : ICartLogRepository
    {
        private readonly ApplicationDbContext _context = context;

        public async Task<CartLog?> GetCartLogByIdAsync(int id)
        {
            return await _context.CartLogs
                .Include(c => c.Cart)
                .Include(l => l.Linens)
                .FirstOrDefaultAsync(cl => cl.CartLogId == id);
        }


        public async Task<IEnumerable<CartLog>> GetCartLogsAsync()
        {
            return await _context.CartLogs
                .Include(c => c.Cart)
                .Include(l => l.Linens)
                .ToListAsync();
        }

        public async Task UpsertCartLogAsync(CartLog cartLog)
        {
            if (cartLog.CartLogId == 0)
                _context.CartLogs.Add(cartLog);
            else
                _context.CartLogs.Update(cartLog);

            await _context.SaveChangesAsync();
        }

        public async Task DeleteCartLogAsync(int id)
        {
            var cartLog = await GetCartLogByIdAsync(id);
            if (cartLog != null)
            {
                _context.CartLogs.Remove(cartLog);
                await _context.SaveChangesAsync();
            }
        }
    }
}