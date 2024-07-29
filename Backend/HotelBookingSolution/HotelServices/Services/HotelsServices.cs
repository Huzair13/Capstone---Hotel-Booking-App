using HotelBooking.Interfaces;
using HotelServices.Exceptions;
using HotelServices.Interfaces;
using HotelServices.Models;
using HotelServices.Models.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Threading.Tasks;
using Azure;
using Newtonsoft.Json;
using BookingServices.Models.DTOs;

namespace HotelServices.Services
{
    public class HotelsServices : IHotelServices
    {
        //INITIALIZATION
        private readonly IRepository<int, Hotel> _hotelRepo;
        private readonly IRepository<int, Room> _roomRepo;
        private readonly IRepository<int, Amenity> _amenityRepo;
        private readonly ILogger<HotelsServices> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        //DEPENDENCY INJECTION
        public HotelsServices(IRepository<int, Hotel> hotelRepo, ILogger<HotelsServices> logger,
                                IRepository<int, Room> roomRepo, IHttpClientFactory httpClientFactory,
                                IHttpContextAccessor httpContextAccessor, IRepository<int,Amenity> amenityRepo)
        {
            _hotelRepo = hotelRepo;
            _logger = logger;
            _roomRepo = roomRepo;
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
            _amenityRepo = amenityRepo;
        }

        //ADD HOTEL 
        public async Task<HotelReturnDTO> AddHotelAsync(HotelDTO hotelDTO)
        {
            try
            {
                var hotel = await MapHotelDTOToHotel(hotelDTO);
                var result = await _hotelRepo.Add(hotel);
                return await MapHotelToHotelReturnDTO(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while adding the hotel");
                throw;
            }
        }

        //ADD ROOMS TO HOTEL
        public async Task<HotelReturnDTO> AddRoomToHotelAsync(int hotelId, RoomDTO roomDTO)
        {
            try
            {
                var hotel = await _hotelRepo.Get(hotelId);
                if (hotel == null)
                {
                    throw new NoSuchHotelException(hotelId);
                }

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
                    .Where(r => !r.IsDeleted) // Filter out deleted rooms
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

        //GET ALL ROOMS
        public async Task<List<HotelReturnDTO>> GetAllHotels()
        {
            try
            {
                var hotels = await _hotelRepo.Get();
                var hotelDTOs = new List<HotelReturnDTO>();

                foreach (var hotel in hotels)
                {
                    var hotelDTO = await MapHotelToHotelReturnDTO(hotel);
                    hotelDTOs.Add(hotelDTO);
                }

                return hotelDTOs;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving all hotels");
                throw;
            }
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
                    if (amenity == null)
                    {
                        throw new NoSuchAmenityFound(amenityId);
                    }

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


        //GET HOTEL BY ID
        public async Task<HotelReturnDTO> GetHotelByID(int hotelId)
        {
            try
            {
                var hotel = await _hotelRepo.Get(hotelId);
                if (hotel == null)
                {
                    throw new NoSuchHotelException(hotelId);
                }
                return await MapHotelToHotelReturnDTO(hotel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving the hotel by ID");
                throw;
            }
        }

        // GET HOTEL BY NAME
        public async Task<HotelReturnDTO> GetHotelByName(string hotelName)
        {
            try
            {
                var hotels = await _hotelRepo.Get();
                var hotel = hotels.FirstOrDefault(h => h.Name.Equals(hotelName, StringComparison.OrdinalIgnoreCase));
                if (hotel == null)
                {
                    throw new NoSuchHotelException(hotelName);
                }
                return await MapHotelToHotelReturnDTO(hotel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving the hotel by name");
                throw;
            }
        }


        public async Task<List<Room>> BestAvailableCombinationAsync(BestCombinationDTO bestCombinationDTO)
        {
            try
            {
                var allRooms = await _roomRepo.Get();
                var token = _httpContextAccessor.HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

                var hotelClient = _httpClientFactory.CreateClient("BookingService");

                var formattedCheckInDate = bestCombinationDTO.CheckInDate.ToString("yyyy-MM-dd");
                var formattedCheckOutDate = bestCombinationDTO.CheckOutDate.ToString("yyyy-MM-dd");

                var requestUri = $"api/GetBookedRooms?checkInDate={formattedCheckInDate}&checkOutDate={formattedCheckOutDate}";
                hotelClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var hotelResponse = await hotelClient.GetAsync(requestUri);
                if (!hotelResponse.IsSuccessStatusCode)
                {
                    var errorMessage = await hotelResponse.Content.ReadAsStringAsync();
                    throw new Exception($"Failed to fetch booked rooms. Status Code: {hotelResponse.StatusCode}");
                }

                var jsonResponse = await hotelResponse.Content.ReadAsStringAsync();
                var bookedRooms = JsonConvert.DeserializeObject<List<int>>(jsonResponse);

                // Filter out booked rooms
                var availableRooms = allRooms
                            .Where(r => r.HotelId == bestCombinationDTO.HotelId && !bookedRooms.Contains(r.RoomNumber))
                            .OrderBy(r => r.AllowedNumOfGuests) 
                            .ToList();

                // Find combinations of rooms
                var roomCombinations = FindRoomCombinations(availableRooms, bestCombinationDTO.NumOfGuests, bestCombinationDTO.NumOfRooms);

                // Select the best combination with the minimum total capacity
                var bestCombination = roomCombinations
                    .OrderBy(c => c.Sum(r => r.AllowedNumOfGuests))
                    .FirstOrDefault();

                return bestCombination;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while checking hotel availability");
                throw;
            }
        }


        public async Task<bool> CheckAvailabilityAsync(BestCombinationDTO bestCombinationDTO)
        {
            try
            {
                var token = _httpContextAccessor.HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

                var hotelClient = _httpClientFactory.CreateClient("BookingService");

                var formattedCheckInDate = bestCombinationDTO.CheckInDate.ToString("yyyy-MM-dd");
                var formattedCheckOutDate = bestCombinationDTO.CheckOutDate.ToString("yyyy-MM-dd");

                var requestUri = $"api/GetBookedRooms?checkInDate={formattedCheckInDate}&checkOutDate={formattedCheckOutDate}";
                hotelClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var hotelResponse = await hotelClient.GetAsync(requestUri);
                if (!hotelResponse.IsSuccessStatusCode)
                {
                    var errorMessage = await hotelResponse.Content.ReadAsStringAsync();
                    throw new Exception($"Failed to fetch booked rooms. Status Code: {hotelResponse.StatusCode}");
                }

                var jsonResponse = await hotelResponse.Content.ReadAsStringAsync();
                var bookedRooms = JsonConvert.DeserializeObject<List<int>>(jsonResponse);

                var allRooms = await _roomRepo.Get();

                // Filter out booked rooms
                var availableRooms = allRooms
                    .Where(r => r.HotelId == bestCombinationDTO.HotelId && !bookedRooms.Contains(r.RoomNumber))
                    .OrderByDescending(r => r.AllowedNumOfGuests)
                    .ToList();

                // Find combinations of rooms
                var roomCombinations = FindRoomCombinations(availableRooms, bestCombinationDTO.NumOfGuests, bestCombinationDTO.NumOfRooms);

                return roomCombinations.Any();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while checking hotel availability");
                throw;
            }
        }

        private List<List<Room>> FindRoomCombinations(List<Room> rooms, int numberOfGuests, int numberOfRooms)
        {
            var results = new List<List<Room>>();
            FindCombinations(rooms, numberOfGuests, numberOfRooms, new List<Room>(), results);
            return results;
        }

        private void FindCombinations(List<Room> rooms, int numberOfGuests, int numberOfRooms, List<Room> current, List<List<Room>> results)
        {
            if (current.Count == numberOfRooms)
            {
                if (current.Sum(r => r.AllowedNumOfGuests) >= numberOfGuests)
                {
                    results.Add(new List<Room>(current));
                }
                return;
            }

            for (int i = 0; i < rooms.Count; i++)
            {
                var room = rooms[i];
                current.Add(room);
                FindCombinations(rooms.Skip(i + 1).ToList(), numberOfGuests, numberOfRooms, current, results);
                current.RemoveAt(current.Count - 1);
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
                    .Where(r => !bookedRoomNumbers.Contains(r.RoomNumber))
                    .Where(r => r.AllowedNumOfGuests >= numberOfGuests)
                    .ToList();

                // Map to RoomDTO and include HotelId
                return availableRooms.Select(r => new RoomDTO
                {
                    IsDeleted =r.IsDeleted,
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
                    .Where(r => !bookedRoomNumbers.Contains(r.RoomNumber))
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

        // MAPPING ----- HOTELDTO TO HOTEL
        private async Task<Hotel> MapHotelDTOToHotel(HotelDTO hotelDTO)
        {
            return new Hotel
            {
                Name = hotelDTO.Name,
                Address = hotelDTO.Address,
                City = hotelDTO.City,
                State = hotelDTO.State,
                Type = hotelDTO.Type,
                NumOfRooms = hotelDTO.NumOfRooms,
                AverageRatings = hotelDTO.AverageRatings,
                Description = hotelDTO.Description,
                HotelImages = hotelDTO.HotelImages.Select(url => new HotelImage { ImageUrl = url }).ToList()
            };
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


        //UPDATE HOTEL AVERAGE RATING
        public async Task UpdateHotelAverageRatingAsync(HotelUpdateDTO hotelUpdateDTO)
        {
            try
            {
                var hotel = await _hotelRepo.Get(hotelUpdateDTO.HotelID);
                if (hotel == null)
                {
                    throw new NoSuchHotelException(hotelUpdateDTO.HotelID);
                }

                hotel.AverageRatings=hotelUpdateDTO.averageRating;

                var result =await _hotelRepo.Update(hotel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating the hotel's average rating");
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
            catch(NoSuchAmenityFound ex)
            {
                throw ex;
            }
            catch(Exception ex)
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

        public async Task<HotelDetailsDTO> GetAllHotelsRoomsAndAmenitiesAsync()
        {
            try
            {
                var hotels = await _hotelRepo.Get();
                var rooms = await _roomRepo.Get();
                var amenities = await _amenityRepo.Get();

                var hotelDTOs = new List<HotelReturnDTO>();
                foreach (var hotel in hotels)
                {
                    var hotelDTO = await MapHotelToHotelReturnDTO(hotel);
                    hotelDTOs.Add(hotelDTO);
                }

                var roomDTOs = rooms
                    .Where(r => !r.IsDeleted) // Filter out deleted rooms
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

                var amenityDTOs = amenities
                    .Select(a => new AmenityDTO
                    {
                        Id = a.Id,
                        Name = a.Name
                    }).ToList();

                return new HotelDetailsDTO
                {
                    Hotel = hotelDTOs,
                    Rooms = roomDTOs,
                    Amenities = amenityDTOs
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving all hotels, rooms, and amenities");
                throw;
            }
        }


        //DELETE AMENITIES
        public async Task<HotelReturnDTO> DeleteAmenityFromHotelAsync(DeleteAmenityDTO deleteAmenityDTO)
        {
            try
            {
                var hotel = await _hotelRepo.Get(deleteAmenityDTO.HotelID);
                if (hotel == null)
                {
                    throw new NoSuchHotelException(deleteAmenityDTO.HotelID);
                }

                var amenity = await _amenityRepo.Get(deleteAmenityDTO.AmenityId);
                if (amenity == null)
                {
                    throw new NoSuchAmenityFound(deleteAmenityDTO.AmenityId);
                }

                var hotelAmenity = hotel.HotelAmenities.FirstOrDefault(ha => ha.AmenityId == deleteAmenityDTO.AmenityId);
                if (hotelAmenity == null)
                {
                    throw new AmenityNotInHotelException(deleteAmenityDTO.HotelID, deleteAmenityDTO.AmenityId);
                }

                hotel.HotelAmenities.Remove(hotelAmenity);

                var result = await _hotelRepo.Update(hotel);
                var returnResult = await MapHotelToHotelReturnDTO(result);

                return returnResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting the amenity from the hotel");
                throw;
            }
        }


    }
}
