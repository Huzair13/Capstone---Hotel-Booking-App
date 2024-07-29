using HotelBooking.Models.DTOs;
using HotelBooking.Models;
using Microsoft.AspNetCore.Mvc;
using HotelBooking.Interfaces;
using Microsoft.AspNetCore.Cors;
using HotelBooking.Exceptions;
using AuthenticationServices.Models.DTOs;
using System.Security.Claims;
using AuthenticationServices.Models;
using Microsoft.AspNetCore.Authorization;

namespace HotelBooking.Controllers
{
    [Route("/")]
    [ApiController]
    [EnableCors("MyCors")]
    public class UserController :ControllerBase
    {
        //INITIALIZATION
        private readonly IUserLoginAndRegisterServices _userService;
        private readonly IUserServices _userServices2;
        private readonly ILogger<UserController> _logger;

        //DEPENDENCY INJECTION
        public UserController(IUserLoginAndRegisterServices userService,
                ILogger<UserController> logger,
                IUserServices userServices2)
        {
            _userService = userService;
            _logger = logger;
            _userServices2 = userServices2;
        }

        //USER LOGIN
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
                return BadRequest(new ErrorModel(500, ex.Message));
            }
        }

        [Authorize]
        [HttpGet("getuser/{userId}")]
        [ProducesResponseType(typeof(User), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<User>> GetUserById(string userId)
        {
            try
            {
                User result = await _userServices2.GetUserById(Convert.ToInt32(userId));
                _logger.LogInformation("getuser successful for user: {UserId}", result.Id);
                return Ok(result);
            }
            catch (NoSuchUserException ex)
            {
                _logger.LogInformation("NoSuchUserException at getuser for {userId}",userId);
                return NotFound(new ErrorModel(404, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogInformation("An Error Occured At getuser");
                return BadRequest(new ErrorModel(500, ex.Message));
            }
        }

        [Authorize]
        [HttpPost("DeactivateUser/{userId}")]
        [ProducesResponseType(typeof(User), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<User>> DeactivateUser(int userId)
        {
            try
            {
                User result = await _userServices2.DeactivateUser(Convert.ToInt32(userId));
                _logger.LogInformation("DeactivateUser successful for user: {UserId}", result.Id);
                return Ok(result);
            }
            catch(NoSuchUserException ex)
            {
                _logger.LogError(ex, "Deactivation failed for user: {UserID} because of invalid UserID", userId);
                return Unauthorized(new ErrorModel(401, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An Error Occured At DeactivateUser for {UserID}", userId);
                return BadRequest(new ErrorModel(501, ex.Message));
            }
        }

        [Authorize]
        [HttpPost("AddUserCoins")]
        [ProducesResponseType(typeof(User), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<User>> AddUserCoins([FromBody] UpdateCoinsDTO updateCoinsDTO)
        {
            try
            {
                User result = await _userServices2.AddUserCoins(updateCoinsDTO);
                _logger.LogInformation("Add User Coins successful for user: {UserId}", result.Id);
                return Ok(result);
            }
            catch (NoSuchUserException ex)
            {
                _logger.LogError(ex, "Add Coins failed for user: {UserID} because of invalid UserID", updateCoinsDTO.UserId);
                return Unauthorized(new ErrorModel(401, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An Error Occured At AddUserCoins");
                return BadRequest(new ErrorModel(501, ex.Message));
            }
        }

        [Authorize]
        [HttpPost("ReduceUserCoins")]
        [ProducesResponseType(typeof(User), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<User>> ReduceUserCoins([FromBody] UpdateCoinsDTO updateCoinsDTO)
        {
            try
            {
                User result = await _userServices2.ReduceUserCoins(updateCoinsDTO);
                return Ok(result);
            }
            catch (NoSuchUserException ex)
            {
                _logger.LogError(ex, "Update Coins failed for user: {UserID} because of invalid UserID", updateCoinsDTO.UserId);
                return Unauthorized(new ErrorModel(401, ex.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(new ErrorModel(501, ex.Message));
            }
        }

        [Authorize]
        [HttpPost("UpdateUserCoins")]
        [ProducesResponseType(typeof(User), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<User>> UpdateUserCoins([FromBody] UpdateCoinsDTO updateCoinsDTO)
        {
            try
            {
                User result = await _userServices2.UpdateUserCoins(updateCoinsDTO);
                return Ok(result);
            }
            catch (NoSuchUserException ex)
            {
                _logger.LogError(ex, "Update Coins failed for user: {UserID} because of invalid UserID", updateCoinsDTO.UserId);
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

        [Authorize(Roles ="Guest")]
        [HttpPost("RequestForActivation")]
        [ProducesResponseType(typeof(Request), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Request>> RequestForActivation([FromBody]RequestInputDTO requestInputDTO)
        {
            int userId = Convert.ToInt32(User.FindFirstValue(ClaimTypes.Name));
            try
            {
                RequestDTO requestDTO = new RequestDTO
                {
                    userId = userId,
                    Reason = requestInputDTO.Reason
                };
                var result = await _userServices2.RequestForActivation(requestDTO);
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

        [Authorize(Roles = "Admin")]
        [HttpPut("RejectRequest")]
        [ProducesResponseType(typeof(Request), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Request>> RejectRequest(int requestId)
        {
            int userId = Convert.ToInt32(User.FindFirstValue(ClaimTypes.Name));
            try
            {
                var result = await _userServices2.RejectRequest(requestId);
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

        [Authorize(Roles = "Admin")]
        [HttpPut("AcceptRequest")]
        [ProducesResponseType(typeof(Request), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Request>> AcceptRequest(int requestId)
        {
            int userId = Convert.ToInt32(User.FindFirstValue(ClaimTypes.Name));
            try
            {
                var result = await _userServices2.AcceptRequest(requestId);
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
