using BookingServices.Interfaces;
using BookingServices.Models;
using BookingServices.Models.DTOs;
using HotelBooking.Interfaces;
using HotelServices.Models.DTOs;
using Newtonsoft.Json;
using System.Net.Http.Headers;

namespace BookingServices.Services
{
    public class BookingsServices :IBookingServices
    {
        private readonly IRepository<int, Booking> _bookingRepo;
        private readonly ILogger<BookingsServices> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public BookingsServices(IRepository<int, Booking> bookingRepo, ILogger<BookingsServices> logger, IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
        {
            _bookingRepo = bookingRepo;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<List<int>> GetBookedRoomNumbersAsync(DateTime checkInDate, DateTime checkOutDate)
        {
            try
            {
                var bookings = await _bookingRepo.Get();
                var bookedRoomNumbers = bookings
                    .Where(b => (b.CheckInDate < checkOutDate) && (b.CheckOutDate > checkInDate))
                    .Select(b => b.RoomNumber)
                    .Distinct()
                    .ToList();

                return bookedRoomNumbers;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving booked rooms");
                throw;
            }
        }


        public async Task<BookingReturnDTO> AddBookingAsync(BookingDTO bookingDTO)
        {
            try
            {
                // Retrieve the token from the incoming HTTP request
                var token = _httpContextAccessor.HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

                // Check if HotelID exists in Hotel Service
                var hotelClient = _httpClientFactory.CreateClient("HotelService");
                hotelClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var hotelResponse = await hotelClient.GetAsync($"api/GetHotelByID/{bookingDTO.HotelId}");
                if (!hotelResponse.IsSuccessStatusCode)
                {
                    throw new Exception("Invalid Hotel ID");
                }

                // Check if UserID exists in UserAuth Service
                var userClient = _httpClientFactory.CreateClient("UserAuthService");
                userClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var userResponse = await userClient.GetAsync($"getuser/{bookingDTO.UserId}");
                if (!userResponse.IsSuccessStatusCode)
                {
                    throw new Exception("Invalid User ID");
                }

                // Fetch available rooms for the specified date range and number of guests from Hotel Service
                var roomClient = _httpClientFactory.CreateClient("HotelService");
                roomClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var formattedCheckInDate = bookingDTO.CheckInDate.ToString("yyyy-MM-dd");
                var formattedCheckOutDate = bookingDTO.CheckOutDate.ToString("yyyy-MM-dd");
                var availableRoomsResponse = await roomClient.GetAsync($"api/GetAvailableHotelsRooms?checkInDate={formattedCheckInDate}&checkOutDate={formattedCheckOutDate}&numberOfGuests={bookingDTO.NumberOfGuests}");

                if (!availableRoomsResponse.IsSuccessStatusCode)
                {
                    var errorMessage = await availableRoomsResponse.Content.ReadAsStringAsync();
                    _logger.LogError($"Error fetching available rooms: {errorMessage}");
                    throw new Exception($"Failed to fetch available rooms. Status Code: {availableRoomsResponse.StatusCode}");
                }

                var availableRoomsJson = await availableRoomsResponse.Content.ReadAsStringAsync();
                var availableRooms = JsonConvert.DeserializeObject<List<RoomDTO>>(availableRoomsJson);

                if (availableRooms == null || !availableRooms.Any())
                {
                    throw new Exception("No rooms available for the specified criteria");
                }

                _logger.LogInformation("Available rooms fetched: " + string.Join(", ", availableRooms.Select(r => $"{r.RoomNumber} (Hotel: {r.HotelId})")));

                // Filter rooms by the hotel ID
                var roomsInHotel = availableRooms.Where(r => r.HotelId == bookingDTO.HotelId).ToList();
                _logger.LogInformation("Rooms in specified hotel: " + string.Join(", ", roomsInHotel.Select(r => $"{r.RoomNumber}")));

                // Select a room
                RoomDTO allocatedRoom = null;
                if (bookingDTO.RoomNumber.HasValue)
                {
                    // Check if the specific room is available
                    allocatedRoom = roomsInHotel.FirstOrDefault(r => r.RoomNumber == bookingDTO.RoomNumber);
                    if (allocatedRoom == null)
                    {
                        throw new Exception("The specified room is not available");
                    }
                }
                else
                {
                    // Allocate a random available room from the correct hotel
                    allocatedRoom = roomsInHotel.FirstOrDefault();
                    if (allocatedRoom == null)
                    {
                        throw new Exception("No available rooms found in the specified hotel");
                    }
                }

                _logger.LogInformation($"Allocated room: {allocatedRoom.RoomNumber} (Hotel: {allocatedRoom.HotelId})");

                // Create the booking
                Booking booking = new Booking
                {
                    HotelId = bookingDTO.HotelId,
                    UserId = bookingDTO.UserId,
                    RoomNumber = allocatedRoom.RoomNumber,
                    CheckInDate = bookingDTO.CheckInDate,
                    CheckOutDate = bookingDTO.CheckOutDate,
                    NumberOfGuests = bookingDTO.NumberOfGuests,
                    TotalAmount = bookingDTO.TotalPrice
                };

                var result = await _bookingRepo.Add(booking);
                return new BookingReturnDTO
                {
                    Id = result.Id,
                    HotelId = result.HotelId,
                    UserId = result.UserId,
                    RoomNumber = result.RoomNumber,
                    CheckInDate = result.CheckInDate,
                    CheckOutDate = result.CheckOutDate,
                    NumberOfGuests = result.NumberOfGuests,
                    TotalPrice = result.TotalAmount
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred at AddBooking service");
                throw new Exception(ex.Message);
            }
        }

        public async Task<List<BookingReturnDTO>> GetAllBookings()
        {
            try
            {
                var bookings = await _bookingRepo.Get();
                return bookings.Select(booking => new BookingReturnDTO
                {
                    Id = booking.Id,
                    HotelId = booking.HotelId,
                    UserId = booking.UserId,
                    CheckInDate = booking.CheckInDate,
                    CheckOutDate = booking.CheckOutDate,
                    NumberOfGuests = booking.NumberOfGuests,
                    TotalPrice = booking.TotalAmount
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving all bookings");
                throw new Exception(ex.Message);
            }
        }

        public async Task<BookingReturnDTO> GetBookingByID(int bookingId)
        {
            try
            {
                var booking = await _bookingRepo.Get(bookingId);
                if (booking == null)
                {
                    throw new Exception("Booking not found");
                }
                return new BookingReturnDTO
                {
                    Id = booking.Id,
                    HotelId = booking.HotelId,
                    UserId = booking.UserId,
                    CheckInDate = booking.CheckInDate,
                    CheckOutDate = booking.CheckOutDate,
                    NumberOfGuests = booking.NumberOfGuests,
                    TotalPrice = booking.TotalAmount
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving the booking by ID");
                throw new Exception(ex.Message);
            }
        }

        public async Task<BookingReturnDTO> GetBookingByUser(int userId)
        {
            try
            {
                var bookings = await _bookingRepo.Get();
                var booking = bookings.FirstOrDefault(b => b.UserId == userId);
                if (booking == null)
                {
                    throw new Exception("Booking not found");
                }
                return new BookingReturnDTO
                {
                    Id = booking.Id,
                    HotelId = booking.HotelId,
                    UserId = booking.UserId,
                    CheckInDate = booking.CheckInDate,
                    CheckOutDate = booking.CheckOutDate,
                    NumberOfGuests = booking.NumberOfGuests,
                    TotalPrice = booking.TotalAmount
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving the booking by user");
                throw new Exception(ex.Message);
            }
        }
    }
}
