using HotelBooking.Interfaces;
using HotelServices.Contexts;
using HotelServices.Exceptions;
using HotelServices.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HotelServices.Repositories
{
    public class RoomRepository : IRepository<int, Room>
    {
        private readonly HotelServicesContext _context;

        public RoomRepository(HotelServicesContext context)
        {
            _context = context;
        }

        public async Task<Room> Add(Room item)
        {
            _context.Rooms.Add(item);
            await _context.SaveChangesAsync();
            return item;
        }

        public async Task<Room> Delete(int roomId)
        {
            var room = await Get(roomId);
            _context.Rooms.Remove(room);
            await _context.SaveChangesAsync();
            return room;
        }

        public async Task<Room> Get(int roomId)
        {
            var room = await _context.Rooms
                    .FirstOrDefaultAsync(r => r.RoomNumber == roomId);

            if (room != null)
            {
                return room;
            }

            throw new NoSuchRoomException(roomId);
        }

        public async Task<IEnumerable<Room>> Get()
        {
            var rooms = await _context.Rooms
                    .ToListAsync();

            if (rooms.Count != 0)
            {
                return rooms;
            }

            throw new NoSuchRoomException();
        }

        public async Task<Room> Update(Room item)
        {
            var existingRoom = await Get(item.RoomNumber);
            _context.Entry(existingRoom).CurrentValues.SetValues(item);
            await _context.SaveChangesAsync();
            return existingRoom;
        }
    }
}
