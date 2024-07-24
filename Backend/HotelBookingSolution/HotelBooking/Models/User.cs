using System.ComponentModel.DataAnnotations;

namespace HotelBooking.Models
{
    public enum Gender
    {
        Men,
        Women,
        Others
    }

    public class User
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string MobileNumber { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string UserType { get; set; }
        public Gender Gender { get; set; }
        public bool IsActivated { get; set; } = true;
        public int Age
        {
            get
            {
                DateTime today = DateTime.Today;
                int age = today.Year - DateOfBirth.Year;
                if (today < DateOfBirth.AddYears(age))
                {
                    age--;
                }

                return age;
            }
        }
    }
}
