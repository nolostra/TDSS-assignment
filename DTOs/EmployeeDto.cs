// DTOs/EmployeeDto.cs
using LinenManagementSystem.Models;

namespace LinenManagementSystem.DTOs
{
    public class EmployeeDto
    {
        public int EmployeeId { get; set; }
        public required string Name { get; set; }
        public required string Email { get; set; }
        public string? RefreshToken { get; set; }

    }


    public class EmployeeDtoFetch
    {
        public int EmployeeId { get; set; }
        public required string Name { get; set; }
    }
}

