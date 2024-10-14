using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; // Required for [ForeignKey]

namespace LinenManagementSystem.Models
{
    public class CartLog
    {
        [Key]
        public int CartLogId { get; set; }

        public string? ReceiptNumber { get; set; }
        public int? ReportedWeight { get; set; }
        public int ActualWeight { get; set; }
        public string? Comments { get; set; }
        public DateTime DateWeighed { get; set; }


        public int CartId { get; set; }

        public int LocationId { get; set; }


        public int EmployeeId { get; set; }


    }
}
