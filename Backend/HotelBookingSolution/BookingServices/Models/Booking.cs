namespace BookingServices.Models
{
    public class Booking
    {
        public int Id { get; set; }
        public int HotelId { get; set; }
        public int RoomNumber { get; set; } 
        public int UserId { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public int NumberOfGuests { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
