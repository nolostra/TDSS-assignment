// DTOs/LinenDto.cs
namespace LinenManagementSystem.DTOs
{
    public class LinenDtoFetch
    {
        public int LinenId { get; set; }
        public required string Name { get; set; }
        public int Count { get; set; }
        public int CartLogDetailId { get; set; }
    }
}