using BookingServices.Interfaces;
using BookingServices.Models.DTOs;
using HotelBooking.Models;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BookingServices.Controllers
{
    [Route("api/")]
    [ApiController]
    [EnableCors("MyCors")]
    public class BookingController :ControllerBase
    {
        private readonly IBookingServices _bookingServices;
        private readonly ILogger<BookingController> _logger;

        public BookingController(IBookingServices bookingServices, ILogger<BookingController> logger)
        {
            _bookingServices = bookingServices;
            _logger = logger;
        }

        [HttpGet("GetBookedRooms")]
        [ProducesResponseType(typeof(List<int>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<int>>> GetBookedRoomNumbers(DateTime checkInDate, DateTime checkOutDate)
        {
            if(ModelState.IsValid)
            {
                try
                {
                    var userId = Convert.ToInt32(User.FindFirstValue(ClaimTypes.Name));
                    var result = await _bookingServices.GetBookedRoomNumbersAsync(checkInDate,checkOutDate);
                    return Ok(result);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while adding the booking.");
                    return StatusCode(500, new ErrorModel(500, $"An error occurred while processing your request. + {ex.Message}"));
                }
            }
            return BadRequest("All Details are not provided");
        }

        // Add Booking
        [HttpPost("AddBooking")]
        [ProducesResponseType(typeof(BookingReturnDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<BookingReturnDTO>> AddBooking([FromBody] bookingInputDTO bookingInputDTO)
        {
            try
            {
                var userId = Convert.ToInt32(User.FindFirstValue(ClaimTypes.Name));
                BookingDTO bookingDTO = await MapBookingInputDTOToBookingDTO(bookingInputDTO, userId);
                var result = await _bookingServices.AddBookingAsync(bookingDTO);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while adding the booking.");
                return StatusCode(500, new ErrorModel(500, "An error occurred while processing your request."));
            }
            return BadRequest("All Details are not provided");
        }

        private async Task<BookingDTO> MapBookingInputDTOToBookingDTO(bookingInputDTO bookingInputDTO,int userId)
        {
            BookingDTO bookingDTO = new BookingDTO()
            {
                UserId = userId,
                CheckInDate = bookingInputDTO.CheckInDate,
                CheckOutDate = bookingInputDTO.CheckOutDate,
                HotelId = bookingInputDTO.HotelId,
                NumberOfGuests = bookingInputDTO.NumberOfGuests,
                TotalPrice = bookingInputDTO.TotalPrice,
                RoomNumber = bookingInputDTO.RoomNumber,
            };
            return bookingDTO;
        }

        // Get All Bookings
        [HttpGet("GetAllBookings")]
        [ProducesResponseType(typeof(List<BookingReturnDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<BookingReturnDTO>>> GetAllBookings()
        {
            try
            {
                var result = await _bookingServices.GetAllBookings();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving all bookings.");
                return StatusCode(500, new ErrorModel(500, "An error occurred while processing your request."));
            }
        }

        // Get Booking by ID
        [HttpGet("GetBookingByID/{bookingId}")]
        [ProducesResponseType(typeof(BookingReturnDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<BookingReturnDTO>> GetBookingByID(int bookingId)
        {
            try
            {
                var result = await _bookingServices.GetBookingByID(bookingId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving the booking by ID.");
                return StatusCode(500, new ErrorModel(500, "An error occurred while processing your request."));
            }
        }

        // Get Booking by User
        [HttpGet("GetBookingByUser/{userId}")]
        [ProducesResponseType(typeof(BookingReturnDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<BookingReturnDTO>> GetBookingByUser(int userId)
        {
            try
            {
                var result = await _bookingServices.GetBookingByUser(userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving the booking by user.");
                return StatusCode(500, new ErrorModel(500, "An error occurred while processing your request."));
            }
        }
    }
}
