using System.ComponentModel.DataAnnotations;
namespace LinenManagementSystem.Models
{
    public class Linen
    {
        [Key]
        public required int LinenId { get; set; }
        public required string Name { get; set; }
        public required double Weight { get; set; }
    }
}