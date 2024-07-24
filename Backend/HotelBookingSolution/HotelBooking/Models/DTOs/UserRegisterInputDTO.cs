using System.ComponentModel.DataAnnotations;

namespace HotelBooking.Models.DTOs
{
    public class UserRegisterInputDTO
    {
        [Required(ErrorMessage = "Name is required.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid Email Address.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Mobile Number is required.")]
        [StringLength(10, MinimumLength = 10, ErrorMessage = "Mobile Number must be exactly 10 characters.")]
        public string MobileNumber { get; set; }

        [Required(ErrorMessage = "Date of Birth is required.")]
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }

        [Required(ErrorMessage = "Password cannot be empty.")]
        [MinLength(6, ErrorMessage = "Password has to be minimum 6 characters long.")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Gender cannot be empty.")]
        public Gender Gender { get; set; }

        [Required(ErrorMessage = "User Type is required.")]
        [StringLength(50, ErrorMessage = "User Type cannot be longer than 50 characters.")]
        public string UserType { get; set; }
    }
}
