using HotelBooking.Exceptions;
using HotelBooking.Interfaces;
using HotelBooking.Models;

namespace HotelBooking.Services
{
    public class UserServices : IUserServices
    {
        //REPOSITORY INITIALIZATION
        private readonly IRepository<int, User> _userRepo;
        private readonly ILogger<UserServices> _logger;

        //DEPENDENCY INJECTION
        public UserServices(IRepository<int, User> userRepo,ILogger<UserServices> logger)
        {
            _userRepo = userRepo;
            _logger = logger;
        }

        public async Task<User> DeactivateUser(int UserId)
        {
            try
            {
                var user = await _userRepo.Get(UserId);
                user.IsActivated = false;
                return await _userRepo.Update(user);
            }
            catch(NoSuchUserException ex)
            {
                throw ex;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public async Task<bool> IsActivated(int UserId)
        {
            try
            {
                var user = await _userRepo.Get(UserId);
                if (!user.IsActivated)
                {
                    return false;
                }
                return true;
            }
            catch (NoSuchUserException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //GET USER BY ID
        public async Task<User> GetUserById(int UserId)
        {
            try
            {
                var user = await _userRepo.Get(UserId);
                return user;
            }
            catch (NoSuchUserException ex)
            {
                _logger.LogError(ex, "No User found");
                throw new NoSuchUserException(ex.Message);
            }
        }
    }
}
