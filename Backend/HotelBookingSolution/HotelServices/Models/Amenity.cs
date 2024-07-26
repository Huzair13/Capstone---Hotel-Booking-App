using System.ComponentModel.DataAnnotations;

namespace HotelServices.Models
{
    public class Amenity
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public ICollection<HotelAmenity> HotelAmenities { get; set; }
    }
}
