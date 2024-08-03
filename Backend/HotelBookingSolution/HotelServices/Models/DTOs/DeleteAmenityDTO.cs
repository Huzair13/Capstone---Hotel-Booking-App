namespace HotelServices.Models.DTOs
{
    public class DeleteAmenityDTO
    {
        public int HotelID { get; set; }
        public List<int> AmenityIds { get; set; }

    }
}
