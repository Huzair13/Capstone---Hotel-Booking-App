namespace HotelServices.Models.DTOs
{
    public class RoomDTO
    {
        public int RoomNumber { get; set; }
        public int RoomType { get; set; }
        public int RoomFloor { get; set; }
        public bool IsDeleted { get; set; }
        public int AllowedNumOfGuests { get; set; }
        public int HotelId { get; set; }
        public decimal Rent { get; set; }
    }
}
