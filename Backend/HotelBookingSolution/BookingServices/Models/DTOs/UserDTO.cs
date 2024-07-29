namespace BookingServices.Models.DTOs
{
    public enum Gender
    {
        Men,
        Women,
        Others
    }
    public class UserDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string MobileNumber { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string UserType { get; set; }
        public Gender Gender { get; set; }
        public bool IsActivated { get; set; } = true;
        public int? CoinsEarned { get; set; }
    }
}
