using HotelBooking.Interfaces;
using HotelServices.Contexts;
using HotelServices.Exceptions;
using HotelServices.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelServices.Repositories
{
    public class HotelRepository : IRepository<int, Hotel>
    {
        private readonly HotelServicesContext _context;

        public HotelRepository(HotelServicesContext context)
        {
            _context = context;
        }
        public async Task<Hotel> Add(Hotel item)
        {
            if (item.HotelImages == null) item.HotelImages = new List<HotelImage>();
            if (item.Rooms == null) item.Rooms = new List<Room>();

            _context.Add(item);
            await _context.SaveChangesAsync();
            return item;
        }

        public async Task<Hotel> Delete(int hotelId)
        {
            var hotel = await Get(hotelId);
            _context.Remove(hotel);
            await _context.SaveChangesAsync(true);
            return hotel;
        }

        public async Task<Hotel> Get(int hotelID)
        {
            var hotel = await _context.Hotels
                .Include(h => h.HotelImages)
                .Include(h => h.Rooms) 
                .Include(a=>a.HotelAmenities)
                .FirstOrDefaultAsync(h => h.Id == hotelID);

            if (hotel != null)
            {
                return hotel;
            }
            throw new NoSuchHotelException(hotelID);
        }

        public async Task<IEnumerable<Hotel>> Get()
        {
            var hotels = await _context.Hotels.Include(q => q.HotelImages)
                                .Include(a => a.HotelAmenities)
                                .Include(h=>h.Rooms).ToListAsync();
            if (hotels.Count != 0)
            {
                return hotels;
            }
            throw new NoSuchHotelException();
        }

        public async Task<Hotel> Update(Hotel hotel)
        {
            var existingHotel = await Get(hotel.Id);
            _context.Update(hotel);
            await _context.SaveChangesAsync();
            return existingHotel;
        }
    }
}
