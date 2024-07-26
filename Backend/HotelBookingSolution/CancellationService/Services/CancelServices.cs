using CancellationService.Exceptions;
using CancellationService.Interfaces;
using CancellationService.Models;
using CancellationService.Models.DTOs;
using Newtonsoft.Json;
using System.Net.Http.Headers;


namespace CancellationService.Services
{
    public class CancelServices : ICancelService
    {
        private readonly IRepository<int, Cancel> _cancelRepo;
        private readonly IRepository<int, Refund> _refundRepo;
        private readonly ILogger<CancelServices> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CancelServices(IRepository<int, Cancel> cancelRepo, ILogger<CancelServices> logger, 
                IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor,
                    IRepository<int, Refund> refundRepo)
        {
            _refundRepo = refundRepo;
            _cancelRepo = cancelRepo;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
        }
        public async Task<CancelReturnDTO> CancelTheBooking(int BookingId,int currUserId)
        {
            try
            {
                var token = _httpContextAccessor.HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

                //Check if User is Active
                await checkUserActiveStatus(currUserId,token);


                // Check if HotelID exists
                var bookingClient = _httpClientFactory.CreateClient("BookingService");
                bookingClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var bookingResponse = await bookingClient.GetAsync($"api/GetBookingByID/{BookingId}");
                if (!bookingResponse.IsSuccessStatusCode)
                {
                    throw new Exception("Invalid Booking ID");
                }

                var bookingResponseJson = await bookingResponse.Content.ReadAsStringAsync();
                var bookingResponseByID = JsonConvert.DeserializeObject<BookingDTO>(bookingResponseJson);

                if (bookingResponseByID.IsCancelled)
                {
                    throw new BookingAlreadyCancelledException();
                }

                if(bookingResponseByID.UserId != currUserId)
                {
                    throw new UnAuthotizedToCancelException();
                }

                // Calculate refund amount based on cancellation time
                decimal refundPercentage = GetRefundPercentage(bookingResponseByID.CheckInDate, DateTime.Now);
                decimal refundAmount = bookingResponseByID.TotalPrice * refundPercentage / 100;


                Refund refund = new Refund()
                {
                    RefundAmount = refundAmount,
                    RefundPaymentMode = "Source Payment Mode",
                    RefundStatus = "Success"
                };

                // Cancel Booking
                var bookingClient2 = _httpClientFactory.CreateClient("BookingService");
                bookingClient2.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var cancelResponseFromBookingService = await bookingClient2.PostAsync($"api/CancelBookingByBookingId/{BookingId}",null);
                if (!cancelResponseFromBookingService.IsSuccessStatusCode)
                {
                    var errorContent = await cancelResponseFromBookingService.Content.ReadAsStringAsync();
                    throw new Exception($"Failed to cancel booking. Status code: {cancelResponseFromBookingService.StatusCode}. Error: {errorContent}");
                }

                var refundResult = await _refundRepo.Add(refund);

                Cancel cancel = new Cancel()
                {
                    RefundId = refundResult.Id,
                    BookingId = BookingId,
                    CancelledOn = DateTime.Now
                };

                var cancelResponse = await _cancelRepo.Add(cancel);

                // Check the number of cancellations for the user
                var userId = bookingResponseByID.UserId;

                var bookingClient3 = _httpClientFactory.CreateClient("BookingService");
                bookingClient3.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var allBookingsResponse = await bookingClient3.GetAsync("api/GetAllBookings");
                if (!allBookingsResponse.IsSuccessStatusCode)
                {
                    throw new Exception("Failed to retrieve bookings");
                }

                var allBookingsJson = await allBookingsResponse.Content.ReadAsStringAsync();
                var allBookings = JsonConvert.DeserializeObject<List<BookingDTO>>(allBookingsJson);

                var userCancellations = allBookings.Count(b => b.UserId == userId && b.IsCancelled);

                if (userCancellations > 2)
                {
                    await DeactivateUser(userId);
                }

                var cancelReturn = await MapCancelToCancelReturnDTO(cancelResponse, refundResult);
                return cancelReturn;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private async Task checkUserActiveStatus(int currUserId, string token)
        {
            var userClient = _httpClientFactory.CreateClient("UserService");
            userClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var userResponse = await userClient.PostAsync($"IsActive/{currUserId}",null);

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

        private async Task<CancelReturnDTO> MapCancelToCancelReturnDTO(Cancel cancelResponse, Refund refundResult)
        {
            CancelReturnDTO cancelReturn = new CancelReturnDTO()
            {
                CancelledOn = cancelResponse.CancelledOn,
                RefundStatus = refundResult.RefundStatus,
                BookingId = cancelResponse.BookingId,
                RefundAmount = refundResult.RefundAmount,
                RefundId = cancelResponse.RefundId,
                RefundPaymentMode = refundResult.RefundPaymentMode,
                Id = cancelResponse.Id
            };
            return cancelReturn;
        }

        private decimal GetRefundPercentage(DateTime checkInDate, DateTime cancellationDate)
        {
            TimeSpan timeDifference = checkInDate - cancellationDate;
            double hoursDifference = timeDifference.TotalHours;

            if (hoursDifference >= 100)
            {
                return 100; // Full refund
            }
            else if (hoursDifference >= 75)
            {
                return 75;
            }
            else if (hoursDifference >= 50)
            {
                return 50;
            }
            else if (hoursDifference >= 25)
            {
                return 25;
            }
            else if (hoursDifference >= 10)
            {
                return 10;
            }
            else
            {
                return 0; // No refund
            }
        }

        private async Task DeactivateUser(int userId)
        {
            var userClient = _httpClientFactory.CreateClient("UserService");
            var deactivateResponse = await userClient.PostAsync($"DeactivateUser/{userId}", null);
            if (!deactivateResponse.IsSuccessStatusCode)
            {
                throw new Exception("Failed to deactivate user");
            }
        }

    }
}
