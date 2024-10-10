namespace LinenManagementSystem.Models
{
    public class Linen
    {
        public int LinenId { get; set; }
        public required string Name { get; set; }
        public int Count { get; set; }
    }
}