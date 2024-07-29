using AuthenticationServices.Exceptions;
using AuthenticationServices.Models;
using HotelBooking.Contexts;
using HotelBooking.Interfaces;
using HotelBooking.Models;
using Microsoft.EntityFrameworkCore;

namespace AuthenticationServices.Repositories
{
    public class RequestRepository : IRepository<int, Request>
    {
        private readonly HotelBookingDbContext _context;

        public RequestRepository(HotelBookingDbContext context)
        {
            _context = context;
        }
        public async Task<Request> Add(Request item)
        {
            _context.Add(item);
            await _context.SaveChangesAsync();
            return item;
        }

        public async Task<Request> Delete(int requestID)
        {
            var request = await Get(requestID);
            _context.Remove(request);
            _context.SaveChangesAsync(true);
            return request;
        }

        public async Task<Request> Get(int requestId)
        {
            var request = await _context.Requests
                .FirstOrDefaultAsync(b => b.Id == requestId);
            if (request != null)
            {
                return request;
            }
            throw new NoSuchRequestException(requestId);
        }

        public async Task<IEnumerable<Request>> Get()
        {
            var requests = await _context.Requests.ToListAsync();
            if (requests.Count == 0)
            {
                throw new NoSuchRequestException();
            }
            return requests;
        }

        public async Task<Request> Update(Request request)
        {
            _context.Update(request);
            await _context.SaveChangesAsync();
            var existingRequest = await Get(request.Id);
            return existingRequest;
        }
    }
}
