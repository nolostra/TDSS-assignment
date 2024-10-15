using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; // Required for [ForeignKey]

namespace LinenManagementSystem.Models
{
    public class CartLogDetail
    {
        [Key]
        public int CartLogDetailId { get; set; }


        public int CartLogId { get; set; }


        public int LinenId { get; set; }

        public int Count { get; set; }
    }
}
