// DTOs/LinenDto.cs
namespace LinenManagementSystem.DTOs
{
    public class LinenDto
    {
        public int LinenId { get; set; }
        public required string Name { get; set; }
        public int Count { get; set; }
    }
}