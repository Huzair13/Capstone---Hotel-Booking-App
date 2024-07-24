namespace HotelServices.Models.DTOs
{
    public class RoomReturnDTO
    {
        public int RoomNumber { get; set; }
        public int RoomType { get; set; }
        public int RoomFloor { get; set; }
        public int AllowedNumOfGuests { get; set; }
        public decimal Rent { get; set; }
    }
}
