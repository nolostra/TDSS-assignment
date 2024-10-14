using System.ComponentModel.DataAnnotations; 
namespace LinenManagementSystem.Models
{
    public class Locations
    {
        [Key]
        public required int LocationId { get; set; }
        public required string Name { get; set; }
        public required string Type { get; set; }
    }
}