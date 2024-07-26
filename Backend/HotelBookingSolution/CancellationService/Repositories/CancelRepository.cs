using BookingServices.Exceptions;
using CancellationService.Contexts;
using CancellationService.Interfaces;
using CancellationService.Models;
using Microsoft.EntityFrameworkCore;

namespace CancellationService.Repositories
{
    public class CancelRepository : IRepository<int, Cancel>
    {
        private readonly CancelServiceContext _context;

        public CancelRepository(CancelServiceContext context)
        {
            _context = context;
        }

        public async Task<Cancel> Add(Cancel item)
        {
            _context.Add(item);
            await _context.SaveChangesAsync();
            return item;
        }

        public async Task<Cancel> Delete(int cancellationID)
        {
            var booking = await Get(cancellationID);
            _context.Remove(booking);
            await _context.SaveChangesAsync(true);
            return booking;
        }

        public async Task<Cancel> Get(int cancellationID)
        {
            var booking = await _context.Cancels
                        .FirstOrDefaultAsync(b => b.Id == cancellationID);
            if (booking != null)
            {
                return booking;
            }
            throw new NoSuchCancelsException(cancellationID);
        }

        public async Task<IEnumerable<Cancel>> Get()
        {
            var bookings = await _context.Cancels.ToListAsync();
            if (bookings.Count != 0)
            {
                return bookings;
            }
            throw new NoSuchCancelsException();
        }

        public async Task<Cancel> Update(Cancel cancel)
        {
            var existingBooking = await Get(cancel.Id);
            _context.Update(cancel);
            await _context.SaveChangesAsync();
            return existingBooking;
        }
    }
}
