using HotelBooking.Models;
using HotelServices.Exceptions;
using HotelServices.Interfaces;
using HotelServices.Models.DTOs;
using HotelServices.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace HotelServices.Controllers
{
    [Route("api/")]
    [ApiController]
    [EnableCors("MyCors")]
    public class RoomController :ControllerBase
    {
        private readonly IRoomService _roomService;
        private readonly IAzureBlobService _blobService;
        private readonly ILogger<HotelController> _logger;

        public RoomController(IRoomService roomService, ILogger<HotelController> logger)
        {
            _roomService = roomService;
            _logger = logger;
        }


        // Get All Rooms
        [Authorize]
        [HttpGet("GetAllRooms")]
        [ProducesResponseType(typeof(List<HotelReturnDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<RoomDTO>>> GetAllRooms()
        {
            try
            {
                var result = await _roomService.GetAllRoomsAsync();
                return Ok(result);
            }
            catch (NoSuchRoomException ex)
            {
                _logger.LogError(ex, "Rooms Not found");
                return NotFound(new ErrorModel(404, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving all Rooms.");
                return StatusCode(500, new ErrorModel(500, "An error occurred while processing your request."));
            }
        }


        // Get All available rooms
        [Authorize]
        [HttpGet("GetAvailableHotelsRooms")]
        [ProducesResponseType(typeof(List<RoomDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<RoomDTO>>> GetAvailableHotelsRooms(DateTime checkInDate, DateTime checkOutDate, int numberOfGuests)
        {
            try
            {
                var result = await _roomService.GetAvailableRoomsAsync(checkInDate, checkOutDate, numberOfGuests);
                return Ok(result);
            }
            catch (NoSuchHotelException ex)
            {
                _logger.LogError(ex, "Rooms Not found");
                return NotFound(new ErrorModel(404, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving all Available rooms.");
                return StatusCode(500, new ErrorModel(500, $"An error occurred while processing your request. + {ex.Message}"));
            }
        }

        // Get All Available Hotel rooms by date
        [Authorize]
        [HttpGet("GetAvailableHotelsRoomsByDate")]
        [ProducesResponseType(typeof(List<RoomDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<RoomDTO>>> GetAvailableHotelsRoomsByDate(DateTime checkInDate, DateTime checkOutDate)
        {
            try
            {
                var result = await _roomService.GetAvailableRoomsByDateAsync(checkInDate, checkOutDate);
                return Ok(result);
            }
            catch (NoSuchHotelException ex)
            {
                _logger.LogError(ex, "Rooms Not found");
                return NotFound(new ErrorModel(404, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving all Available rooms.");
                return StatusCode(500, new ErrorModel(500, $"An error occurred while processing your request. + {ex.Message}"));
            }
        }


        // Add Room to Hotel
        [Authorize(Roles = "Admin")]
        [HttpPost("AddRoomToHotel/{hotelId}")]
        [ProducesResponseType(typeof(HotelReturnDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<HotelReturnDTO>> AddRoomToHotel(int hotelId, [FromBody] RoomDTO roomDTO)
        {
            if (roomDTO == null)
            {
                return BadRequest("Room details are not provided.");
            }

            try
            {
                var result = await _roomService.AddRoomToHotelAsync(hotelId, roomDTO);
                return Ok(result);
            }
            catch (NoSuchHotelException ex)
            {
                _logger.LogError(ex, $"Hotel not found for ID: {hotelId}");
                return NotFound(new ErrorModel(404, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while adding the room to the hotel.");
                return StatusCode(500, new ErrorModel(500, $"An error occurred while processing your request {ex.Message}"));
            }
        }


        // remove Room from hotel
        [Authorize(Roles = "Admin")]
        [HttpDelete("RemoveRoomFromHotel")]
        [ProducesResponseType(typeof(HotelReturnDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<HotelReturnDTO>> RemoveRoomFromHotel([FromBody] RemoveDTO removeDTO)
        {
            try
            {
                var result = await _roomService.RemoveRoomFromHotelAsync(removeDTO.hotelId, removeDTO.RoomNumber);
                return Ok(result);
            }
            catch(NoSuchRoomException ex)
            {
                _logger.LogError(ex, $"Room not found for ID: {removeDTO.RoomNumber}");
                return NotFound(new ErrorModel(404, ex.Message));
            }
            catch (NoSuchHotelException ex)
            {
                _logger.LogError(ex, $"Hotel not found for ID: {removeDTO.hotelId}");
                return NotFound(new ErrorModel(404, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while adding the room to the hotel.");
                return StatusCode(500, new ErrorModel(500, $"An error occurred while processing your request {ex.Message}"));
            }
        }


        // remove Room from hotel
        [Authorize(Roles = "Admin")]
        [HttpPost("SoftDeleteRoom")]
        [ProducesResponseType(typeof(HotelReturnDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<HotelReturnDTO>> SoftDeleteRoom([FromBody] int hotelId, int RoomNumber)
        {
            try
            {
                var result = await _roomService.SoftDeleteRoom(hotelId, RoomNumber);
                return Ok(result);
            }
            catch (NoSuchRoomException ex)
            {
                _logger.LogError(ex, $"Room not found for ID: {RoomNumber}");
                return NotFound(new ErrorModel(404, ex.Message));
            }
            catch (NoSuchHotelException ex)
            {
                _logger.LogError(ex, $"Hotel not found for ID: {hotelId}");
                return NotFound(new ErrorModel(404, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while adding the room to the hotel.");
                return StatusCode(500, new ErrorModel(500, $"An error occurred while processing your request {ex.Message}"));
            }
        }

        [Authorize]
        [HttpPost("GetRoomsByHotelID/{hotelId}")]
        [ProducesResponseType(typeof(IList<RoomDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<RoomDTO>>> GetRoomsByHotelID(int hotelId)
        {
            try
            {
                var result = await _roomService.GetRoomByHotelId(hotelId);
                return Ok(result);
            }
            catch (NoSuchRoomException ex)
            {
                _logger.LogError(ex, $"Room not found for HOTELID: {hotelId}");
                return NotFound(new ErrorModel(404, ex.Message));
            }
            catch (NoSuchHotelException ex)
            {
                _logger.LogError(ex, $"Hotel not found for ID: {hotelId}");
                return NotFound(new ErrorModel(404, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while adding the room to the hotel.");
                return StatusCode(500, new ErrorModel(500, $"An error occurred while processing your request {ex.Message}"));
            }
        }

    }
}
