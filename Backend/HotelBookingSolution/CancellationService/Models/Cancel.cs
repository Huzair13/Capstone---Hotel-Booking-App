using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations.Schema;

namespace CancellationService.Models
{
    public class Cancel
    {
        public int Id { get; set; }
        public int BookingId { get; set; }
        public DateTime CancelledOn { get; set; }

        public int RefundId { get; set; }
        [ForeignKey("RefundId")]
        public Refund Refund { get; set; }
    }
}
