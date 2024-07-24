using HotelBooking.Interfaces;
using HotelServices.Contexts;
using HotelServices.Exceptions;
using HotelServices.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HotelServices.Repositories
{
    public class HotelRoomRepository : IRepository<int, HotelRoom>
    {
        private readonly HotelServicesContext _context;

        public HotelRoomRepository(HotelServicesContext context)
        {
            _context = context;
        }

        public async Task<HotelRoom> Add(HotelRoom item)
        {
            _context.HotelRooms.Add(item);
            await _context.SaveChangesAsync();
            return item;
        }

        public async Task<HotelRoom> Delete(int roomId)
        {
            var hotelRoom = await Get(roomId);
            _context.HotelRooms.Remove(hotelRoom);
            await _context.SaveChangesAsync();
            return hotelRoom;
        }

        public async Task<HotelRoom> Get(int roomId)
        {
            var hotelRoom = await _context.HotelRooms
                .Include(hr => hr.Room) 
                .Include(hr => hr.Hotel) 
                .FirstOrDefaultAsync(hr => hr.RoomID == roomId);

            if (hotelRoom != null)
            {
                return hotelRoom;
            }
            throw new NoSuchHotelRoomException(roomId);
        }

        public async Task<IEnumerable<HotelRoom>> Get()
        {
            var hotelRooms = await _context.HotelRooms
                .Include(hr => hr.Room) 
                .Include(hr => hr.Hotel) 
                .ToListAsync();

            if (hotelRooms.Any())
            {
                return hotelRooms;
            }
            throw new NoSuchHotelRoomException();
        }

        public async Task<HotelRoom> Update(HotelRoom item)
        {
            var existingHotelRoom = await Get(item.RoomID);
            _context.Entry(existingHotelRoom).CurrentValues.SetValues(item);
            await _context.SaveChangesAsync();
            return existingHotelRoom;
        }
    }
}
