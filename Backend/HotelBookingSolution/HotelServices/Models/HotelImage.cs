using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace HotelServices.Models
{
    public class HotelImage
    {
        [Key]
        public int Id { get; set; }

        public string ImageUrl { get; set; }

        public int HotelId { get; set; }
        public Hotel Hotel { get; set; }
    }
}
