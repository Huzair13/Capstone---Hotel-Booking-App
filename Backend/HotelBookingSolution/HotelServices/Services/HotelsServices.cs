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

namespace HotelServices.Services
{
    public class HotelsServices : IHotelServices
    {
        private readonly IRepository<int, Hotel> _hotelRepo;
        private readonly IRepository<int, Room> _roomRepo;
        private readonly ILogger<HotelsServices> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HotelsServices(IRepository<int, Hotel> hotelRepo, ILogger<HotelsServices> logger,
                                IRepository<int, Room> roomRepo, IHttpClientFactory httpClientFactory,
                                IHttpContextAccessor httpContextAccessor)
        {
            _hotelRepo = hotelRepo;
            _logger = logger;
            _roomRepo = roomRepo;
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
        }

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

        private async Task<HotelReturnDTO> MapHotelToHotelReturnDTO(Hotel hotel)
        {
            var hotelImages = hotel.HotelImages?.Select(image => image.ImageUrl).ToList() ?? new List<string>();

            List<Room> rooms = new List<Room>();
            try
            {
                // Get a new DbContext instance from the repository
                rooms = (await _roomRepo.Get()).Where(r => r.HotelId == hotel.Id).ToList();
            }
            catch (NoSuchRoomException ex)
            {
                // Handle exception appropriately
            }

            var roomIDs = rooms.Select(r => r.RoomNumber).ToList();

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
                RoomIDs = roomIDs
            };
        }


        public async Task UpdateHotelAverageRatingAsync(int hotelId, double newRating)
        {
            try
            {
                //var hotel = await _hotelRepo.Get(hotelId);
                //if (hotel == null)
                //{
                //    throw new NoSuchHotelException(hotelId);
                //}

                //// Fetch all rooms associated with this hotel to get their ratings
                //var rooms = await _roomRepo.Get();
                //var hotelRooms = rooms.Where(r => r.HotelId == hotel.Id).ToList();

                //// Calculate the new average rating
                //var totalRatings = hotelRooms.Sum(r => r.Ratings);
                //var numberOfRatings = hotelRooms.Count;
                //var newAverageRating = totalRatings / (double)numberOfRatings;

                //hotel.AverageRatings = newAverageRating;
                //await _hotelRepo.Update(hotel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating the hotel's average rating");
                throw;
            }
        }
    }
}
