namespace BookingServices.Models
{
    public class BookingDetail
    {
        public int Id { get; set; }
        public int BookingId { get; set; } // Foreign key to Booking
        public int RoomNumber { get; set; }
        public decimal Rent { get; set; }
        public int HotelId { get; set; }
        public Booking Booking { get; set; } // Navigation property for Booking
    }

}
