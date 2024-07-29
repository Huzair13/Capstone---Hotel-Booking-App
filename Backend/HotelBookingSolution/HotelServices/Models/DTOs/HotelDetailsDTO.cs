namespace HotelServices.Models.DTOs
{
    public class HotelDetailsDTO
    {
        public List<HotelReturnDTO> Hotel { get; set; }
        public List<RoomDTO> Rooms { get; set; }
        public List<AmenityDTO> Amenities { get; set; }
    }

}
