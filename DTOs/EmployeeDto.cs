// DTOs/EmployeeDto.cs
namespace LinenManagementSystem.DTOs
{
    public class EmployeeDto
    {
        public int EmployeeId { get; set; }
        public required string Name { get; set; }
        public required string Email { get; set; }
        public required string Password { get; set; }
        public required string RefreshToken { get; set; }
    }
}

