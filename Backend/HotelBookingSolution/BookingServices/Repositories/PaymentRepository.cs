using BookingServices.Contexts;
using BookingServices.Models;
using HotelBooking.Interfaces;

namespace BookingServices.Repositories
{
    public class PaymentRepository : IRepository<int, Payment>
    {
        private readonly HotelBookingContext _context;

        public PaymentRepository(HotelBookingContext context)
        {
            _context = context;
        }

        public async Task<Payment> Add(Payment item)
        {
            _context.Add(item);
            await _context.SaveChangesAsync();
            return item;
        }

        public Task<Payment> Delete(int key)
        {
            throw new NotImplementedException();
        }

        public Task<Payment> Get(int key)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Payment>> Get()
        {
            throw new NotImplementedException();
        }

        public Task<Payment> Update(Payment item)
        {
            throw new NotImplementedException();
        }
    }
}
