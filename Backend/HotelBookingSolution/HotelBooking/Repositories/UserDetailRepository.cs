using HotelBooking.Contexts;
using HotelBooking.Exceptions;
using HotelBooking.Interfaces;
using HotelBooking.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.Repositories
{
    public class UserDetailRepository : IRepository<int, UserDetails>
    {
        private HotelBookingDbContext _context;

        public UserDetailRepository(HotelBookingDbContext context)
        {
            _context = context;
        }
        public async Task<UserDetails> Add(UserDetails item)
        {
            _context.Add(item);
            await _context.SaveChangesAsync();
            return item;
        }

        public async Task<UserDetails> Delete(int key)
        {
            var user = await Get(key);
            _context.Remove(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<UserDetails> Get(int key)
        {
            return (await _context.UsersDetails.SingleOrDefaultAsync(u => u.UserId == key)) ?? throw new NoSuchUserException(key);
        }

        public async Task<IEnumerable<UserDetails>> Get()
        {
            var results = await _context.UsersDetails.ToListAsync();
            if (results.Count() == 0)
            {
                throw new NoSuchUserException();
            }
            return results;
        }

        public async Task<UserDetails> Update(UserDetails item)
        {
            var user = await Get(item.UserId);
            _context.Update(item);
            await _context.SaveChangesAsync();
            return user;
        }
    }

}
