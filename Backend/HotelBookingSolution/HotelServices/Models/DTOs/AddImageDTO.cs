namespace HotelServices.Models.DTOs
{
    public class AddImageDTO
    {
        public int hotelId {  get; set; } 
        public List<string> imageUrls { get; set; }
    }
}
