using HotelBooking.Models.DTOs;
using HotelBooking.Models;
using Microsoft.AspNetCore.Mvc;
using HotelBooking.Interfaces;
using Microsoft.AspNetCore.Cors;
using HotelBooking.Exceptions;

namespace HotelBooking.Controllers
{
    [Route("/")]
    [ApiController]
    [EnableCors("MyCors")]
    public class UserController :ControllerBase
    {
        private readonly IUserLoginAndRegisterServices _userService;
        private readonly IUserServices _userServices2;
        private readonly ILogger<UserController> _logger;

        public UserController(IUserLoginAndRegisterServices userService,
                ILogger<UserController> logger,
                IUserServices userServices2)
        {
            _userService = userService;
            _logger = logger;
            _userServices2 = userServices2;
        }


        [HttpPost("Login")]
        [ProducesResponseType(typeof(User), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<User>> Login(UserLoginDTO userLoginDTO)
        {
            try
            {
                var result = await _userService.Login(userLoginDTO);
                _logger.LogInformation("Login successful for user: {UserID}", userLoginDTO.UserId);
                return Ok(result);
            }
            catch(UnauthorizedUserException ex)
            {
                _logger.LogError(ex, "Login failed for user: {UserID}", userLoginDTO.UserId);
                return Unauthorized(new ErrorModel(401, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while Logging in");
                return StatusCode(500, new ErrorModel(500, $"An error occurred while processing your request. + {ex.Message}"));
            }

        }

        [HttpPost("Register")]
        [ProducesResponseType(typeof(RegisterReturnDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<RegisterReturnDTO>> Register(UserRegisterInputDTO userDTO)
        {
            try
            {
                RegisterReturnDTO result = await _userService.Register(userDTO);
                _logger.LogInformation("Registration successful for user: {UserId}", result.Id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Registration failed for user");
                return BadRequest(new ErrorModel(501, ex.Message));
            }
        }

        [HttpGet("getuser/{userId}")]
        [ProducesResponseType(typeof(User), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<User>> GetUserById(string userId)
        {
            try
            {
                User result = await _userServices2.GetUserById(Convert.ToInt32(userId));
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new ErrorModel(501, ex.Message));
            }
        }

        [HttpPost("DeactivateUser/{userId}")]
        [ProducesResponseType(typeof(User), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<User>> DeactivateUser(int userId)
        {
            try
            {
                User result = await _userServices2.DeactivateUser(Convert.ToInt32(userId));
                return Ok(result);
            }
            catch(NoSuchUserException ex)
            {
                _logger.LogError(ex, "Deactivation failed for user: {UserID} because of invalid UserID", userId);
                return Unauthorized(new ErrorModel(401, ex.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(new ErrorModel(501, ex.Message));
            }
        }

        [HttpPost("IsActive/{userId}")]
        [ProducesResponseType(typeof(User), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<bool>> IsActive(int userId)
        {
            try
            {
                var result = await _userServices2.IsActivated(Convert.ToInt32(userId));
                return Ok(result);
            }
            catch (NoSuchUserException ex)
            {
                _logger.LogError(ex, "Deactivation failed for user: {UserID} because of invalid UserID", userId);
                return Unauthorized(new ErrorModel(401, ex.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(new ErrorModel(501, ex.Message));
            }
        }

    }
}
