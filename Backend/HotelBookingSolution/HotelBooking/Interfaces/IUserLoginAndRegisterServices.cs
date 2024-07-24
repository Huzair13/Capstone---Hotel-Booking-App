using HotelBooking.Models.DTOs;

namespace HotelBooking.Interfaces
{
    public interface IUserLoginAndRegisterServices
    {
        public Task<LoginReturnDTO> Login(UserLoginDTO loginDTO);
        public Task<RegisterReturnDTO> Register(UserRegisterInputDTO userInputDTO);
    }
}
