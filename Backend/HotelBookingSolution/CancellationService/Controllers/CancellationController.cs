using CancellationService.Interfaces;
using CancellationService.Models.DTOs;
using HotelBooking.Models;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CancellationService.Controllers
{
    [Route("api/")]
    [ApiController]
    [EnableCors("MyCors")]
    public class CancellationController :ControllerBase
    {
        private readonly ICancelService _cancelService;
        private readonly ILogger<CancellationController> _logger;

        public CancellationController(ICancelService cancelService, ILogger<CancellationController> logger)
        {
            _cancelService = cancelService;
            _logger = logger;
        }

        [HttpPost("CancelBooking/{bookingID}")]
        [ProducesResponseType(typeof(CancelReturnDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<CancelReturnDTO>> CancelBooking(int bookingID)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var userId = Convert.ToInt32(User.FindFirstValue(ClaimTypes.Name));
                    var result = await _cancelService.CancelTheBooking(bookingID,userId);
                    return Ok(result);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while adding the booking.");
                    return StatusCode(500, new ErrorModel(500, $"An error occurred while processing your request. {ex.Message}"));
                }
            }
            return BadRequest("All Details are not provided");
        }
    }
}
