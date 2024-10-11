using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; // Required for [ForeignKey]

namespace LinenManagementSystem.Models
{
    public class CartLog
    {
        [Key]
        public int CartLogId { get; set; }

        public required string ReceiptNumber { get; set; }
        public double ReportedWeight { get; set; }
        public double ActualWeight { get; set; }
        public required string Comments { get; set; }
        public DateTime DateWeighed { get; set; }

        // Foreign keys with navigation properties
        [ForeignKey("Cart")]
        public int CartId { get; set; }

        [ForeignKey("Location")]
        public int LocationId { get; set; }

        [ForeignKey("Employee")]
        public int EmployeeId { get; set; }

        public virtual Carts Cart { get; set; } // Use 'virtual' for lazy loading
        public virtual Location Location { get; set; }
        public virtual Employees Employee { get; set; }
    }
}
