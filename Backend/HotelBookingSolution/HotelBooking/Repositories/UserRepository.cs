using HotelBooking.Contexts;
using HotelBooking.Interfaces;
using HotelBooking.Models;

namespace HotelBooking.Repositories
{
    public class UserRepository : GenericUserRepository<User>, IRepository<int, User>
    {
        public UserRepository(HotelBookingDbContext context) : base(context)
        {
        }

    }
}
