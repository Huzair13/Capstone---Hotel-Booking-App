using AuthenticationServices.Exceptions;
using AuthenticationServices.Models;
using AuthenticationServices.Models.DTOs;
using HotelBooking.Exceptions;
using HotelBooking.Interfaces;
using HotelBooking.Models;

namespace HotelBooking.Services
{
    public class UserServices : IUserServices
    {
        //REPOSITORY INITIALIZATION
        private readonly IRepository<int, User> _userRepo;
        private readonly IRepository<int, Request> _requestRepo;
        private readonly ILogger<UserServices> _logger;

        //DEPENDENCY INJECTION
        public UserServices(IRepository<int, User> userRepo,ILogger<UserServices> logger,IRepository<int,Request> requestRepo)
        {
            _userRepo = userRepo;
            _logger = logger;
            _requestRepo = requestRepo;
        }

        //DEACTIVATE USER
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

        //UPDATE USER COINS
        public async Task<User> UpdateUserCoins(UpdateCoinsDTO updateCoinsDTO)
        {
            try
            {
                var exisitingUser = await _userRepo.Get(updateCoinsDTO.UserId);
                exisitingUser.CoinsEarned = updateCoinsDTO.CoinsEarned;
                var result = await _userRepo.Update(exisitingUser);
                return result;
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


        //ADD USER COINS
        public async Task<User> AddUserCoins(UpdateCoinsDTO updateCoinsDTO)
        {
            try
            {
                var exisitingUser = await _userRepo.Get(updateCoinsDTO.UserId);
                exisitingUser.CoinsEarned += updateCoinsDTO.CoinsEarned;
                var result = await _userRepo.Update(exisitingUser);
                return result;
            }
            catch (NoSuchUserException ex)
            {
                throw ex;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        //REDUCE USER COINS

        public async Task<User> ReduceUserCoins(UpdateCoinsDTO updateCoinsDTO)
        {
            try
            {
                var exisitingUser = await _userRepo.Get(updateCoinsDTO.UserId);
                exisitingUser.CoinsEarned -= updateCoinsDTO.CoinsEarned;
                var result = await _userRepo.Update(exisitingUser);
                return result;
            }
            catch (NoSuchUserException ex)
            {
                throw ex;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }


        //REQUEST FOR ACTIVATION
        public async Task<Request> RequestForActivation(RequestDTO requestDTO)
        {
            try
            {
                Request request = new Request
                {
                    userId = requestDTO.userId,
                    Reason = requestDTO.Reason,
                    Status = "Requested"
                };
                var result = await _requestRepo.Add(request);
                return result;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }


        //ACCPET THE REQUEST
        public async Task<Request> AcceptRequest(int requestId)
        {
            try
            {
                var request = await _requestRepo.Get(requestId);
                request.Status = "Accepted";

                var user = await _userRepo.Get(request.userId);
                user.IsActivated = true;
                var userUpdateResult = await _userRepo.Update(user);

                var requestUpdateResult =await _requestRepo.Update(request);
                return requestUpdateResult;
            }
            catch(NoSuchUserException ex)
            {
                throw new NoSuchUserException(ex.Message);
            }
            catch(NoSuchRequestException ex)
            {
                throw new NoSuchRequestException(ex.Message);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //REJECT THE REQUEST
        public async Task<Request> RejectRequest(int requestId)
        {
            try
            {
                var request = await _requestRepo.Get(requestId);
                request.Status = "Rejected";
                var requestUpdateResult = await _requestRepo.Update(request);
                return requestUpdateResult;
            }
            catch(NoSuchRequestException ex)
            {
                throw new NoSuchRequestException(ex.Message);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        //CHECK IF USER IS ACTIVATED
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
                throw new NoSuchUserException(ex.Message);
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
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public async Task<IEnumerable<Request>> GetAllRequest()
        {
            try
            {
                var requests =await _requestRepo.Get();
                return requests;
            }
            catch(NoSuchRequestException ex)
            {
                throw ex;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public async Task<User> ChangeUserRole(int userId,string role)
        {
            try
            {

                var user = await _userRepo.Get(userId);
                user.UserType = role;
                var updatedUser = await _userRepo.Update(user);
                return updatedUser;
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
    }
}
