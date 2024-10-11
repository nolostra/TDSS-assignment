using LinenManagementSystem.DTOs;
using LinenManagementSystem.Models; // Use Models instead of DTOs

namespace LinenManagementSystem.Repositories
{
    public interface ICartLogRepository
    {
        Task<CartLog> GetCartLogByIdAsync(int cartLogId);
        Task<IEnumerable<CartLog>> GetCartLogsAsync(string cartType, string location, int? employeeId);
        Task<CartLog> UpsertCartLogAsync(CartLog cartLog);
        Task<bool> DeleteCartLogAsync(int cartLogId, int employeeId);
    }

}
