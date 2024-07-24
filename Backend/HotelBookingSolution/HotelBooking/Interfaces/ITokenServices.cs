using HotelBooking.Models;

namespace HotelBooking.Interfaces
{
    public interface ITokenServices
    {
        public Task<string> GenerateToken(User user);
    }
}
