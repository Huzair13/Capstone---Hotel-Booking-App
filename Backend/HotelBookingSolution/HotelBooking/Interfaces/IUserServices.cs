using HotelBooking.Models;

namespace HotelBooking.Interfaces
{
    public interface IUserServices
    {
        public Task<User> GetUserById(int UserId);
    }
}
