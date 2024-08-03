using HotelBooking.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using RatingServices.Interfaces;
using RatingServices.Models;
using RatingServices.Models.DTOs;
using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;

namespace RatingServices.Controllers
{
    [Route("api/")]
    [ApiController]
    [EnableCors("MyCors")]
    [ExcludeFromCodeCoverage]
    public class RatingController : ControllerBase
    {
        private readonly IRatingService _ratingService;
        private readonly ILogger<RatingController> _logger;

        public RatingController(IRatingService ratingService, ILogger<RatingController> logger)
        {
            _ratingService = ratingService;
            _logger = logger;
        }

        // Add a rating
        [Authorize]
        [HttpPost("AddRating")]
        [ProducesResponseType(typeof(Rating), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddRating([FromBody] RatingInputDTO ratingDTO)
        {
            try
            {
                // You may need to get the current user ID from the context or authentication token
                int userId = GetCurrentUserId();

                var rating = await _ratingService.AddRatingAsync(ratingDTO, userId);
                return Ok(rating);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while adding the rating");
                return BadRequest(new ErrorModel(400, ex.Message));
            }
        }

        // Update a rating
        [Authorize]
        [HttpPut("updateRating/{ratingId}")]
        [ProducesResponseType(typeof(Rating), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateRating(int ratingId, [FromBody] RatingUpdateDTO ratingDTO)
        {
            try
            {
                int userId = GetCurrentUserId();
                await _ratingService.UpdateRatingAsync(ratingId, ratingDTO, userId);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating the rating");
                return BadRequest(new ErrorModel(400, ex.Message));
            }
        }

        // Delete a rating
        [Authorize]
        [HttpDelete("DeleteRating/{ratingId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteRating(int ratingId)
        {
            try
            {
                int userId = GetCurrentUserId();
                var result = await _ratingService.DeleteRatingAsync(ratingId, userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting the rating");
                return NotFound(new ErrorModel(404, ex.Message));
            }
        }

        // Get all ratings
        [Authorize]
        [HttpGet("GetAllRating")]
        [ProducesResponseType(typeof(IEnumerable<Rating>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<Rating>>> GetAllRatings()
        {
            try
            {
                var ratings = await _ratingService.GetAllRatingsAsync();
                return Ok(ratings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving ratings");
                return StatusCode(StatusCodes.Status500InternalServerError, new ErrorModel(500, ex.Message));
            }
        }

        // Get a rating by ID
        [Authorize]
        [HttpGet("GetRatingsByID/{ratingId}")]
        [ProducesResponseType(typeof(Rating), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Rating>> GetRatingById(int ratingId)
        {
            try
            {
                var rating = await _ratingService.GetRatingByIdAsync(ratingId);
                if (rating == null)
                {
                    return NotFound(new ErrorModel(404, "Rating not found"));
                }
                return Ok(rating);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving the rating");
                return StatusCode(StatusCodes.Status500InternalServerError, new ErrorModel(500, ex.Message));
            }
        }

        [Authorize]
        [HttpGet("GetRatingByHotelID/{hotelID}")]
        [ProducesResponseType(typeof(Rating), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Rating>> GetRatingByHotelID(int hotelID)
        {
            try
            {
                var rating = await _ratingService.GetRatingsByHotelIdAsync(hotelID);
                if (rating == null)
                {
                    return NotFound(new ErrorModel(404, "Rating not found"));
                }
                return Ok(rating);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving the rating");
                return StatusCode(StatusCodes.Status500InternalServerError, new ErrorModel(500, ex.Message));
            }
        }

        private int GetCurrentUserId()
        {
            int userId = Convert.ToInt32(User.FindFirstValue(ClaimTypes.Name));
            return userId;
        }
    }
}
