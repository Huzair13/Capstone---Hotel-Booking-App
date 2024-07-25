namespace BookingServices.Models.DTOs
{
    public class BookingReturnDTO
    {
        public int BookingId { get; set; }
        public int HotelId { get; set; }
        public int UserId { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public int NumberOfGuests { get; set; }
        public decimal TotalPrice { get; set; }
        public List<int> RoomNumbers { get; set; } // List of room numbers
    }

}
