using HotelBooking.Contexts;
using HotelBooking.Exceptions;
using HotelBooking.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.Repositories
{
    public class GenericUserRepository<T> : IRepository<int, T> where T : class
    {
        protected readonly HotelBookingDbContext _context;
        public GenericUserRepository(HotelBookingDbContext context)
        {
            _context = context;
        }


        public async Task<T> Add(T user)
        {
            _context.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<T> Delete(int userId)
        {
            var user = await Get(userId);
            _context.Remove(user);
            _context.SaveChangesAsync(true);
            return user;
        }

        public async Task<T> Get(int userId)
        {
            var user = await _context.Set<T>().FindAsync(userId);
            if (user != null)
            {
                return user;
            }
            throw new NoSuchUserException(userId);
        }

        public async Task<IEnumerable<T>> Get()
        {
            var users = await _context.Set<T>().ToListAsync();
            return users;
        }

        public async Task<T> Update(T user)
        {

            var userIdProperty = typeof(T).GetProperty("Id");
            var userIdValue = (int)userIdProperty.GetValue(user);

            var existingUser = await Get(userIdValue);

            _context.Entry(existingUser).CurrentValues.SetValues(user);
            await _context.SaveChangesAsync();
            return existingUser;
        }
    }
}
