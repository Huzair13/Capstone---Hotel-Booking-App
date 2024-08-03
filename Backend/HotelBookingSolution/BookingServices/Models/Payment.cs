using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.InteropServices;

namespace BookingServices.Models
{
    public class Payment
    {
        [Key]
        public int Id { get; set; }

        public int BookingId { get; set; }
        [ForeignKey("BookingId")]
        public Booking Booking { get; set; }   

        public decimal Amount { get; set; }
        public PaymentMode PaymentMode { get; set; }
    }
}
