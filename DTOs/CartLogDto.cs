namespace LinenManagementSystem.DTOs
{
    public class CartLog
    {
        public int CartLogId { get; set; }
        public required string ReceiptNumber { get; set; }
        public double ReportedWeight { get; set; }
        public double ActualWeight { get; set; }
        public required string Comments { get; set; }
        public DateTime DateWeighed { get; set; }

        // Complex types: Cart, Location, Employee
        public required CartDto Cart { get; set; }
        public required LocationDto Location { get; set; }
        public required EmployeeDto Employee { get; set; }

        // Initialize the Linens collection to avoid null reference issues
        public required ICollection<LinenDto> Linens { get; set; } = [];
    }
}
