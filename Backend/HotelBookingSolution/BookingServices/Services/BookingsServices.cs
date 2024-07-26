using BookingServices.Exceptions;
using BookingServices.Interfaces;
using BookingServices.Models;
using BookingServices.Models.DTOs;
using CancellationService.Exceptions;
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
            List<int> bookedRoomNumbers = new List<int>();

            try
            {
                var bookings = await _bookingRepo.Get(); // Assumes Get() returns all bookings including details

                bookedRoomNumbers = bookings
                    .Where(b => b.CheckInDate < checkOutDate && b.CheckOutDate > checkInDate) // Filter bookings by date range
                    .SelectMany(b => b.BookingDetails) // Flatten BookingDetails collection
                    .Select(d => d.RoomNumber) // Extract RoomNumber
                    .Distinct() // Ensure room numbers are unique
                    .ToList(); // Convert to list
            }
            catch (NoSuchBookingException ex)
            {
                // Log the exception if needed
                _logger.LogWarning(ex, "No bookings found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving booked rooms");
                throw;
            }

            return bookedRoomNumbers;
        }


        public async Task<BookingReturnDTO> AddBookingAsync(BookingDTO bookingDTO,int currUserId)
        {
            try
            {
                var token = _httpContextAccessor.HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

                await checkUserActiveStatus(currUserId, token);

                // Check if HotelID exists
                var hotelClient = _httpClientFactory.CreateClient("HotelService");
                hotelClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var hotelResponse = await hotelClient.GetAsync($"api/GetHotelByID/{bookingDTO.HotelId}");
                if (!hotelResponse.IsSuccessStatusCode)
                {
                    throw new Exception("Invalid Hotel ID");
                }

                // Check if UserID exists
                var userClient = _httpClientFactory.CreateClient("UserAuthService");
                userClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var userResponse = await userClient.GetAsync($"getuser/{bookingDTO.UserId}");
                if (!userResponse.IsSuccessStatusCode)
                {
                    throw new Exception("Invalid User ID");
                }

                // Fetch available rooms
                var roomClient = _httpClientFactory.CreateClient("HotelService");
                roomClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var formattedCheckInDate = bookingDTO.CheckInDate.ToString("yyyy-MM-dd");
                var formattedCheckOutDate = bookingDTO.CheckOutDate.ToString("yyyy-MM-dd");
                var availableRoomsResponse = await roomClient.GetAsync($"api/GetAvailableHotelsRoomsByDate?checkInDate={formattedCheckInDate}&checkOutDate={formattedCheckOutDate}");

                if (!availableRoomsResponse.IsSuccessStatusCode)
                {
                    var errorMessage = await availableRoomsResponse.Content.ReadAsStringAsync();
                    throw new Exception($"Failed to fetch available rooms. Status Code: {availableRoomsResponse.StatusCode}");
                }

                var availableRoomsJson = await availableRoomsResponse.Content.ReadAsStringAsync();
                var availableRooms = JsonConvert.DeserializeObject<List<RoomDTO>>(availableRoomsJson);

                if (availableRooms == null || !availableRooms.Any())
                {
                    throw new Exception("No rooms available for the specified criteria");
                }

                var roomsInHotel = availableRooms.Where(r => r.HotelId == bookingDTO.HotelId).ToList();

                if (bookingDTO.NumberOfRooms > 4)
                {
                    throw new Exception("Cannot book more than 4 rooms at a time");
                }

                if (bookingDTO.NumberOfGuests > bookingDTO.NumberOfRooms * 3)
                {
                    throw new Exception("The total number of guests exceeds the capacity of the requested number of rooms");
                }

                var allocatedRooms = new List<BookingDetail>();
                var totalGuestsToAllocate = bookingDTO.NumberOfGuests;
                var totalDays = (bookingDTO.CheckOutDate - bookingDTO.CheckInDate).Days;

                for (int i = 0; i < bookingDTO.NumberOfRooms; i++)
                {
                    if (roomsInHotel.Count == 0)
                    {
                        throw new Exception("Not enough rooms available");
                    }

                    var room = roomsInHotel.First();
                    roomsInHotel.RemoveAt(0);

                    var guestsForThisRoom = Math.Min(totalGuestsToAllocate, 3);
                    totalGuestsToAllocate -= guestsForThisRoom;

                    var totalRentForRoom = room.Rent * totalDays;

                    allocatedRooms.Add(new BookingDetail
                    {
                        RoomNumber = room.RoomNumber,
                        Rent = totalRentForRoom,
                        HotelId = room.HotelId
                    });

                    if (totalGuestsToAllocate <= 0)
                        break;
                }

                // Ensure all requested rooms are allocated even if not all guests are distributed
                while (allocatedRooms.Count < bookingDTO.NumberOfRooms && roomsInHotel.Count > 0)
                {
                    var room = roomsInHotel.First();
                    roomsInHotel.RemoveAt(0);

                    var totalRentForRoom = room.Rent * totalDays;

                    allocatedRooms.Add(new BookingDetail
                    {
                        RoomNumber = room.RoomNumber,
                        Rent = totalRentForRoom,
                        HotelId = room.HotelId
                    });
                }

                if (allocatedRooms.Count < bookingDTO.NumberOfRooms)
                {
                    throw new Exception("Could not allocate the requested number of rooms");
                }

                var totalAmount = allocatedRooms.Sum(detail => detail.Rent);

                var booking = new Booking
                {
                    HotelId = bookingDTO.HotelId,
                    UserId = bookingDTO.UserId,
                    CheckInDate = bookingDTO.CheckInDate,
                    CheckOutDate = bookingDTO.CheckOutDate,
                    NumberOfGuests = bookingDTO.NumberOfGuests,
                    TotalAmount = totalAmount,
                    BookingDetails = allocatedRooms // Associate the booking details
                };

                var result = await _bookingRepo.Add(booking);

                return new BookingReturnDTO
                {
                    BookingId = result.Id,
                    HotelId = bookingDTO.HotelId,
                    UserId = bookingDTO.UserId,
                    CheckInDate = bookingDTO.CheckInDate,
                    CheckOutDate = bookingDTO.CheckOutDate,
                    NumberOfGuests = bookingDTO.NumberOfGuests,
                    TotalPrice = totalAmount,
                    RoomNumbers = allocatedRooms.Select(d => d.RoomNumber).ToList() // List all room numbers
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while adding the booking");
                throw;
            }
        }


        public async Task RevertBookingAsync(int bookingId, int currUserId)
        {
            try
            {
                var token = _httpContextAccessor.HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

                // Check if the user is active
                await checkUserActiveStatus(currUserId, token);

                // Fetch the booking details
                var booking = await _bookingRepo.Get(bookingId);
                if (booking == null)
                {
                    throw new Exception("Booking not found");
                }

                // Check if the booking belongs to the current user
                if (booking.UserId != currUserId)
                {
                    throw new UnauthorizedAccessException("You do not have permission to revert this booking");
                }

                // Delete the booking details
                try 
                {
                    var isBookingDetailsDeleted = await _bookingRepo.Delete(bookingId);
                }
                catch(Exception ex)
                {
                    throw ex;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while reverting the booking");
                throw;
            }
        }


        private async Task checkUserActiveStatus(int currUserId, string token)
        {
            var userClient = _httpClientFactory.CreateClient("UserAuthService");
            userClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var userResponse = await userClient.PostAsync($"IsActive/{currUserId}", null);

            if (userResponse.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                throw new InvalidUserException();
            }

            if (!userResponse.IsSuccessStatusCode)
            {
                throw new InvalidUserException("Failed to check user activation status");
            }

            var contentString = await userResponse.Content.ReadAsStringAsync();
            var isActive = JsonConvert.DeserializeObject<bool>(contentString);

            if (!isActive)
            {
                throw new UserNotActiveException();
            }
        }

        public async Task<CancelReturnDTO> CancelBookingByBookingId(int BookingID)
        {
            try
            {
                var booking = await _bookingRepo.Get(BookingID);
                booking.IsCancelled = true;
                var result = await _bookingRepo.Update(booking);
                var returnResult = await MapBookingToCanelReturnDTO(result);
                return returnResult;
            }
            catch(NoSuchBookingException ex)
            {
                throw ex;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        private async Task<CancelReturnDTO> MapBookingToCanelReturnDTO(Booking result)
        {
            CancelReturnDTO cancelReturnDTO = new CancelReturnDTO()
            {
                Id = result.Id,
                HotelId = result.HotelId,
                CheckInDate = result.CheckInDate,
                CheckOutDate = result.CheckOutDate,
                IsCancelled = result.IsCancelled,
                IsPaid = result.IsPaid,
                NumberOfGuests = result.NumberOfGuests,
                TotalAmount = result.TotalAmount,
                UserId = result.UserId,
            };
            return cancelReturnDTO;
        }

        public async Task<List<BookingReturnDTO>> GetAllBookings()
        {
            try
            {
                var bookings = await _bookingRepo.Get();
                return bookings.Select(booking => new BookingReturnDTO
                {
                    BookingId = booking.Id,
                    HotelId = booking.HotelId,
                    UserId = booking.UserId,
                    CheckInDate = booking.CheckInDate,
                    CheckOutDate = booking.CheckOutDate,
                    NumberOfGuests = booking.NumberOfGuests,
                    TotalPrice = booking.TotalAmount,
                    IsCancelled = booking.IsCancelled,
                    IsPaid =booking.IsPaid,
                    RoomNumbers = booking.BookingDetails.Select(d => d.RoomNumber).ToList() 
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving all bookings");
                throw;
            }
        }

        public async Task<BookingByIdReturnDTO> GetBookingByID(int bookingId)
        {
            try
            {
                var booking = await _bookingRepo.Get(bookingId);
                if (booking == null)
                {
                    throw new Exception("Booking not found");
                }

                return new BookingByIdReturnDTO
                {
                    Id = booking.Id,
                    HotelId = booking.HotelId,
                    UserId = booking.UserId,
                    CheckInDate = booking.CheckInDate,
                    CheckOutDate = booking.CheckOutDate,
                    NumberOfGuests = booking.NumberOfGuests,
                    TotalPrice = booking.TotalAmount,
                    IsCancelled =booking.IsCancelled,
                    IsPaid = booking.IsPaid
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving the booking by ID");
                throw;
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
                    BookingId = booking.Id,
                    HotelId = booking.HotelId,
                    UserId = booking.UserId,
                    CheckInDate = booking.CheckInDate,
                    CheckOutDate = booking.CheckOutDate,
                    NumberOfGuests = booking.NumberOfGuests,
                    TotalPrice = booking.TotalAmount,
                    RoomNumbers = booking.BookingDetails.Select(d => d.RoomNumber).ToList() // Populate RoomNumbers
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving the booking by user");
                throw;
            }
        }


    }
}
