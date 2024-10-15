using System.ComponentModel.DataAnnotations;
namespace LinenManagementSystem.Models
{
    public class Linen
    {
        [Key]
        public required int LinenId { get; set; }
        public required string Name { get; set; }
        public required decimal Weight { get; set; }
    }
}