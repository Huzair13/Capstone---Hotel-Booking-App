using BookingServices.Models.DTOs;
using HotelBooking.Models;
using HotelServices.Exceptions;
using HotelServices.Interfaces;
using HotelServices.Models;
using HotelServices.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;

namespace HotelServices.Controllers
{
    [Route("api/")]
    [ApiController]
    [EnableCors("MyCors")]
    public class HotelController : ControllerBase
    {
        private readonly IHotelServices _hotelServices;
        private readonly IAzureBlobService _blobService;
        private readonly ILogger<HotelController> _logger;

        public HotelController(IHotelServices hotelServices, ILogger<HotelController> logger, IAzureBlobService blobService)
        {
            _hotelServices = hotelServices;
            _logger = logger;
            _blobService = blobService;
        }

        // Add Hotel
        [Authorize(Roles ="Admin")]
        [HttpPost("AddHotel")]
        [ProducesResponseType(typeof(HotelReturnDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<HotelReturnDTO>> AddHotel([FromForm] HotelInputDTO hotelInputDTO, IFormFileCollection files)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var userId = User.FindFirstValue(ClaimTypes.Name);

                    var imageUrls = new List<string>();
                    foreach (var file in files)
                    {
                        if (file.Length > 0)
                        {
                            using (var stream = file.OpenReadStream())
                            {
                                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                                var imageUrl = await _blobService.UploadAsync(stream, fileName);
                                imageUrls.Add(imageUrl);
                            }
                        }
                    }

                    HotelDTO hotelDTO = new HotelDTO()
                    {
                        Name = hotelInputDTO.Name,
                        Description = hotelInputDTO.Description,
                        State = hotelInputDTO.State,
                        City = hotelInputDTO.City,
                        Address = hotelInputDTO.Address,
                        AverageRatings = 0,
                        NumOfRooms = 0,
                        Type = hotelInputDTO.Type,
                        HotelImages = imageUrls
                    };

                    var result = await _hotelServices.AddHotelAsync(hotelDTO);
                    return Ok(result);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while adding the hotel.");
                    return StatusCode(500, new ErrorModel(500, "An error occurred while processing your request."));
                }
            }
            return BadRequest("All Details are not provided");
        }



        // Get Hotel Details
        [Authorize]
        [HttpGet("GetAllHotelDetails")]
        [ProducesResponseType(typeof(IEnumerable<HotelDetailsDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<HotelDetailsDTO>>> GetAllHotelDetails()
        {
            try
            {
                var result = await _hotelServices.GetAllHotelsRoomsAndAmenitiesAsync();
                return Ok(result);
            }
            catch (NoSuchHotelException ex)
            {
                _logger.LogError(ex, "Details Not found");
                return NotFound(new ErrorModel(404, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving all details.");
                return StatusCode(500, new ErrorModel(500, "An error occurred while processing your request."));
            }
        }

        // Get All Hotels
        [Authorize]
        [HttpGet("GetAllHotels")]
        [ProducesResponseType(typeof(List<HotelReturnDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<HotelReturnDTO>>> GetAllHotels()
        {
            try
            {
                var result = await _hotelServices.GetAllHotels();
                return Ok(result);
            }
            catch(NoSuchHotelException ex)
            {
                _logger.LogError(ex, "Hotels Not found");
                return NotFound(new ErrorModel(404, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving all hotels.");
                return StatusCode(500, new ErrorModel(500, "An error occurred while processing your request."));
            }
        }

        // Get All Available hotels by date
        [Authorize]
        [HttpGet("GetAllAvailableHotelsByDate")]
        [ProducesResponseType(typeof(List<HotelReturnDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<HotelReturnDTO>>> GetAllAvailableHotelsByDate(DateTime checkInDate,DateTime checkOutDate)
        {
            try
            {
                var result = await _hotelServices.GetAvailableHotelsByDateAsync(checkInDate,checkOutDate);
                return Ok(result);
            }
            catch (NoSuchHotelException ex)
            {
                _logger.LogError(ex, "Hotels Not found");
                return NotFound(new ErrorModel(404, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving all hotels.");
                return StatusCode(500, new ErrorModel(500, "An error occurred while processing your request."));
            }
        }


        //UPDATE HOTEL AVERAGE RATING
        [Authorize]
        [HttpPut("UpdateAverageRatings")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> UpdateAverageRatings(HotelUpdateDTO hotelUpdateDTO)
        {
            try
            {
                await _hotelServices.UpdateHotelAverageRatingAsync(hotelUpdateDTO);
                return Ok();
            }
            catch (NoSuchHotelException ex)
            {
                _logger.LogError(ex, "Hotels Not found");
                return NotFound(new ErrorModel(404, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving all hotels.");
                return StatusCode(500, new ErrorModel(500, "An error occurred while processing your request."));
            }
        }

        // Get All Hotels
        [Authorize]
        [HttpPost("CheckHotelAvailability")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<bool>> CheckHotelAvailability([FromBody]BestCombinationDTO bestCombinationDTO)
        {
            try
            {
                var result = await _hotelServices.CheckAvailabilityAsync(bestCombinationDTO);
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

        // Get All Hotels
        [Authorize]
        [HttpPost("BestRoomCombination")]
        [ProducesResponseType(typeof(List<Room>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<Room>>> BestRoomCombination(BestCombinationDTO bestCombinationDTO)
        {
            try
            {
                var result = await _hotelServices.BestAvailableCombinationAsync(bestCombinationDTO);
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

        // Get Hotel by ID
        [Authorize]
        [HttpGet("GetHotelByID/{hotelId}")]
        [ProducesResponseType(typeof(HotelReturnDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<HotelReturnDTO>> GetHotelByID(int hotelId)
        {
            try
            {
                var result = await _hotelServices.GetHotelByID(hotelId);
                return Ok(result);
            }
            catch (NoSuchHotelException ex)
            {
                _logger.LogError(ex, $"Hotels Not found for the hotel ID : {hotelId}");
                return NotFound(new ErrorModel(404, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving the hotel by ID.");
                return StatusCode(500, new ErrorModel(500, "An error occurred while processing your request."));
            }
        }

        // Get Hotel by ID
        [Authorize]
        [HttpGet("GetHotelCity")]
        [ProducesResponseType(typeof(IList<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IList<string>>> GetHotelCity()
        {
            try
            {
                var result = await _hotelServices.GetCities();
                return Ok(result);
            }
            catch (NoSuchHotelException ex)
            {
                _logger.LogError(ex, $"cities Not found ");
                return NotFound(new ErrorModel(404, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving the cities.");
                return StatusCode(500, new ErrorModel(500, $"An error occurred while processing your request {ex.Message}"));
            }
        }

        // Get Hotel by Name
        [Authorize]
        [HttpGet("GetHotelByName/{hotelName}")]
        [ProducesResponseType(typeof(HotelReturnDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<HotelReturnDTO>> GetHotelByName(string hotelName)
        {
            try
            {
                var result = await _hotelServices.GetHotelByName(hotelName);
                return Ok(result);
            }
            catch (NoSuchHotelException ex)
            {
                _logger.LogError(ex, $"Hotels Not found for the hotel Name : {hotelName}");
                return NotFound(new ErrorModel(404, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving the hotel by name.");
                return StatusCode(500, new ErrorModel(500, "An error occurred while processing your request."));
            }
        }

        // Update Hotel
        [Authorize]
        [HttpPut("UpdateHotel")]
        [ProducesResponseType(typeof(HotelReturnDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<HotelReturnDTO>> UpdateHotel([FromBody] HotelUpdateDataDTO hotelUpdateDataDTO)
        {
            try
            {
                var result = await _hotelServices.UpdateHotelAsync(hotelUpdateDataDTO);
                return Ok(result);
            }
            catch (NoSuchHotelException ex)
            {
                _logger.LogError(ex, $"Hotels Not found for the hotel ID : {hotelUpdateDataDTO.HotelId}");
                return NotFound(new ErrorModel(404, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving the hotel by name.");
                return StatusCode(500, new ErrorModel(500, $"An error occurred while processing your request {ex.Message}"));
            }
        }

        // Update Hotel
        [Authorize]
        [HttpDelete("DeleteHotel")]
        [ProducesResponseType(typeof(HotelReturnDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<HotelReturnDTO>> DeleteHotel([FromBody] int hotelId)
        {
            try
            {
                var result = await _hotelServices.DeleteHotelAsync(hotelId);
                return Ok(result);
            }
            catch (NoSuchHotelException ex)
            {
                _logger.LogError(ex, $"Hotels Not found for the hotel ID : {hotelId}");
                return NotFound(new ErrorModel(404, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving the hotel by name.");
                return StatusCode(500, new ErrorModel(500, $"An error occurred while processing your request {ex.Message}"));
            }
        }


        //ADD IMAGES TO THE HOTEL
        [Authorize(Roles = "Admin")]
        [HttpPost("AddImagesToHotel/{hotelId}")]
        [ProducesResponseType(typeof(HotelReturnDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<HotelReturnDTO>> AddImagesToHotel(int hotelId, [FromForm] IFormFileCollection files)
        {
            if (files == null || files.Count == 0)
            {
                return BadRequest("No files were uploaded.");
            }

            try
            {
                var imageUrls = new List<string>();

                foreach (var file in files)
                {
                    if (file.Length > 0)
                    {
                        using (var stream = file.OpenReadStream())
                        {
                            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                            var imageUrl = await _blobService.UploadAsync(stream, fileName);
                            imageUrls.Add(imageUrl);
                        }
                    }
                }

                // Create DTO for the service call
                var addImageDTO = new AddImageDTO
                {
                    hotelId = hotelId,
                    imageUrls = imageUrls
                };

                // Call service method
                var updatedHotel = await _hotelServices.AddImagesToHotelAsync(addImageDTO);
                return Ok(updatedHotel);
            }
            catch (NoSuchHotelException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while adding images to the hotel.");
                return StatusCode(500, new ErrorModel(500, "An error occurred while processing your request."));
            }
        }

    }
}
