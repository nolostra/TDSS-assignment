using LinenManagementSystem.DTOs;
using LinenManagementSystem.Repositories;

namespace LinenManagementSystem.Services
{
    public interface IEmployeeService
    {
        Task<EmployeeDto?> GetEmployeeByIdAsync(int employeeId);
    }
    public class EmployeeService(IEmployeeRepository EmployeeRepository) : IEmployeeService
    {

        private readonly IEmployeeRepository _employeeRepository = EmployeeRepository;


        public Task<EmployeeDto?> GetEmployeeByIdAsync(int employeeId)
        {
            return _employeeRepository.GetEmployeeByIdAsync(employeeId);
        }
    }
}