using LinenManagementSystem.Data;
using LinenManagementSystem.DTOs;

namespace LinenManagementSystem.Repositories
{
    public interface IEmployeeRepository
    {
        Task<EmployeeDto?> GetEmployeeByIdAsync(int employeeId);
    }

    public class EmployeeRepository(ApplicationDbContext context) : IEmployeeRepository
    {
        private readonly ApplicationDbContext _context = context;

        public async Task<EmployeeDto?> GetEmployeeByIdAsync(int employeeId)
        {
            var employee = await _context.Employees
                    .FindAsync(employeeId);
            if (employee == null) return null;
            return new EmployeeDto
            {
                EmployeeId = employee.EmployeeId,
                Name = employee.Name,
                Email = employee.Email,
                RefreshToken = employee.RefreshToken,
            };
        }
    }
}