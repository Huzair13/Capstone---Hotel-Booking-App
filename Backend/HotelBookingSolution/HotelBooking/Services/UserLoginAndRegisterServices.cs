using HotelBooking.Exceptions;
using HotelBooking.Interfaces;
using HotelBooking.Models.DTOs;
using HotelBooking.Models;
using System.Security.Cryptography;
using System.Text;

namespace HotelBooking.Services
{
    public class UserLoginAndRegisterServices : IUserLoginAndRegisterServices
    {

        //INITIALIZATION
        private readonly IRepository<int, UserDetails> _userDetailsRepo;
        private readonly IRepository<int, User> _userRepo;
        private readonly ITokenServices _tokenServices;
        private readonly ILogger<UserLoginAndRegisterServices> _logger;

        //DEPENDENCY INJECTION
        public UserLoginAndRegisterServices(IRepository<int, UserDetails> userDetailsRepo, 
                                            IRepository<int, User> userRepo, 
                                            ITokenServices tokenServices, ILogger<UserLoginAndRegisterServices> logger)
        {
            _userRepo = userRepo;
            _userDetailsRepo = userDetailsRepo;
            _tokenServices = tokenServices;
            _logger = logger;
        }

        //LOGIN 
        public async Task<LoginReturnDTO> Login(UserLoginDTO loginDTO)
        {
            try
            {
                var userDB = await _userDetailsRepo.Get(loginDTO.UserId);
                HMACSHA512 hMACSHA = new HMACSHA512(userDB.PasswordHashKey);
                var encrypterPass = hMACSHA.ComputeHash(Encoding.UTF8.GetBytes(loginDTO.Password));
                bool isPasswordSame = ComparePassword(encrypterPass, userDB.Password);
                if (isPasswordSame)
                {
                    var user = await _userRepo.Get(loginDTO.UserId);
                    LoginReturnDTO loginReturnDTO = await MapUserToLoginReturn(user);
                    return loginReturnDTO;
                }
                throw new UnauthorizedUserException("Invalid username or password");
            }
            catch (UnauthorizedUserException ex)
            {
                throw ex;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        //COMPARE PASSWORD (LOGIN)
        private bool ComparePassword(byte[] encrypterPass, byte[] password)
        {
            for (int i = 0; i < encrypterPass.Length; i++)
            {
                if (encrypterPass[i] != password[i])
                {
                    return false;
                }
            }
            return true;
        }

        //REGISTER

        public async Task<RegisterReturnDTO> Register(UserRegisterInputDTO userInputDTO)
        {
            User user = null;
            UserDetails userDetail = null;
            try
            {
                user = await MapUserDTOToUser(userInputDTO);
                userDetail = await MapUserDTOToUserDetails(userInputDTO);
                var existingUsers = await _userRepo.Get();
                var result = existingUsers.FirstOrDefault(u => u.Email == userInputDTO.Email);
                var resultPhone = existingUsers.FirstOrDefault(u => u.MobileNumber == userInputDTO.MobileNumber);
                if (result != null || resultPhone != null)
                {
                    throw new UserAlreadyExistsException();
                }
                user = await _userRepo.Add(user);
                userDetail.UserId = user.Id;
                userDetail = await _userDetailsRepo.Add(userDetail);
                RegisterReturnDTO registerReturnDTO = await MapUserToRegisterReturnDTO(user);
                return registerReturnDTO;
            }
            catch (UserAlreadyExistsException ex)
            {
                _logger.LogError(ex, "User Already Exists Error at Student Register service");
                throw new UserAlreadyExistsException(ex.Message);
            }
            catch (Exception)
            {

            }
            await RevertAction(user, userDetail);
            throw new UnableToRegisterException("Not able to register at this moment");
        }


        //REVERT ACTION
        private async Task RevertAction(User? user, UserDetails? userDetail)
        {
            if (user != null)
                await RevertUserInsert(user);
            if (userDetail != null && user == null)
                await RevertUserDetailsInsert(userDetail);
        }

        //MAP USER TO REGISTER RETURN DTO
        private async Task<RegisterReturnDTO> MapUserToRegisterReturnDTO(User user)
        {
            RegisterReturnDTO userReturn = new RegisterReturnDTO();
            userReturn.Id = user.Id;
            userReturn.Name = user.Name;
            userReturn.MobileNumber = user.MobileNumber;
            userReturn.Email = user.Email;
            userReturn.DateOfBirth = user.DateOfBirth;
            userReturn.Role = user.UserType;
            return userReturn;
        }

        //MAP USER DTO TO USER DETAILS
        private async Task<UserDetails> MapUserDTOToUserDetails(UserRegisterInputDTO userInputDTO)
        {
            UserDetails userDetails = new UserDetails();
            HMACSHA512 hMACSHA = new HMACSHA512();

            userDetails.PasswordHashKey = hMACSHA.Key;
            userDetails.Password = hMACSHA.ComputeHash(Encoding.UTF8.GetBytes(userInputDTO.Password));

            return userDetails;
        }

        //REVERT --- USERDETAILS
        private async Task RevertUserDetailsInsert(UserDetails userDetails)
        {
            await _userDetailsRepo.Delete(userDetails.UserId);
        }

        //REVERT - USER 
        private async Task RevertUserInsert(User user)
        {
            await _userRepo.Delete(user.Id);
        }

        // MAPPPERS

        //MAP USER TO LOGIN RETURN DTO
        private async Task<LoginReturnDTO> MapUserToLoginReturn(User user)
        {
            LoginReturnDTO returnDTO = new LoginReturnDTO
            {
                userID = user.Id,
                Role = user.UserType,
                Token = await _tokenServices.GenerateToken(user)
            };
            return returnDTO;
        }


        //MAP USER DTO TO USER
        private async Task<User> MapUserDTOToUser(UserRegisterInputDTO userDTO)
        {
            User user = new User();
            user.MobileNumber = userDTO.MobileNumber;
            user.Name = userDTO.Name;
            user.Email = userDTO.Email;
            user.DateOfBirth = userDTO.DateOfBirth;
            user.Gender = userDTO.Gender;
            user.UserType = userDTO.UserType;

            return user;
        }
    }
}
