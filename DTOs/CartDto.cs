// DTOs/CartDto.cs

namespace LinenManagementSystem.DTOs
{
    public class CartDto
    {
        public int CartId { get; set; }
        public required string Name { get; set; }
        public int Weight { get; set; }
        public required string Type { get; set; }
    }
}