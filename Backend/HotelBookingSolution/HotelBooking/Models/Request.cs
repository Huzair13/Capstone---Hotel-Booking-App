using HotelBooking.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuthenticationServices.Models
{
    public class Request
    {
        [Key]
        public int Id { get; set; }
        public int userId { get; set; }
        [ForeignKey("userId")]
        public User User { get; set; }
        public string Reason { get; set; }
        public string Status { get; set; }

    }
}
