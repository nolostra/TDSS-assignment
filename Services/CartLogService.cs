using System;
using System.Threading.Tasks;
using LinenManagementSystem.Repositories;
using LinenManagementSystem.Models;
using LinenManagementSystem.DTOs;

namespace LinenManagementSystem.Services
{
    public interface ICartLogService
    {
        Task<CartLog> GetCartLogByIdAsync(int cartLogId);
        Task<IEnumerable<CartLog>> GetCartLogsAsync(string cartType, string location, int? employeeId);
        Task<CartLog> UpsertCartLogAsync(CartLog cartLog, int employeeId);
        Task<bool> DeleteCartLogAsync(int cartLogId, int employeeId);
    }

    public class CartLogService : ICartLogService
    {
        private readonly ICartLogRepository _cartLogRepository;

        public CartLogService(ICartLogRepository cartLogRepository)
        {
            _cartLogRepository = cartLogRepository;
        }

        public Task<CartLog> GetCartLogByIdAsync(int cartLogId)
        {
            return _cartLogRepository.GetCartLogByIdAsync(cartLogId);
        }

        public Task<IEnumerable<CartLog>> GetCartLogsAsync(string cartType, string location, int? employeeId)
        {
            return _cartLogRepository.GetCartLogsAsync(cartType, location, employeeId);
        }

        public async Task<CartLog> UpsertCartLogAsync(CartLog cartLog, int employeeId)
        {
            // Ensure that only the creator can update the log
            if (cartLog.CartLogId != 0 && cartLog.EmployeeId != employeeId)
            {
                throw new UnauthorizedAccessException("You do not have permission to update this cart log.");
            }
            return await _cartLogRepository.UpsertCartLogAsync(cartLog);
        }

        public Task<bool> DeleteCartLogAsync(int cartLogId, int employeeId)
        {
            return _cartLogRepository.DeleteCartLogAsync(cartLogId, employeeId);
        }
    }

}