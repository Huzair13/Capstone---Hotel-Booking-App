using BookingServices.Exceptions;
using CancellationService.Contexts;
using CancellationService.Interfaces;
using CancellationService.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Channels;

namespace CancellationService.Repositories
{
    public class RefundRepository : IRepository<int, Refund>
    {
        private readonly CancelServiceContext _context;

        public RefundRepository(CancelServiceContext context)
        {
            _context = context;
        }
        public async Task<Refund> Add(Refund item)
        {
            _context.Add(item);
            await _context.SaveChangesAsync();
            return item;
        }

        public async Task<Refund> Delete(int refundID)
        {
            var booking = await Get(refundID);
            _context.Remove(booking);
            await _context.SaveChangesAsync(true);
            return booking;
        }

        public async Task<Refund> Get(int RefundID)
        {
            var refunds = await _context.Refunds
            .FirstOrDefaultAsync(b => b.Id == RefundID);
            if (refunds != null)
            {
                return refunds;
            }
            throw new NoSuchRefundsException(RefundID);
        }

        public async Task<IEnumerable<Refund>> Get()
        {
            var refunds = await _context.Refunds.ToListAsync();
            if (refunds.Count != 0)
            {
                return refunds;
            }
            throw new NoSuchRefundsException();
        }

        public async Task<Refund> Update(Refund refund)
        {
            var existingRefunds = await Get(refund.Id);
            _context.Update(refund);
            await _context.SaveChangesAsync();
            return existingRefunds;
        }
    }
}
