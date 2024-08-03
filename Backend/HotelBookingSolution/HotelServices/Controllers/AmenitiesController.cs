using HotelBooking.Models;
using HotelServices.Exceptions;
using HotelServices.Interfaces;
using HotelServices.Models;
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
    public class AmenitiesController :ControllerBase
    {
        private readonly IAmenityService _amenitiesService;
        private readonly ILogger<HotelController> _logger;

        public AmenitiesController(IAmenityService amenitiesService, ILogger<HotelController> logger)
        {
            _amenitiesService = amenitiesService;
            _logger = logger;
        }

        // Get All Amenities
        [Authorize]
        [HttpGet("GetAllAmenities")]
        [ProducesResponseType(typeof(IEnumerable<Amenity>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<Amenity>>> GetAllAmenities()
        {
            try
            {
                var result = await _amenitiesService.GetAllAmenities();
                return Ok(result);
            }
            catch (NoSuchAmenityFound ex)
            {
                _logger.LogError(ex, "Amenities Not found");
                return NotFound(new ErrorModel(404, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving all Amenities.");
                return StatusCode(500, new ErrorModel(500, $"An error occurred while processing your request {ex.Message}"));
            }
        }

        //ADD AMENITIES TO HOTEL
        [Authorize(Roles = "Admin")]
        [HttpPost("AddAmenitiesToHotel")]
        [ProducesResponseType(typeof(List<HotelReturnDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Hotel>> AddAmenitiesToHotel(AddAmenitiesToHotelDTO addAmenitiesToHotelDTO)
        {
            try
            {
                var result = await _amenitiesService.AddAmenitiesToHotelAsync(addAmenitiesToHotelDTO);
                return Ok(result);
            }
            catch (NoSuchHotelException ex)
            {
                _logger.LogError(ex, "Hotels Not found");
                return NotFound(new ErrorModel(404, ex.Message));
            }
            catch(NoSuchAmenityFound ex)
            {
                _logger.LogError(ex, "Amenity Not found");
                return NotFound(new ErrorModel(404, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while adding amenties to the room ");
                return StatusCode(500, new ErrorModel(500, $"An error occurred while processing your request {ex.Message}"));
            }
        }

        //ADD AMENITIES
        [Authorize(Roles = "Admin")]
        [HttpPost("AddAmenity")]
        [ProducesResponseType(typeof(List<AmenityDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<AmenityDTO>> AddAmenity(AmenityDTO amenityDTO)
        {
            try
            {
                var result = await _amenitiesService.AddAmenityAsync(amenityDTO);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while adding amenty ");
                return StatusCode(500, new ErrorModel(500, $"An error occurred while processing your request {ex.Message}"));
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("DeleteAmenityFromHotel")]
        [ProducesResponseType(typeof(List<HotelReturnDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Room>> DeleteAmenityFromHotel(DeleteAmenityDTO deleteAmenityDTO)
        {
            try
            {
                var result = await _amenitiesService.DeleteAmenityFromHotelAsync(deleteAmenityDTO);
                return Ok(result);
            }
            catch(NoSuchAmenityFound ex)
            {
                _logger.LogError(ex, "Amenities Not found");
                return NotFound(new ErrorModel(404, ex.Message));
            }
            catch (NoSuchHotelException ex)
            {
                _logger.LogError(ex, "Hotels Not found");
                return NotFound(new ErrorModel(404, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while adding amenties to the room ");
                return StatusCode(500, new ErrorModel(500, $"An error occurred while processing your request {ex.Message}"));
            }
        }
    }
}
