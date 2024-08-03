using AuthenticationServices.Models;
using AuthenticationServices.Models.DTOs;
using HotelBooking.Models;

namespace HotelBooking.Interfaces
{
    public interface IUserServices
    {
        public Task<User> GetUserById(int UserId);
        public Task<User> DeactivateUser(int UserId);
        public Task<bool> IsActivated(int UserId);
        public Task<User> UpdateUserCoins(UpdateCoinsDTO updateCoinsDTO);
        public Task<User> AddUserCoins(UpdateCoinsDTO updateCoinsDTO);
        public Task<User> ReduceUserCoins(UpdateCoinsDTO updateCoinsDTO);
        public Task<Request> RejectRequest(int requestId);
        public Task<Request> AcceptRequest(int requestId);
        public Task<Request> RequestForActivation(RequestDTO requestDTO);
        public Task<IEnumerable<Request>> GetAllRequest();
        public Task<User> ChangeUserRole(int userId, string role);

    }
}
