namespace BookingServices.Models.DTOs
{
    public class BookingMultipleRoomsRequestDTO
    {
        public int userId {  get; set; }
        public int HotelId { get; set; }
        public List<int> RoomNumbers { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public int NumberOfGuests { get; set; }
    }
}
