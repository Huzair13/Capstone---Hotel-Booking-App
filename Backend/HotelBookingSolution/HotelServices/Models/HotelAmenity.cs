namespace HotelServices.Models
{
    public class HotelAmenity
    {
        public int Id { get; set; }
        public int HotelId { get; set; }
        public Hotel Hotel { get; set; }

        public int AmenityId { get; set; }
        public Amenity Amenity { get; set; }
    }
}
