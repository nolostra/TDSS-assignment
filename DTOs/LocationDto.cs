// DTOs/LocationDto.cs
namespace LinenManagementSystem.DTOs
{
    public class LocationDto
    {
        public int LocationId { get; set; }
        public required string Name { get; set; }
        public required string Type { get; set; }
    }
}