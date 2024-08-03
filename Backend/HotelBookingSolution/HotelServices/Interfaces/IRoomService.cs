using HotelServices.Models.DTOs;

namespace HotelServices.Interfaces
{
    public interface IRoomService
    {
        public Task<HotelReturnDTO> AddRoomToHotelAsync(int hotelId, RoomDTO roomDTO);
        public Task<List<RoomDTO>> GetAvailableRoomsAsync(DateTime checkInDate, DateTime checkOutDate, int numberOfGuests);
        public Task<List<RoomDTO>> GetAllRoomsAsync();
        public Task<List<RoomDTO>> GetAvailableRoomsByDateAsync(DateTime checkInDate, DateTime checkOutDate);
        public Task<HotelReturnDTO> RemoveRoomFromHotelAsync(int hotelId, int roomNumber);
        public Task<HotelReturnDTO> SoftDeleteRoom(int hotelId, int roomNumber);
        public Task<List<RoomDTO>> GetRoomByHotelId(int hotelId);

    }
}
