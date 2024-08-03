using HotelBooking.Interfaces;
using HotelServices.Exceptions;
using HotelServices.Interfaces;
using HotelServices.Models;
using HotelServices.Models.DTOs;

namespace HotelServices.Services
{
    public class AmenitiesServices : IAmenityService
    {
        //INITIALIZATION
        private readonly IRepository<int, Hotel> _hotelRepo;
        private readonly IRepository<int, Room> _roomRepo;
        private readonly IRepository<int, Amenity> _amenityRepo;
        private readonly ILogger<HotelsServices> _logger;

        //DEPENDENCY INJECTION
        public AmenitiesServices(IRepository<int, Hotel> hotelRepo, ILogger<HotelsServices> logger,
                                IRepository<int, Room> roomRepo,IRepository<int, Amenity> amenityRepo)
        {
            _hotelRepo = hotelRepo;
            _logger = logger;
            _roomRepo = roomRepo;
            _amenityRepo = amenityRepo;
        }

        //ADD AMENITIES TO ROOM
        public async Task<HotelReturnDTO> AddAmenitiesToHotelAsync(AddAmenitiesToHotelDTO dto)
        {
            try
            {
                var hotel = await _hotelRepo.Get(dto.HotelID);
                if (hotel == null)
                {
                    throw new NoSuchHotelException(dto.HotelID);
                }

                foreach (var amenityId in dto.AmenityIds)
                {
                    var amenity = await _amenityRepo.Get(amenityId);

                    if (!hotel.HotelAmenities.Any(ha => ha.AmenityId == amenityId))
                    {
                        hotel.HotelAmenities.Add(new HotelAmenity
                        {
                            HotelId = hotel.Id,
                            AmenityId = amenityId
                        });
                    }
                }

                var result = await _hotelRepo.Update(hotel);
                var returnResult = await MapHotelToHotelReturnDTO(result);
                return returnResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while adding amenities to the hotel");
                throw;
            }
        }

        //DELETE AMENITIES
        public async Task<HotelReturnDTO> DeleteAmenityFromHotelAsync(DeleteAmenityDTO deleteAmenityDTO)
        {
            try
            {
                var hotel = await _hotelRepo.Get(deleteAmenityDTO.HotelID);

                foreach (var amenityId in deleteAmenityDTO.AmenityIds)
                {
                    var amenity = await _amenityRepo.Get(amenityId);

                    var hotelAmenity = hotel.HotelAmenities.FirstOrDefault(ha => ha.AmenityId == amenityId);
                    if (hotelAmenity == null)
                    {
                        throw new AmenityNotInHotelException(deleteAmenityDTO.HotelID, amenityId);
                    }

                    hotel.HotelAmenities.Remove(hotelAmenity);
                }

                var result = await _hotelRepo.Update(hotel);
                var returnResult = await MapHotelToHotelReturnDTO(result);

                return returnResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting the amenities from the hotel");
                throw;
            }
        }


        //GET AMENITIES
        public async Task<IEnumerable<Amenity>> GetAllAmenities()
        {
            try
            {
                var result = await _amenityRepo.Get();
                return result;
            }
            catch (NoSuchAmenityFound ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        // ADD AMENITIES
        public async Task<AmenityDTO> AddAmenityAsync(AmenityDTO amenityDTO)
        {
            try
            {
                var amenity = new Amenity
                {
                    Name = amenityDTO.Name,
                };

                var result = await _amenityRepo.Add(amenity);
                return new AmenityDTO
                {
                    Id = result.Id,
                    Name = result.Name
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while adding the amenity");
                throw;
            }
        }

        //MAPPING --- HOTEL TO HOTEL RETURN DTO
        private async Task<HotelReturnDTO> MapHotelToHotelReturnDTO(Hotel hotel)
        {
            var hotelImages = hotel.HotelImages?.Select(image => image.ImageUrl).ToList() ?? new List<string>();

            List<Room> rooms = new List<Room>();
            List<Amenity> amenities = new List<Amenity>();
            try
            {
                // Get a new DbContext instance from the repository
                rooms = (await _roomRepo.Get()).Where(r => r.HotelId == hotel.Id).ToList() ?? new List<Room>();

                amenities = (await _amenityRepo.Get())
                    .Where(a => a.HotelAmenities != null && a.HotelAmenities.Any(ha => ha.HotelId == hotel.Id))
                    .ToList() ?? new List<Amenity>();
            }
            catch (NoSuchRoomException ex)
            {
                // Handle exception appropriately
            }

            var roomIDs = rooms.Select(r => r.RoomNumber).ToList();
            var amenityIDs = amenities.Select(a => a.Id).ToList();

            return new HotelReturnDTO
            {
                Name = hotel.Name,
                Id = hotel.Id,
                Address = hotel.Address,
                City = hotel.City,
                State = hotel.State,
                Type = hotel.Type,
                NumOfRooms = hotel.NumOfRooms,
                AverageRatings = hotel.AverageRatings,
                Description = hotel.Description,
                HotelImages = hotelImages,
                RoomIDs = roomIDs,
                Amenities = amenityIDs
            };
        }

    }
}
