namespace BookingServices.Models.DTOs
{
    public class BestCombinationDTO
    {
        public int HotelId {  get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public int NumOfGuests { get; set; }
        public int NumOfRooms { get; set; }
    }
}
