using BookingServices.Models.DTOs;
using HotelServices.Models;
using HotelServices.Models.DTOs;

namespace HotelServices.Interfaces
{
    public interface IHotelServices
    {
        public Task<HotelDetailsDTO> GetAllHotelsRoomsAndAmenitiesAsync();


        public Task<List<HotelReturnDTO>> GetAvailableHotelsByDateAsync(DateTime checkInDate, DateTime checkOutDate);
        public Task<List<HotelReturnDTO>> GetAllHotels();
        public Task<HotelReturnDTO> GetHotelByName(string hotelName);
        public Task<HotelReturnDTO> GetHotelByID(int hotelId);
        public Task<HotelReturnDTO> AddHotelAsync(HotelDTO hotelDTO);
        public Task UpdateHotelAverageRatingAsync(HotelUpdateDTO hotelUpdateDTO);
        public Task<HotelReturnDTO> UpdateHotelAsync(HotelUpdateDataDTO hotelUpdateDTO);
        public Task<HotelReturnDTO> DeleteHotelAsync(int hotelId);
        public Task<HotelReturnDTO> AddImagesToHotelAsync(AddImageDTO addImageDTO);
        public Task<IList<string>> GetCities();


        public Task<bool> CheckAvailabilityAsync(BestCombinationDTO bestCombinationDTO);
        public Task<List<Room>> BestAvailableCombinationAsync(BestCombinationDTO bestCombinationDTO);

    }
}
