using HotelServices.Models.DTOs;
using HotelServices.Models;

namespace HotelServices.Interfaces
{
    public interface IAmenityService
    {
        public Task<HotelReturnDTO> AddAmenitiesToHotelAsync(AddAmenitiesToHotelDTO dto);
        public Task<AmenityDTO> AddAmenityAsync(AmenityDTO amenityDTO);
        public Task<HotelReturnDTO> DeleteAmenityFromHotelAsync(DeleteAmenityDTO deleteAmenityDTO);
        public Task<IEnumerable<Amenity>> GetAllAmenities();
    }
}
