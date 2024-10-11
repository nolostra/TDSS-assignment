using System.ComponentModel.DataAnnotations;
namespace LinenManagementSystem.Models
{
    public class Employees
    {
        [Key]
        public required int EmployeeId { get; set; }
        public required string Name { get; set; }
        public required string Email { get; set; }
        public required string Password { get; set; }
        public string? RefreshToken { get; set; } // Nullable string to represent nvarchar(max)
    }
}
