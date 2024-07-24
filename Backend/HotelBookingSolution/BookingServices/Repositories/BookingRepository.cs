using BookingServices.Contexts;
using BookingServices.Exceptions;
using BookingServices.Models;
using HotelBooking.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BookingServices.Repositories
{
    public class BookingRepository : IRepository<int, Booking>
    {
        private readonly HotelBookingContext _context;

        public BookingRepository(HotelBookingContext context)
        {
            _context = context;
        }
        public async Task<Booking> Add(Booking item)
        {
            _context.Add(item);
            await _context.SaveChangesAsync();
            return item;
        }

        public async Task<Booking> Delete(int bookingID)
        {
            var booking = await Get(bookingID);
            _context.Remove(booking);
            await _context.SaveChangesAsync(true);
            return booking;
        }

        public async Task<Booking> Get(int bookingID)
        {
            var booking = await _context.Bookings.FirstOrDefaultAsync(b => b.Id == bookingID);
            if (booking != null)
            {
                return booking;
            }
            throw new NoSuchBookingException(bookingID);
        }

        public async Task<IEnumerable<Booking>> Get()
        {
            var bookings = await _context.Bookings.ToListAsync();
            if (bookings.Count != 0)
            {
                return bookings;
            }
            throw new NoSuchBookingException();
        }

        public async Task<Booking> Update(Booking booking)
        {
            var existingBooking = await Get(booking.Id);
            _context.Update(booking);
            await _context.SaveChangesAsync();
            return existingBooking;
        }
    }
}
