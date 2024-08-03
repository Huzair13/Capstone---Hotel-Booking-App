using HotelBooking.Interfaces;
using HotelServices.Exceptions;
using HotelServices.Interfaces;
using HotelServices.Models;
using HotelServices.Models.DTOs;
using Newtonsoft.Json;
using System.Net.Http.Headers;

namespace HotelServices.Services
{
    public class RoomService : IRoomService
    {
        //INITIALIZATION
        private readonly IRepository<int, Hotel> _hotelRepo;
        private readonly IRepository<int, Room> _roomRepo;
        private readonly IRepository<int, Amenity> _amenityRepo;
        private readonly ILogger<HotelsServices> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        //DEPENDENCY INJECTION
        public RoomService(IRepository<int, Hotel> hotelRepo, ILogger<HotelsServices> logger,
                                IRepository<int, Room> roomRepo, IHttpClientFactory httpClientFactory,
                                IHttpContextAccessor httpContextAccessor, IRepository<int, Amenity> amenityRepo)
        {
            _hotelRepo = hotelRepo;
            _logger = logger;
            _roomRepo = roomRepo;
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
            _amenityRepo = amenityRepo;
        }
        //ADD ROOMS TO HOTEL
        public async Task<HotelReturnDTO> AddRoomToHotelAsync(int hotelId, RoomDTO roomDTO)
        {
            try
            {
                var hotel = await _hotelRepo.Get(hotelId);

                int roomNumber = int.Parse($"{hotelId}{roomDTO.RoomNumber}");

                Room room = new Room
                {
                    RoomNumber = roomNumber,
                    RoomType = roomDTO.RoomType,
                    RoomFloor = roomDTO.RoomFloor,
                    HotelId = hotelId,
                    AllowedNumOfGuests = roomDTO.AllowedNumOfGuests,
                    Rent = roomDTO.Rent,
                    IsDeleted = false
                };

                var resultRoom = await _roomRepo.Add(room);

                // Update the number of rooms in the hotel
                hotel.NumOfRooms += 1;
                await _hotelRepo.Update(hotel);

                return await MapHotelToHotelReturnDTO(hotel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while adding a room to the hotel");
                throw;
            }
        }


        //GET ALL ROOMS
        public async Task<List<RoomDTO>> GetAllRoomsAsync()
        {
            try
            {
                var rooms = await _roomRepo.Get();
                var roomDTOs = rooms
                    .Where(r => !r.IsDeleted) 
                    .Select(r => new RoomDTO
                    {
                        IsDeleted = r.IsDeleted,
                        RoomNumber = r.RoomNumber,
                        RoomType = r.RoomType,
                        RoomFloor = r.RoomFloor,
                        AllowedNumOfGuests = r.AllowedNumOfGuests,
                        Rent = r.Rent,
                        HotelId = r.HotelId
                    }).ToList();
                return roomDTOs;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving all rooms");
                throw;
            }
        }

        //GET AVAILABLE ROOMS 
        public async Task<List<RoomDTO>> GetAvailableRoomsAsync(DateTime checkInDate, DateTime checkOutDate, int numberOfGuests)
        {
            try
            {
                var token = _httpContextAccessor.HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

                var hotelClient = _httpClientFactory.CreateClient("BookingService");

                var formattedCheckInDate = checkInDate.ToString("yyyy-MM-dd");
                var formattedCheckOutDate = checkOutDate.ToString("yyyy-MM-dd");

                var requestUri = $"api/GetBookedRooms?checkInDate={formattedCheckInDate}&checkOutDate={formattedCheckOutDate}";
                hotelClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var hotelResponse = await hotelClient.GetAsync(requestUri);
                if (!hotelResponse.IsSuccessStatusCode)
                {
                    var errorMessage = await hotelResponse.Content.ReadAsStringAsync();
                    throw new Exception($"Failed to fetch booked rooms. Status Code: {hotelResponse.StatusCode}");
                }

                var jsonResponse = await hotelResponse.Content.ReadAsStringAsync();
                var bookedRoomNumbers = JsonConvert.DeserializeObject<List<int>>(jsonResponse);

                // Fetch all rooms from Hotel Service
                var allRooms = await _roomRepo.Get();

                // Filter out rooms that are booked and meet the guest requirements
                var availableRooms = allRooms
                    .Where(r => !bookedRoomNumbers.Contains(r.RoomNumber) && !r.IsDeleted)
                    .Where(r => r.AllowedNumOfGuests >= numberOfGuests)
                    .ToList();

                // Map to RoomDTO and include HotelId
                return availableRooms.Select(r => new RoomDTO
                {
                    IsDeleted = r.IsDeleted,
                    RoomNumber = r.RoomNumber,
                    RoomType = r.RoomType,
                    RoomFloor = r.RoomFloor,
                    AllowedNumOfGuests = r.AllowedNumOfGuests,
                    Rent = r.Rent,
                    HotelId = r.HotelId
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving available rooms");
                throw;
            }
        }


        //GET AVAILABLE ROOMS BY DATE
        public async Task<List<RoomDTO>> GetAvailableRoomsByDateAsync(DateTime checkInDate, DateTime checkOutDate)
        {
            try
            {
                var token = _httpContextAccessor.HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

                var hotelClient = _httpClientFactory.CreateClient("BookingService");

                var formattedCheckInDate = checkInDate.ToString("yyyy-MM-dd");
                var formattedCheckOutDate = checkOutDate.ToString("yyyy-MM-dd");

                // Fetch booked rooms for the given date range
                var requestUri = $"api/GetBookedRooms?checkInDate={formattedCheckInDate}&checkOutDate={formattedCheckOutDate}";
                hotelClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var hotelResponse = await hotelClient.GetAsync(requestUri);
                if (!hotelResponse.IsSuccessStatusCode)
                {
                    var errorMessage = await hotelResponse.Content.ReadAsStringAsync();
                    throw new Exception($"Failed to fetch booked rooms. Status Code: {hotelResponse.StatusCode}");
                }

                var jsonResponse = await hotelResponse.Content.ReadAsStringAsync();
                var bookedRoomNumbers = JsonConvert.DeserializeObject<List<int>>(jsonResponse);

                // Fetch all rooms from Hotel Service
                var allRooms = await _roomRepo.Get();

                // Filter out rooms that are booked
                var availableRooms = allRooms
                    .Where(r => !bookedRoomNumbers.Contains(r.RoomNumber) && !r.IsDeleted)
                    .ToList();

                // Map to RoomDTO
                return availableRooms.Select(r => new RoomDTO
                {
                    IsDeleted = r.IsDeleted,
                    RoomNumber = r.RoomNumber,
                    RoomType = r.RoomType,
                    RoomFloor = r.RoomFloor,
                    AllowedNumOfGuests = r.AllowedNumOfGuests,
                    Rent = r.Rent,
                    HotelId = r.HotelId
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving available rooms");
                throw;
            }
        }


        public async Task<List<RoomDTO>> GetRoomByHotelId(int hotelId)
        {
            try
            {
                var result = await _roomRepo.Get();

                var hotelRooms = result.Where(r => r.HotelId == hotelId).ToList();

                var returnHotelRooms = hotelRooms.Select(r => new RoomDTO
                {
                    HotelId = r.HotelId,
                    RoomNumber = r.RoomNumber,
                    RoomType = r.RoomType,
                    RoomFloor = r.RoomFloor,
                    AllowedNumOfGuests = r.AllowedNumOfGuests,
                    IsDeleted = r.IsDeleted,
                    Rent = r.Rent
                }).ToList();

                return returnHotelRooms;
            }
            catch (Exception ex)
            {
                throw;
            }
        }


        public async Task<HotelReturnDTO> RemoveRoomFromHotelAsync(int hotelId, int roomNumber)
        {
            try
            {
                // Retrieve the hotel with its rooms included
                var hotel = await _hotelRepo.Get(hotelId);

                // Find the room to be removed
                var room = hotel.Rooms.FirstOrDefault(r => r.RoomNumber == roomNumber);
                if (room == null)
                {
                    throw new NoSuchRoomException(roomNumber);
                }

                // Remove the room
                hotel.Rooms.Remove(room);
                await _roomRepo.Delete(room.RoomNumber);

                // Update the number of rooms in the hotel
                hotel.NumOfRooms -= 1;
                await _hotelRepo.Update(hotel);

                return await MapHotelToHotelReturnDTO(hotel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while removing a room from the hotel");
                throw;
            }
        }



        public async Task<HotelReturnDTO> SoftDeleteRoom(int hotelId, int roomNumber)
        {
            try
            {
                var hotel = await _hotelRepo.Get(hotelId);

                var room = hotel.Rooms.FirstOrDefault(r => r.RoomNumber == roomNumber);
                if (room == null)
                {
                    throw new NoSuchRoomException(roomNumber);
                }

                room.IsDeleted = true;
                await _roomRepo.Update(room);

                // Update the number of rooms in the hotel
                hotel.NumOfRooms -= 1;
                await _hotelRepo.Update(hotel);

                return await MapHotelToHotelReturnDTO(hotel);
            }
            catch (NoSuchHotelException ex)
            {
                throw ex;
            }
            catch (NoSuchRoomException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while removing a room from the hotel");
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
