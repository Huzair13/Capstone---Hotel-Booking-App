using BookingServices.Models.DTOs;
using HotelServices.Models;
using HotelServices.Models.DTOs;

namespace HotelServices.Interfaces
{
    public interface IHotelServices
    {
        public Task<HotelReturnDTO> AddHotelAsync(HotelDTO hotelDTO);
        public Task<List<HotelReturnDTO>> GetAllHotels();
        public Task<HotelReturnDTO> GetHotelByName(string hotelName);
        public Task<HotelReturnDTO> GetHotelByID(int hotelId);
        public Task<HotelReturnDTO> AddRoomToHotelAsync(int hotelId, RoomDTO roomDTO);
        public Task<List<RoomDTO>> GetAvailableRoomsAsync(DateTime checkInDate, DateTime checkOutDate, int numberOfGuests);
        public Task<List<RoomDTO>> GetAllRoomsAsync();
        public Task<List<RoomDTO>> GetAvailableRoomsByDateAsync(DateTime checkInDate, DateTime checkOutDate);
        public Task UpdateHotelAverageRatingAsync(HotelUpdateDTO hotelUpdateDTO);
        public Task<HotelReturnDTO> AddAmenitiesToHotelAsync(AddAmenitiesToHotelDTO dto);
        public Task<AmenityDTO> AddAmenityAsync(AmenityDTO amenityDTO);
        public Task<HotelReturnDTO> DeleteAmenityFromHotelAsync(DeleteAmenityDTO deleteAmenityDTO);
        public Task<IEnumerable<Amenity>> GetAllAmenities();
        public Task<HotelDetailsDTO> GetAllHotelsRoomsAndAmenitiesAsync();
        public Task<bool> CheckAvailabilityAsync(BestCombinationDTO bestCombinationDTO);
        public Task<List<Room>> BestAvailableCombinationAsync(BestCombinationDTO bestCombinationDTO);
    }
}
