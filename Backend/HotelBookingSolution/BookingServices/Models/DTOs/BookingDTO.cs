namespace BookingServices.Models.DTOs
{
    public class BookingDTO
    {
        public int HotelId { get; set; }
        public int UserId { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public int NumberOfRooms { get; set; }
        public int NumberOfGuests { get; set; }
        public int? RoomNumber { get; set; }
    }
}
