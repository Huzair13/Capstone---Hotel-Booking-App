using HotelBooking.Interfaces;
using HotelServices.Contexts;
using HotelServices.Exceptions;
using HotelServices.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelServices.Repositories
{
    public class AmenitiesRepository : IRepository<int, Amenity>
    {
        private readonly HotelServicesContext _context;

        public AmenitiesRepository(HotelServicesContext context)
        {
            _context = context;
        }
        public async Task<Amenity> Add(Amenity item)
        {
            _context.Amenities.Add(item);
            await _context.SaveChangesAsync();
            return item;
        }

        public async Task<Amenity> Delete(int amenityID)
        {
            var amenity = await Get(amenityID);
            _context.Amenities.Remove(amenity);
            await _context.SaveChangesAsync();
            return amenity;
        }

        public async Task<Amenity> Get(int amenityID)
        {
            var amenity = await _context.Amenities
                .FirstOrDefaultAsync(r => r.Id == amenityID);

            if (amenity != null)
            {
                return amenity;
            }

            throw new NoSuchAmenityFound(amenityID);
        }

        public async Task<IEnumerable<Amenity>> Get()
        {
            var rooms = await _context.Amenities
                .ToListAsync();

            if (rooms.Count != 0)
            {
                return rooms;
            }

            throw new NoSuchAmenityFound();
        }

        public async Task<Amenity> Update(Amenity amenity)
        {
            var existingAmenity = await Get(amenity.Id);
            _context.Entry(existingAmenity).CurrentValues.SetValues(amenity);
            await _context.SaveChangesAsync();
            return existingAmenity;
        }
    }
}
