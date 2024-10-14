using System.ComponentModel.DataAnnotations; 
namespace LinenManagementSystem.Models
{
     public class Carts
    {
        [Key]
        public required int CartId { get; set; }
        public required string Name { get; set; }
        public required int Weight { get; set; }
        public required string Type { get; set; }
    }
}