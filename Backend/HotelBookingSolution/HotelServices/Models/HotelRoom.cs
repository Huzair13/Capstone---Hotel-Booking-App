namespace HotelServices.Models
{
    public class HotelRoom
    {
        public int Id { get; set; }

        public int RoomID { get; set; }
        public Room Room { get; set; }

        public int HotelID {  get; set; }
        public Hotel Hotel { get; set; }
    }
}
