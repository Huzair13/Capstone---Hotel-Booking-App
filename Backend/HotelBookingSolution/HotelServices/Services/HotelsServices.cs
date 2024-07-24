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
        private readonly IRepository<int, HotelRoom> _hotelRoomRepo;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HotelsServices(IRepository<int, Hotel> hotelRepo, ILogger<HotelsServices> logger,
                                IRepository<int, Room> roomRepo, IRepository<int, HotelRoom> hotelRoomRepo,
                                   IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
        {
            _hotelRepo = hotelRepo;
            _logger = logger;
            _roomRepo = roomRepo;
            _hotelRoomRepo = hotelRoomRepo;
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<HotelReturnDTO> AddHotelAsync(HotelDTO hotelDTO)
        {
            try
            {
                Hotel hotel = MapHotelDTOToHotel(hotelDTO);
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
                    AllowedNumOfGuests = roomDTO.AllowedNumOfGuests,
                    Rent = roomDTO.Rent,
                    IsDeleted = false
                };

                var resultRoom = await _roomRepo.Add(room);

                HotelRoom hotelRoom = new HotelRoom
                {
                    RoomID = resultRoom.RoomNumber,
                    HotelID = hotelId,
                    Room = resultRoom,
                    Hotel = hotel
                };

                await _hotelRoomRepo.Add(hotelRoom);

                return await MapHotelToHotelReturnDTO(hotel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while adding a room to the hotel");
                throw;
            }
        }

        public async Task<List<HotelReturnDTO>> GetAllHotels()
        {
            try
            {
                var hotels = await _hotelRepo.Get();
                var hotelDTOs = await Task.WhenAll(hotels.Select(h => MapHotelToHotelReturnDTO(h)));
                return hotelDTOs.ToList();
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

        // In HotelServices.Services.HotelsServices

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
                    _logger.LogError($"Error fetching booked rooms: {errorMessage}");
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
                    RoomNumber = r.RoomNumber,
                    RoomType = r.RoomType,
                    RoomFloor = r.RoomFloor,
                    AllowedNumOfGuests = r.AllowedNumOfGuests,
                    Rent = r.Rent,
                    HotelId = r.HotelRooms.FirstOrDefault()?.HotelID ?? 0 // Assuming one HotelRoom per Room
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving available rooms");
                throw;
            }
        }

        private Hotel MapHotelDTOToHotel(HotelDTO hotelDTO)
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
            // Ensure HotelRooms and HotelImages are initialized
            var roomIDs = hotel.HotelRooms?.Select(hr => hr.RoomID).ToList() ?? new List<int>();
            var hotelImages = hotel.HotelImages?.Select(image => image.ImageUrl).ToList() ?? new List<string>();

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

    }
}
