namespace HotelServices.Models.DTOs
{
    public class AddAmenitiesToHotelDTO
    {
        public int HotelID { get; set; }
        public List<int> AmenityIds { get; set; }
    }
}
