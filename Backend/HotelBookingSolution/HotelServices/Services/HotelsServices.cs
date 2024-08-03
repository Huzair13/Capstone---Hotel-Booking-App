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
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

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
        public async Task<IList<string>> GetCities()
        {
            try
            {
                var allHotels = await _hotelRepo.Get();

                var cities = allHotels
                    .Select(hotel => hotel.City)
                    .Select(city => city.ToLower())  
                    .Distinct(StringComparer.OrdinalIgnoreCase)  
                    .ToList();  
                return cities;
            }
            catch (Exception ex)
            {

                return new List<string>();
            }
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

        //GET HOTEL BY ID
        public async Task<HotelReturnDTO> GetHotelByID(int hotelId)
        {
            try
            {
                var hotel = await _hotelRepo.Get(hotelId);
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
                    throw new NoSuchHotelException();
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
                            .Where(r => r.HotelId == bestCombinationDTO.HotelId && !bookedRooms.Contains(r.RoomNumber) && !r.IsDeleted)
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
                    .Where(r => r.HotelId == bestCombinationDTO.HotelId && !bookedRooms.Contains(r.RoomNumber) && !r.IsDeleted)
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


        // GET AVAILABLE HOTELS BY DATE
        public async Task<List<HotelReturnDTO>> GetAvailableHotelsByDateAsync(DateTime checkInDate, DateTime checkOutDate)
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

                // Fetch all rooms from Room Repository
                var allRooms = await _roomRepo.Get();

                // Filter out rooms that are booked
                var availableRooms = allRooms
                    .Where(r => !bookedRoomNumbers.Contains(r.RoomNumber) && !r.IsDeleted)
                    .ToList();

                // Get distinct Hotel IDs from available rooms
                var availableHotelIds = availableRooms
                    .Select(r => r.HotelId)
                    .Distinct()
                    .ToList();

                // Fetch all hotels from Hotel Repository
                var allHotels = await _hotelRepo.Get();

                var availableHotels = allHotels
                            .Where(h => availableHotelIds.Contains(h.Id))
                            .ToList();

                // Map each Hotel to HotelDTO
                var hotelDTOs = new List<HotelReturnDTO>();
                foreach (var hotel in availableHotels)
                {
                    var hotelDTO = await MapHotelToHotelReturnDTO(hotel);
                    hotelDTOs.Add(hotelDTO);
                }
                return hotelDTOs;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving available hotels");
                throw;
            }
        }



        //UPDATE HOTELS
        public async Task<HotelReturnDTO> UpdateHotelAsync(HotelUpdateDataDTO hotelUpdateDTO)
        {
            try
            {
                var hotel = await _hotelRepo.Get(hotelUpdateDTO.HotelId);

                // Update hotel properties
                hotel.Name = hotelUpdateDTO.Name ?? hotel.Name;
                hotel.Address = hotelUpdateDTO.Address ?? hotel.Address;
                hotel.City = hotelUpdateDTO.City ?? hotel.City;
                hotel.State = hotelUpdateDTO.State ?? hotel.State;
                hotel.Type = hotelUpdateDTO.Type ?? hotel.Type;
                hotel.Description = hotelUpdateDTO.Description ?? hotel.Description;

                var updatedHotel = await _hotelRepo.Update(hotel);
                return await MapHotelToHotelReturnDTO(updatedHotel);
            }
            catch(NoSuchHotelException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating the hotel");
                throw;
            }
        }

        //DELETE HOTEL -- HARD DELETE
        public async Task<HotelReturnDTO> DeleteHotelAsync(int hotelId)
        {
            try
            {
                var hotel = await _hotelRepo.Get(hotelId);
                var result = await _hotelRepo.Delete(hotelId);
                return await MapHotelToHotelReturnDTO(result);
            }
            catch(NoSuchHotelException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting the hotel");
                throw;
            }
        }

        public async Task<HotelReturnDTO> AddImagesToHotelAsync(AddImageDTO addImageDTO)
        {
            try
            {
                var hotel = await _hotelRepo.Get(addImageDTO.hotelId);

                // Convert ICollection to List
                var hotelImages = hotel.HotelImages.ToList();
                var newHotelImages = addImageDTO.imageUrls.Select(url => new HotelImage { ImageUrl = url }).ToList();
                hotelImages.AddRange(newHotelImages);

                // Reassign back to ICollection
                hotel.HotelImages = hotelImages;

                var result = await _hotelRepo.Update(hotel);
                return await MapHotelToHotelReturnDTO(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while adding images to the hotel");
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
    }
}
