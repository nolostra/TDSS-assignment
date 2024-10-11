namespace LinenManagementSystem.DTOs
{
    public class CartLogFetch
    {
        public int CartLogId { get; set; }
        public required string ReceiptNumber { get; set; }
        public double ReportedWeight { get; set; }
        public double ActualWeight { get; set; }
        public required string Comments { get; set; }
        public DateTime DateWeighed { get; set; }

        // Complex types: Cart, Location, Employee
        public  CartDto? Cart { get; set; }
        public  LocationDto? Location { get; set; }
        public  EmployeeDtoFetch? Employee { get; set; }

        // Initialize the Linens collection to avoid null reference issues
        public required ICollection<LinenDtoFetch> Linen { get; set; } = [];
    }

     public class CartLogInsert
    {
        public int CartLogId { get; set; }
        public required string ReceiptNumber { get; set; }
        public double ReportedWeight { get; set; }
        public double ActualWeight { get; set; }
        public required string Comments { get; set; }
        public DateTime DateWeighed { get; set; }

        // Complex types: Cart, Location, Employee
        public required int CartId { get; set; }
        public required int LocationId { get; set; }
        public required int EmployeeId { get; set; }

        // Initialize the Linens collection to avoid null reference issues
        public required ICollection<LinenDtoFetch> Linen { get; set; } = [];
    }
}
