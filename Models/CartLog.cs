namespace LinenManagementSystem.Models
{
    public class CartLog
    {
        public int CartLogId { get; set; }
        public required string ReceiptNumber { get; set; }
        public double ReportedWeight { get; set; }
        public double ActualWeight { get; set; }
        public required string Comments { get; set; }
        public DateTime DateWeighed { get; set; }

        public int CartId { get; set; }
        public required Cart Cart { get; set; }

        public int LocationId { get; set; }
        public required Location Location { get; set; }

        public int EmployeeId { get; set; }
        public required Employee Employee { get; set; }

        public required ICollection<Linen> Linens { get; set; }
    }

}