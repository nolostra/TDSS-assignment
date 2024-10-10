namespace LinenManagementSystem.Models
{
     public class Cart
    {
        public int CartId { get; set; }
        public required string Name { get; set; }
        public double Weight { get; set; }
        public required string Type { get; set; }
    }
}