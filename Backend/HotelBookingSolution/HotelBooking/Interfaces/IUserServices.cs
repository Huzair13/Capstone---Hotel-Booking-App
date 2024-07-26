using HotelBooking.Models;

namespace HotelBooking.Interfaces
{
    public interface IUserServices
    {
        public Task<User> GetUserById(int UserId);
        public Task<User> DeactivateUser(int UserId);
        public Task<bool> IsActivated(int UserId);
    }
}
