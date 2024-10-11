using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; // Required for [ForeignKey]

namespace LinenManagementSystem.Models
{
    public class CartLogDetail
    {
        [Key]
        public int CartLogDetailId { get; set; }

        // Foreign key for CartLog
        [ForeignKey("CartLog")]
        public int CartLogId { get; set; }

        // Foreign key for Linen
        [ForeignKey("Linen")]
        public int LinenId { get; set; }

        public int Count { get; set; }
    }
}
