using BookingServices.Exceptions;
using BookingServices.Interfaces;
using BookingServices.Models;
using BookingServices.Models.DTOs;
using CancellationService.Exceptions;
using HotelBooking.Interfaces;
using HotelServices.Models.DTOs;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using System.Text;

namespace BookingServices.Services
{
    public class BookingsServices :IBookingServices
    {

        //INITIALIZATION

        private readonly IRepository<int, Booking> _bookingRepo;
        private readonly ILogger<BookingsServices> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        //DEPENDENCY INJECTION
        public BookingsServices(IRepository<int, Booking> bookingRepo, ILogger<BookingsServices> logger, IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
        {
            _bookingRepo = bookingRepo;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
        }

        // ADD BOOKING
        public async Task<BookingReturnDTO> AddBookingAsync(BookingDTO bookingDTO, int currUserId)
        {
            try
            {
                var token = _httpContextAccessor.HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                
                //CHECK USER ACTIVE STATUS
                await checkUserActiveStatus(currUserId, token);

                //CHECK HOTEL EXIST OR NOT
                await CheckHotelExistance(bookingDTO,token);

                int userAvailableCoins=  await CheckUserExistanceAndGetAvailableCoins(bookingDTO, token);

                var userClient = _httpClientFactory.CreateClient("UserAuthService");
                userClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                //BEST ROOM COMBINATION
                var roomCombinationClient = _httpClientFactory.CreateClient("HotelService");
                roomCombinationClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var requestBody = new
                {
                    HotelId = bookingDTO.HotelId,
                    CheckInDate = bookingDTO.CheckInDate.ToString("yyyy-MM-dd"),
                    CheckOutDate = bookingDTO.CheckOutDate.ToString("yyyy-MM-dd"),
                    NumOfGuests = bookingDTO.NumberOfGuests,
                    NumOfRooms = bookingDTO.NumberOfRooms
                };

                var requestContent = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");

                var roomCombinationResponse = await roomCombinationClient.PostAsync("api/BestRoomCombination", requestContent);

                if (!roomCombinationResponse.IsSuccessStatusCode)
                {
                    var errorMessage = await roomCombinationResponse.Content.ReadAsStringAsync();
                    throw new Exception($"Failed to fetch room combination. Status Code: {roomCombinationResponse.StatusCode}, Message: {errorMessage}");
                }

                var roomCombinationJson = await roomCombinationResponse.Content.ReadAsStringAsync();
                var bestCombination = JsonConvert.DeserializeObject<List<RoomDTO>>(roomCombinationJson);

                if (bestCombination == null || !bestCombination.Any())
                {
                    throw new Exception("Could not find a suitable combination of rooms");
                }

                //PRICE CALCULATION
                var totalDays = (bookingDTO.CheckOutDate - bookingDTO.CheckInDate).Days;
                var allocatedRooms = new List<BookingDetail>();

                foreach (var room in bestCombination)
                {
                    var totalRentForRoom = room.Rent * totalDays;
                    allocatedRooms.Add(new BookingDetail
                    {
                        RoomNumber = room.RoomNumber,
                        Rent = totalRentForRoom,
                        HotelId = room.HotelId
                    });
                }

                var totalAmount = allocatedRooms.Sum(detail => detail.Rent);

                var discount = await calculateDiscount(bookingDTO, totalAmount, bookingDTO.UserId);

                var finalAmount = totalAmount - discount - userAvailableCoins;

                var booking = await MapBookingDTOToBooking(bookingDTO, finalAmount, discount, totalAmount, allocatedRooms);

                var result = await _bookingRepo.Add(booking);

                int CoinsEarned = (int)(totalAmount / 100);

                // Update CoinsEarned in the UserService
                UpdateCoinsDTO updateCoinsDTO = new UpdateCoinsDTO
                {
                    UserId = bookingDTO.UserId,
                    CoinsEarned = CoinsEarned
                };

                var updateCoinsRequestContent = new StringContent(JsonConvert.SerializeObject(updateCoinsDTO), Encoding.UTF8, "application/json");

                var updateCoinsResponse = await userClient.PostAsync("UpdateUserCoins", updateCoinsRequestContent);
                if (!updateCoinsResponse.IsSuccessStatusCode)
                {
                    var errorMessage = await updateCoinsResponse.Content.ReadAsStringAsync();
                    throw new Exception($"Failed to update user coins. Status Code: {updateCoinsResponse.StatusCode}, Message: {errorMessage}");
                }

                var bookingReturnDTO = await MapBookingToBookingReturnDTO(booking, allocatedRooms);
                return bookingReturnDTO;
            }
            catch(InvalidUserException ex)
            {
                throw ex;
            }
            catch(UserNotActiveException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while adding the booking");
                throw ex;
            }
        }

        private async Task<BookingReturnDTO> MapBookingToBookingReturnDTO(Booking booking, List<BookingDetail> allocatedRooms)
        {
            return new BookingReturnDTO
            {
                BookingId = booking.Id,
                HotelId = booking.HotelId,
                UserId = booking.UserId,
                CheckInDate = booking.CheckInDate,
                CheckOutDate = booking.CheckOutDate,
                NumberOfGuests = booking.NumberOfGuests,
                TotalPrice = booking.TotalAmount,
                FinalAmount = booking.FinalAmount,
                Discount = booking.Discount,
                RoomNumbers = allocatedRooms.Select(d => d.RoomNumber).ToList()
            };
        }

        private async Task<Booking> MapBookingDTOToBooking(BookingDTO bookingDTO, decimal finalAmount, decimal discount, decimal totalAmount, List<BookingDetail> allocatedRooms)
        {
            var booking = new Booking
            {
                HotelId = bookingDTO.HotelId,
                UserId = bookingDTO.UserId,
                CheckInDate = bookingDTO.CheckInDate,
                CheckOutDate = bookingDTO.CheckOutDate,
                NumberOfGuests = bookingDTO.NumberOfGuests,
                TotalAmount = totalAmount,
                Discount = discount,
                FinalAmount = finalAmount,
                BookingDetails = allocatedRooms
            };
            return booking;
        }

        private async Task<int> CheckUserExistanceAndGetAvailableCoins(BookingDTO bookingDTO, string token)
        {
            // Check if UserID exists
            var userClient = _httpClientFactory.CreateClient("UserAuthService");
            userClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var userResponse = await userClient.GetAsync($"getuser/{bookingDTO.UserId}");
            if (!userResponse.IsSuccessStatusCode)
            {
                throw new Exception("Invalid User ID");
            }

            //GET USER COINS AVAILABLE FOR REDEEMING
            var userJson = await userResponse.Content.ReadAsStringAsync();
            var user = JsonConvert.DeserializeObject<UserDTO>(userJson);

            if (user == null)
            {
                throw new Exception("User data could not be fetched");
            }
            int userAvailableCoins = user.CoinsEarned ?? 0;
            return userAvailableCoins;
        }

        private async Task CheckHotelExistance(BookingDTO bookingDTO, string token)
        {
            // Check if HotelID exists
            var hotelClient = _httpClientFactory.CreateClient("HotelService");
            hotelClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var hotelResponse = await hotelClient.GetAsync($"api/GetHotelByID/{bookingDTO.HotelId}");
            if (!hotelResponse.IsSuccessStatusCode)
            {
                throw new Exception("Invalid Hotel ID");
            }
        }


        //GET BOOKED ROOM NUMBERS
        public async Task<List<int>> GetBookedRoomNumbersAsync(DateTime checkInDate, DateTime checkOutDate)
        {
            List<int> bookedRoomNumbers = new List<int>();

            try
            {
                var bookings = await _bookingRepo.Get(); 

                bookedRoomNumbers = bookings
                    .Where(b => b.CheckInDate < checkOutDate && b.CheckOutDate > checkInDate) 
                    .SelectMany(b => b.BookingDetails) 
                    .Select(d => d.RoomNumber) 
                    .Distinct()
                    .ToList(); 
            }
            catch (NoSuchBookingException ex)
            {
                _logger.LogWarning(ex, "No bookings found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving booked rooms");
                throw;
            }

            return bookedRoomNumbers;
        }

        //GET CANCELLATION COUNT FOR THE USER
        public async Task<int> GetCancellationCount(int userId)
        {
            try
            {
                var allBookings = await _bookingRepo.Get();
                var userCancellations = allBookings.Count(b => b.UserId == userId && b.IsCancelled);
                return userCancellations;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        //CALCULATE DISCOUNT
        private async Task<decimal> calculateDiscount(BookingDTO bookingDTO, decimal totalAmount, int userId)
        {
            var userBookings = new List<Booking>();
            bool isNewUser;
            try
            {
                userBookings = (await _bookingRepo.Get()).Where(b => b.UserId == userId).ToList();
                isNewUser = !userBookings.Any();
            }
            catch(NoSuchBookingException ex)
            {
                isNewUser = true;
            }

            decimal discountAmount = 0;

            if (isNewUser)
            {
                discountAmount += totalAmount * 0.12m;
            }
            if ((bookingDTO.CheckInDate - DateTime.Now).Days >= 30)
            {
                if (bookingDTO.NumberOfRooms >= 5 || bookingDTO.NumberOfGuests >= 10)
                {
                    discountAmount += totalAmount * 0.10m;
                }
                else if (bookingDTO.NumberOfRooms >= 3 || bookingDTO.NumberOfGuests >= 5)
                {
                    discountAmount += totalAmount * 0.05m;
                }
                else
                {
                    discountAmount += totalAmount * 0.03m;
                }
            }
            return discountAmount;
        }


        //CALCULATE TOTAL AMOUNT
        public async Task<CalculateAmountDTO> CalculateTotalAmountAsync(BestCombinationDTO bestCombinationDTO,int userId)
        {
            var userBookings = new List<Booking>();
            bool isNewUser;

            try
            {
                userBookings = (await _bookingRepo.Get()).Where(b => b.UserId == userId).ToList();
                isNewUser = !userBookings.Any();
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred while retrieving bookings for user {userId}: {ex.Message}");
                isNewUser = true;
            }

            var roomCombinationClient = _httpClientFactory.CreateClient("HotelService");
            var requestBody = new
            {
                HotelId = bestCombinationDTO.HotelId,
                CheckInDate = bestCombinationDTO.CheckInDate.ToString("yyyy-MM-dd"),
                CheckOutDat = bestCombinationDTO.CheckOutDat.ToString("yyyy-MM-dd"),
                NumOfGuests = bestCombinationDTO.NumOfGuests,
                NumOfRooms = bestCombinationDTO.NumOfRooms
            };

            var token = _httpContextAccessor.HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            roomCombinationClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var requestContent = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");
            var roomCombinationResponse = await roomCombinationClient.PostAsync("api/BestRoomCombination", requestContent);

            if (!roomCombinationResponse.IsSuccessStatusCode)
            {
                var errorMessage = await roomCombinationResponse.Content.ReadAsStringAsync();
                throw new Exception($"Failed to fetch room combination. Status Code: {roomCombinationResponse.StatusCode}, Message: {errorMessage}");
            }

            var roomCombinationJson = await roomCombinationResponse.Content.ReadAsStringAsync();
            var bestCombination = JsonConvert.DeserializeObject<List<RoomDTO>>(roomCombinationJson);

            if (bestCombination == null || !bestCombination.Any())
            {
                throw new Exception("Could not find a suitable combination of rooms");
            }

            var totalDays = (bestCombinationDTO.CheckOutDat - bestCombinationDTO.CheckInDate).Days;
            var totalAmount = bestCombination.Sum(room => room.Rent * totalDays);

            decimal discountAmount = 0;

            if (isNewUser)
            {
                discountAmount += totalAmount * 0.12m;
            }

            if ((bestCombinationDTO.CheckInDate - DateTime.Now).Days >= 30)
            {
                if (bestCombinationDTO.NumOfRooms >= 5 || bestCombinationDTO.NumOfGuests >= 10)
                {
                    discountAmount += totalAmount * 0.10m;
                }
                else if (bestCombinationDTO.NumOfRooms >= 3 || bestCombinationDTO.NumOfGuests >= 5)
                {
                    discountAmount += totalAmount * 0.05m;
                }
                else
                {
                    discountAmount += totalAmount * 0.03m;
                }
            }
            decimal finalTotalAmount = totalAmount - discountAmount;

            CalculateAmountDTO calculateAmountDTO = new CalculateAmountDTO
            {
                TotalAmount = totalAmount,
                FinalAmount = finalTotalAmount,
                Discount = discountAmount
            };

            return calculateAmountDTO;
        }

        //REVERT BOOKING
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


        //CHEK USER ACTIVE STATUS
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

        //CANCEL BOOKING BY BOOKING ID
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

        //MAP BOOKING TO CANCEL RETURN DTO
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

        
        //GET ALL BOOKINGS
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

        //GET BOOKING BY BOOKING ID
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
                    IsPaid = booking.IsPaid,
                    FinalAmount = booking.FinalAmount,
                    Discount = booking.Discount,
                    RoomNumbers =  booking.BookingDetails.Select(d => d.RoomNumber).ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving the booking by ID");
                throw;
            }
        }

        //GET BOOKING BY USER ID
        public async Task<List<BookingReturnDTO>> GetBookingByUser(int userId)
        {
            try
            {
                // Fetch all bookings
                var bookings = await _bookingRepo.Get();

                // Filter bookings for the specified user
                var userBookings = bookings.Where(b => b.UserId == userId).ToList();

                if (!userBookings.Any())
                {
                    throw new Exception("No bookings found for this user");
                }

                // Map to BookingReturnDTO
                var bookingDtos = userBookings.Select(booking => new BookingReturnDTO
                {
                    BookingId = booking.Id,
                    HotelId = booking.HotelId,
                    UserId = booking.UserId,
                    CheckInDate = booking.CheckInDate,
                    CheckOutDate = booking.CheckOutDate,
                    NumberOfGuests = booking.NumberOfGuests,
                    TotalPrice = booking.TotalAmount,
                    FinalAmount = booking.FinalAmount,
                    Discount = booking.Discount,
                    IsCancelled = booking.IsCancelled,
                    IsPaid = booking.IsPaid,
                    RoomNumbers = booking.BookingDetails.Select(d => d.RoomNumber).ToList() 
                }).ToList();

                return bookingDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving bookings for the user");
                throw;
            }
        }

    }
}
