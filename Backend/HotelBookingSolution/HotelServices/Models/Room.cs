using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.InteropServices;

namespace HotelServices.Models
{
    public class Room
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int RoomNumber { get; set; }
        public int RoomType { get; set; }
        public int RoomFloor { get; set; }
        public int AllowedNumOfGuests { get; set; }
        public decimal Rent {  get; set; }
        public bool IsDeleted { get; set; }

        public ICollection<HotelRoom> HotelRooms { get; set; }

    }
}
