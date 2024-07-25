namespace HotelServices.Models.DTOs
{
    public class RoomDTO
    {
        public int HotelId { get; set; }   
        public int RoomNumber { get; set; }
        public int RoomType { get; set; }
        public int RoomFloor { get; set; }
        public int AllowedNumOfGuests { get; set; }
        public bool IsDeleted { get; set; }=false;
        public decimal Rent { get; set; }
    }
}
