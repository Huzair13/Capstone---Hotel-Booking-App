using System.ComponentModel.DataAnnotations;

namespace HotelBooking.Models.DTOs
{
    public class UserLoginDTO
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "User Id cannot be Empty")]
        [Range(100, 999, ErrorMessage = "Invalid Entry for User ID")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Password cannot be Empty")]
        [MinLength(6, ErrorMessage = "Password has to be minimum 6 char long")]
        public string Password { get; set; } = string.Empty;
    }
}
