using LinenManagementSystem.Models; // Use Models instead of DTOs

namespace LinenManagementSystem.Repositories
{
    public interface ICartLogRepository
    {
        Task<CartLog?> GetCartLogByIdAsync(int id);  // Use the nullable type if you expect null values
        Task<IEnumerable<CartLog>> GetCartLogsAsync(); // No need for nullable in IEnumerable
        Task UpsertCartLogAsync(CartLog cartLog);
        Task DeleteCartLogAsync(int id);
    }
}
