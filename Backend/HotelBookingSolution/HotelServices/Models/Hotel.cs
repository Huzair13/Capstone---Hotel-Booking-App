using System.ComponentModel.DataAnnotations;

namespace HotelServices.Models
{
    public class Hotel
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Type { get; set; }
        public int NumOfRooms { get; set; }
        public int AverageRatings { get; set; }
        public string Description { get; set; }
        public bool IsDeleted { get; set; } = false;
        public ICollection<HotelImage> HotelImages { get; set; }
        public ICollection<Room> Rooms { get; set; } = new List<Room>();
    }
}
